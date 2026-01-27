namespace FsBump.WorldGeneration

open System
open Microsoft.Xna.Framework
open FsBump.Core

module Park =

  /// Generates the park area with all biomes at relaxed intensity
  /// <param name="config">Park configuration</param>
  /// <param name="chunkManager">Chunk manager for the world</param>
  /// <returns>Park generation result with tiles, zone map, gates, and spawn point</returns>
  let generate
    (config: ParkConfig)
    (chunkManager: ChunkManager)
    : ParkGenerationResult =
    {
      ParkTiles = [||]
      ZoneMap = Array2D.create 1 1 Lowland
      Gates = [||]
      SpawnPoint = Vector3(0.0f, 4.0f, 0.0f)
      GenerationTime = 0.0f<Seconds>
    }
