namespace FsBump.WorldGeneration

open Microsoft.Xna.Framework

/// Represents a map of zones for world generation
type ZoneMap = {
  /// 2D array of zone types across the world
  Zones: ZoneType[,]
  /// Array of zone transitions found in the world
  Transitions: ZoneTransition array
}

[<RequireQualifiedAccess>]
module Zone =

  let private lowlandCharacteristics: ZoneCharacteristics = {
    HeightRange = 2.0f, 4.0f
    TerrainSmoothness = 0.9f
    PlatformDensity = 0.85f
    GapFrequency = 0.05f
    ConnectivityTarget = 0.90f
    VerticalMix = {
      Gentle = 0.70f
      Moderate = 0.20f
      Vertical = 0.10f
    }
  }

  let private midlandCharacteristics: ZoneCharacteristics = {
    HeightRange = 4.0f, 7.0f
    TerrainSmoothness = 0.6f
    PlatformDensity = 0.65f
    GapFrequency = 0.15f
    ConnectivityTarget = 0.70f
    VerticalMix = {
      Gentle = 0.30f
      Moderate = 0.50f
      Vertical = 0.20f
    }
  }

  let private highlandCharacteristics: ZoneCharacteristics = {
    HeightRange = 7.0f, 12.0f
    TerrainSmoothness = 0.3f
    PlatformDensity = 0.45f
    GapFrequency = 0.30f
    ConnectivityTarget = 0.50f
    VerticalMix = {
      Gentle = 0.10f
      Moderate = 0.30f
      Vertical = 0.60f
    }
  }

  /// Gets the characteristics for a given zone type
  /// <param name="zone">The zone type to get characteristics for</param>
  /// <returns>The zone characteristics with terrain generation parameters</returns>
  let rec getCharacteristics(zone: ZoneType) : ZoneCharacteristics =
    match zone with
    | Lowland -> lowlandCharacteristics
    | Midland -> midlandCharacteristics
    | Highland -> highlandCharacteristics
    | Transition transition ->
      let fromChars = getCharacteristics transition.From
      let toChars = getCharacteristics transition.To

      {
        HeightRange =
          let struct (fromMin, fromMax) = fromChars.HeightRange
          let struct (toMin, toMax) = toChars.HeightRange

          MathHelper.Lerp(fromMin, toMin, transition.BlendFactor),
          MathHelper.Lerp(fromMax, toMax, transition.BlendFactor)
        TerrainSmoothness =
          MathHelper.Lerp(
            fromChars.TerrainSmoothness,
            toChars.TerrainSmoothness,
            transition.BlendFactor
          )
        PlatformDensity =
          MathHelper.Lerp(
            fromChars.PlatformDensity,
            toChars.PlatformDensity,
            transition.BlendFactor
          )
        GapFrequency =
          MathHelper.Lerp(
            fromChars.GapFrequency,
            toChars.GapFrequency,
            transition.BlendFactor
          )
        ConnectivityTarget =
          MathHelper.Lerp(
            fromChars.ConnectivityTarget,
            toChars.ConnectivityTarget,
            transition.BlendFactor
          )
        VerticalMix = {
          Gentle =
            MathHelper.Lerp(
              fromChars.VerticalMix.Gentle,
              toChars.VerticalMix.Gentle,
              transition.BlendFactor
            )
          Moderate =
            MathHelper.Lerp(
              fromChars.VerticalMix.Moderate,
              toChars.VerticalMix.Moderate,
              transition.BlendFactor
            )
          Vertical =
            MathHelper.Lerp(
              fromChars.VerticalMix.Vertical,
              toChars.VerticalMix.Vertical,
              transition.BlendFactor
            )
        }
      }

  /// Determines the zone type at a specific world position using noise generation
  /// <param name="worldSeed">The seed for world generation</param>
  /// <param name="position">The world position to check</param>
  /// <param name="noiseGenerator">The noise generator to use</param>
  /// <returns>The zone type at the specified position</returns>
  let getZoneAtPosition
    (worldSeed: int)
    (position: Vector3)
    (noiseGenerator: INoiseGenerator)
    : ZoneType =
    let biomeNoise =
      noiseGenerator.OctaveNoise2D(
        position.X * 0.005f,
        position.Z * 0.005f,
        4,
        0.5f,
        2.0f
      )

    let transitionNoise =
      noiseGenerator.Noise2D(position.X * 0.01f, position.Z * 0.01f)

    let normalizedBiome = (biomeNoise + 1.0f) * 0.5f
    let normalizedTransition = (transitionNoise + 1.0f) * 0.5f

    if normalizedTransition > 0.7f then
      let fromZone =
        if normalizedBiome < 0.33f then Lowland
        elif normalizedBiome < 0.66f then Midland
        else Highland

      let toZone =
        if normalizedBiome < 0.33f then Midland
        elif normalizedBiome < 0.66f then Highland
        else Lowland

      Transition {
        From = fromZone
        To = toZone
        BlendFactor = (normalizedTransition - 0.7f) / 0.3f
      }
    elif normalizedBiome < 0.33f then
      Lowland
    elif normalizedBiome < 0.66f then
      Midland
    else
      Highland

  /// Creates a zone map for the specified world dimensions
  /// <param name="width">The width of the zone map in chunks</param>
  /// <param name="depth">The depth of the zone map in chunks</param>
  /// <param name="worldSeed">The seed for world generation</param>
  /// <param name="noiseGenerator">The noise generator to use</param>
  /// <returns>A complete zone map with zones and transitions</returns>
  let createZoneMap
    (width: int)
    (depth: int)
    (worldSeed: int)
    (noiseGenerator: INoiseGenerator)
    : ZoneMap =
    let zones = Array2D.zeroCreate<ZoneType> width depth
    let transitions: ZoneTransition ResizeArray = ResizeArray()

    for x in 0 .. width - 1 do
      for z in 0 .. depth - 1 do
        let worldX = float32 x * 100.0f
        let worldZ = float32 z * 100.0f
        let position = Vector3(worldX, 0.0f, worldZ)
        let zone = getZoneAtPosition worldSeed position noiseGenerator
        zones.[x, z] <- zone

        match zone with
        | Transition transition -> transitions.Add transition
        | _ -> ()

    {
      Zones = zones
      Transitions = transitions.ToArray()
    }

  /// Checks if a position is within the park boundaries
  /// <param name="position">The position to check</param>
  /// <param name="parkDiameter">The diameter of the park</param>
  /// <returns>True if the position is within the park</returns>
  let isInPark (position: Vector3) (parkDiameter: float32<WorldUnits>) : bool =
    let diameter = float32 parkDiameter
    let distance = sqrt(position.X * position.X + position.Z * position.Z)
    distance < diameter * 0.5f

  /// Gets the zone type for a specific chunk coordinate
  /// <param name="chunkCoord">The chunk coordinate to check</param>
  /// <param name="worldSeed">The seed for world generation</param>
  /// <param name="noiseGenerator">The noise generator to use</param>
  /// <returns>The zone type for the specified chunk</returns>
  let getChunkZone
    (chunkCoord: ChunkCoord)
    (worldSeed: int)
    (noiseGenerator: INoiseGenerator)
    : ZoneType =
    let position =
      Vector3(
        float32 chunkCoord.X * 100.0f,
        0.0f,
        float32 chunkCoord.Z * 100.0f
      )

    getZoneAtPosition worldSeed position noiseGenerator
