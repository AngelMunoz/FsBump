namespace FsBump.WorldGeneration

open System
open Microsoft.Xna.Framework
open FsBump.Core

module Park =

  /// Generates park area with all biomes at relaxed intensity
  /// <param name="config">Park configuration</param>
  /// <param name="env">Environment containing noise generator</param>
  /// <returns>Park generation result with tiles, zone map, gates, and spawn point</returns>
  let generate
    (config: ParkConfig)
    (env: #INoiseProvider)
    : ParkGenerationResult =
    {
      ParkTiles = Array.empty
      ZoneMap = Array2D.zeroCreate 1 1
      Gates = Array.empty
      SpawnPoint = Vector3(0.0f, 4.0f, 0.0f)
      GenerationTime = 0.0f<Seconds>
    }
