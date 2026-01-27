namespace FsBump.WorldGeneration

open System
open Microsoft.Xna.Framework

/// Configuration for a single layer of noise
[<Struct>]
type NoiseLayer = {
  /// Scale factor for noise coordinates (higher = more zoomed out)
  Scale: float32
  /// Amplitude multiplier for the noise output
  Amplitude: float32
  /// 2D offset to apply to the noise coordinates
  Offset: Vector2
  /// Seed for this specific noise layer
  Seed: int
}

/// Configuration for octave noise generation
[<Struct>]
type OctaveConfig = {
  /// Number of octaves to layer
  Octaves: int
  /// Amplitude multiplier per octave (0.0 to 1.0)
  Persistence: float32
  /// Frequency multiplier per octave (> 1.0)
  Lacunarity: float32
}

[<RequireQualifiedAccess>]
module NoiseGenerator =
  [<Struct>]
  type internal OctaveContext = {
    Octaves: int
    Persistence: float32
    Lacunarity: float32
    Permutation: int[]
  }

  let inline private fade(t: float32) : float32 =
    let t3 = t * t * t
    let t4 = t3 * t
    6.0f * t4 * t - 15.0f * t4 + 10.0f * t3

  let private grad
    (hash: int)
    (x: float32)
    (y: float32)
    (z: float32)
    : float32 =
    let h = hash &&& 15
    let u = if h < 8 then x else y

    let v =
      if h < 4 then y
      else if h = 12 || h = 14 then x
      else z

    (if (h &&& 1) = 0 then u else -u) + (if (h &&& 2) = 0 then v else -v)

  let noise2D (x: float32) (y: float32) (p: int[]) : float32 =
    let xi = int(floor x) &&& 255
    let yi = int(floor y) &&& 255
    let xf = x - float32(floor x)
    let yf = y - float32(floor y)

    let u = fade xf
    let v = fade yf

    let aa = p.[p.[xi] + yi]
    let ab = p.[p.[xi] + yi + 1]
    let ba = p.[p.[xi + 1] + yi]
    let bb = p.[p.[xi + 1] + yi + 1]

    let x1 = MathHelper.Lerp(grad aa xf yf 0.0f, grad ba (xf - 1.0f) yf 0.0f, u)

    let x2 =
      MathHelper.Lerp(
        grad ab xf (yf - 1.0f) 0.0f,
        grad bb (xf - 1.0f) (yf - 1.0f) 0.0f,
        u
      )

    MathHelper.Lerp(x1, x2, v)

  let noise3D (x: float32) (y: float32) (z: float32) (p: int[]) : float32 =
    let xi = int(floor x) &&& 255
    let yi = int(floor y) &&& 255
    let zi = int(floor z) &&& 255
    let xf = x - float32(floor x)
    let yf = y - float32(floor y)
    let zf = z - float32(floor z)

    let u = fade xf
    let v = fade yf
    let w = fade zf

    let aaa = p.[p.[p.[xi] + yi] + zi]
    let aba = p.[p.[p.[xi] + yi + 1] + zi]
    let aab = p.[p.[p.[xi] + yi] + zi + 1]
    let abb = p.[p.[p.[xi] + yi + 1] + zi + 1]
    let baa = p.[p.[p.[xi + 1] + yi] + zi]
    let bba = p.[p.[p.[xi + 1] + yi + 1] + zi]
    let bab = p.[p.[p.[xi + 1] + yi] + zi + 1]
    let bbb = p.[p.[p.[xi + 1] + yi + 1] + zi + 1]

    let x1 = MathHelper.Lerp(grad aaa xf yf zf, grad baa (xf - 1.0f) yf zf, u)

    let x2 =
      MathHelper.Lerp(
        grad aba xf (yf - 1.0f) zf,
        grad bba (xf - 1.0f) (yf - 1.0f) zf,
        u
      )

    let y1 = MathHelper.Lerp(x1, x2, v)

    let x1 =
      MathHelper.Lerp(
        grad aab xf yf (zf - 1.0f),
        grad bab (xf - 1.0f) yf (zf - 1.0f),
        u
      )

    let x2 =
      MathHelper.Lerp(
        grad abb xf (yf - 1.0f) (zf - 1.0f),
        grad bbb (xf - 1.0f) (yf - 1.0f) (zf - 1.0f),
        u
      )

    let y2 = MathHelper.Lerp(x1, x2, v)

    MathHelper.Lerp(y1, y2, w)

  let inline internal octaveNoise2D
    (context: OctaveContext)
    (x: float32)
    (y: float32)
    : float32 =
    let mutable total = 0.0f
    let mutable frequency = 1.0f
    let mutable amplitude = 1.0f
    let mutable maxValue = 0.0f

    for _ in 0 .. context.Octaves - 1 do
      total <-
        total
        + noise2D (x * frequency) (y * frequency) context.Permutation
          * amplitude

      maxValue <- maxValue + amplitude
      amplitude <- amplitude * context.Persistence
      frequency <- frequency * context.Lacunarity

    total / maxValue

  let inline internal octaveNoise3D
    (context: OctaveContext)
    (x: float32)
    (y: float32)
    (z: float32)
    : float32 =
    let mutable total = 0.0f
    let mutable frequency = 1.0f
    let mutable amplitude = 1.0f
    let mutable maxValue = 0.0f

    for _ in 0 .. context.Octaves - 1 do
      total <-
        total
        + noise3D
            (x * frequency)
            (y * frequency)
            (z * frequency)
            context.Permutation
          * amplitude

      maxValue <- maxValue + amplitude
      amplitude <- amplitude * context.Persistence
      frequency <- frequency * context.Lacunarity

    total / maxValue

  /// Samples a noise layer at a specific 2D position
  /// <param name="noise">The noise generator to use</param>
  /// <param name="x">X coordinate</param>
  /// <param name="y">Y coordinate</param>
  /// <param name="layer">Noise layer configuration</param>
  /// <returns>Sampled noise value</returns>
  let inline sample
    (noise: INoiseGenerator)
    (x: float32)
    (y: float32)
    (layer: NoiseLayer)
    : float32 =
    let nx = (x + layer.Offset.X) * layer.Scale
    let ny = (y + layer.Offset.Y) * layer.Scale
    noise.Noise2D(nx, ny) * layer.Amplitude

  /// Samples a noise layer at a specific 3D position
  /// <param name="noise">The noise generator to use</param>
  /// <param name="x">X coordinate</param>
  /// <param name="y">Y coordinate</param>
  /// <param name="z">Z coordinate</param>
  /// <param name="layer">Noise layer configuration</param>
  /// <returns>Sampled noise value</returns>
  let inline sample3D
    (noise: INoiseGenerator)
    (x: float32)
    (y: float32)
    (z: float32)
    (layer: NoiseLayer)
    : float32 =
    let nx = (x + layer.Offset.X) * layer.Scale
    let ny = (y + layer.Offset.Y) * layer.Scale
    let nz = z * layer.Scale
    noise.Noise3D(nx, ny, nz) * layer.Amplitude

  /// Combines multiple noise layers at a specific 2D position
  /// <param name="noise">The noise generator to use</param>
  /// <param name="layers">Array of noise layers to combine</param>
  /// <param name="x">X coordinate</param>
  /// <param name="y">Y coordinate</param>
  /// <returns>Combined noise value</returns>
  let inline combineLayers
    (noise: INoiseGenerator)
    (layers: NoiseLayer array)
    (x: float32)
    (y: float32)
    : float32 =
    layers |> Array.sumBy(sample noise x y)

/// Factory for creating noise generators
type Noise =

  /// Creates a new noise generator with the specified seed and configuration
  /// <param name="seed">Seed for deterministic noise generation</param>
  /// <param name="octaveConfig">Optional octave configuration (uses defaults if not provided)</param>
  /// <returns>A new noise generator instance</returns>
  static member create
    (seed: int, [<Struct>] ?octaveConfig: OctaveConfig)
    : INoiseGenerator =
    let p =
      let rng = Random seed
      Array.init 512 (fun _ -> rng.Next(0, 256))

    let ctx: NoiseGenerator.OctaveContext =
      let config =
        defaultValueArg octaveConfig {
          Octaves = 4
          Persistence = 0.5f
          Lacunarity = 2.0f
        }

      {
        Octaves = config.Octaves
        Persistence = config.Persistence
        Lacunarity = config.Lacunarity
        Permutation = p
      }

    { new INoiseGenerator with
        member _.Noise2D(x, y) = NoiseGenerator.noise2D x y p
        member _.Noise3D(x, y, z) = NoiseGenerator.noise3D x y z p

        member _.OctaveNoise2D(x, y) = NoiseGenerator.octaveNoise2D ctx x y

        member _.OctaveNoise2D(x, y, octaves, persistence, lacunarity) =
          NoiseGenerator.octaveNoise2D
            {
              Permutation = p
              Octaves = octaves
              Persistence = persistence
              Lacunarity = lacunarity
            }
            x
            y

        member _.OctaveNoise3D(x, y, z) = NoiseGenerator.octaveNoise3D ctx x y z

        member _.OctaveNoise3D(x, y, z, octaves, persistence, lacunarity) =
          NoiseGenerator.octaveNoise3D
            {
              Permutation = p
              Octaves = octaves
              Persistence = persistence
              Lacunarity = lacunarity
            }
            x
            y
            z
    }
