namespace FsBump.Core

open System
open System.Collections.Generic
open Microsoft.Xna.Framework
open FsBump.WorldGeneration
open Mibo.Elmish

/// Elmish-style orchestrator for world generation
module Generation =
  /// Main model for world generation state
  type Model = {
    /// Optional park generation result
    ParkResult: ParkGenerationResult voption
    /// Current chunk manager state
    ChunkManager: ChunkManager
    /// Currently active biome/zone type
    ActiveBiomeIntensity: ZoneType
    /// Last gate the player crossed
    LastGateCrossed: GateType voption
    /// Array of checkpoint states
    Checkpoints: CheckpointState array
  }

  /// Messages for world generation operations
  type Msg =
    /// Initialize a new park with given configuration
    | InitializePark of ParkConfig
    /// Generate a specific chunk
    | GenerateChunk of ChunkCoord
    /// Chunk generation completed
    | ChunkGenerated of ChunkCoord * FsBump.Core.Tile array
    /// Check if player crossed a gate
    | CheckGateCrossing of PlayerPosition * Vector3
    /// Reset player to park spawn point
    | ResetToPark of Vector3
    /// Save current world state
    | SaveWorld of WorldSaveData
    /// Load a world state
    | LoadWorld of WorldSaveData
    /// Save a finite level
    | SaveFiniteLevel of FiniteLevelSaveData
    /// Load a finite level
    | LoadFiniteLevel of FiniteLevelSaveData

  /// Initialize generation system with park configuration
  let init(config: ParkConfig) : struct (Model * Cmd<Msg>) =
    let noiseGenerator = Noise.create config.Seed

    let chunkManager = {
      LoadedChunks = Dictionary<ChunkCoord, ChunkState>()
      PlayerChunk = { X = 0; Z = 0 }
      WorldSeed = config.Seed
      SeedMode = SeedMode.Single
      ViewRadiusChunks = 2
      PreGeneratedRadius = 1
    }

    let model = {
      ParkResult = ValueNone
      ChunkManager = chunkManager
      ActiveBiomeIntensity = Lowland
      LastGateCrossed = ValueNone
      Checkpoints = [||]
    }

    // Start with park generation - for now return empty model with initialize command
    let initialModel = {
      ParkResult = ValueNone
      ChunkManager = chunkManager
      ActiveBiomeIntensity = Lowland
      LastGateCrossed = ValueNone
      Checkpoints = [||]
    }

    initialModel, Cmd.ofMsg(InitializePark config)

  /// Update generation system with Elmish messages
  let update (msg: Msg) (model: Model) : struct (Model * Cmd<Msg>) =
    match msg with
    | InitializePark(config) ->
      let parkResult = Park.generate config model.ChunkManager
      { model with ParkResult = ValueSome parkResult }, Cmd.none
    | GenerateChunk(coord) ->
      let tiles = Chunk.generate coord model.ChunkManager
      model, Cmd.ofMsg(ChunkGenerated(coord, tiles))
    | ChunkGenerated(coord, tiles) ->
      model, Cmd.none
    | CheckGateCrossing(_, _) -> model, Cmd.none
    | ResetToPark(_) -> model, Cmd.none
    | SaveWorld(_) -> model, Cmd.none
    | LoadWorld(_) -> model, Cmd.none
    | SaveFiniteLevel(_) -> model, Cmd.none
    | LoadFiniteLevel(_) -> model, Cmd.none
