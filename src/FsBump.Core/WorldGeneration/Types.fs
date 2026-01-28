namespace FsBump.WorldGeneration

open System
open Microsoft.Xna.Framework
open FSharp.UMX

/// Unit of measure for chunk identifiers
[<Measure>]
type ChunkId

/// Unit of measure for zone identifiers
[<Measure>]
type ZoneId

/// Unit of measure for gate identifiers
[<Measure>]
type GateId

/// Unit of measure for checkpoint identifiers
[<Measure>]
type CheckpointId

/// Unit of measure for platform indices
[<Measure>]
type PlatformIndex

/// Unit of measure for world distances
[<Measure>]
type WorldUnits

/// Unit of measure for angles
[<Measure>]
type Degrees

/// Unit of measure for time
[<Measure>]
type Seconds

/// Unit of measure for velocity (world units per second)
[<Measure>]
type WorldUnitsPerSecond = WorldUnits / Seconds

/// Defines the mix of vertical movement characteristics in a zone
[<Struct>]
type VerticalMix = {
  /// Proportion of gentle vertical movement (0.0 to 1.0)
  Gentle: float32
  /// Proportion of moderate vertical movement (0.0 to 1.0)
  Moderate: float32
  /// Proportion of intense vertical movement (0.0 to 1.0)
  Vertical: float32
}

/// Represents different types of zones in the world
type ZoneType =
  /// Low-lying areas with gentle terrain
  | Lowland
  /// Mid-elevation areas with moderate terrain difficulty
  | Midland
  /// High-elevation areas with challenging terrain
  | Highland
  /// Transitional area between two zone types
  | Transition of ZoneTransition

/// Represents a transition between two zone types
and ZoneTransition = {
  /// The zone type transitioning from
  From: ZoneType
  /// The zone type transitioning to
  To: ZoneType
  /// How much to blend towards the target zone (0.0 to 1.0)
  BlendFactor: float32
}

/// Defines characteristics that influence terrain generation for a zone
[<Struct>]
type ZoneCharacteristics = {
  /// Minimum and maximum height range for terrain (min, max)
  HeightRange: struct (float32 * float32)
  /// How smooth the terrain should be (0.0 = rough, 1.0 = smooth)
  TerrainSmoothness: float32
  /// Density of platforms in the zone (0.0 = sparse, 1.0 = dense)
  PlatformDensity: float32
  /// Frequency of gaps between platforms (0.0 = rare, 1.0 = frequent)
  GapFrequency: float32
  /// Target connectivity between platforms (0.0 = disconnected, 1.0 = fully connected)
  ConnectivityTarget: float32
  /// Mix of vertical movement characteristics
  VerticalMix: VerticalMix
}

/// Defines the dimensions of a platform
[<Struct>]
type PlatformSize = {
  /// Width of the platform in world units
  Width: int
  /// Depth of the platform in world units
  Depth: int
  /// Height of the platform in world units
  Height: int
}

/// Different variants of platforms that can be generated
[<Struct>]
type PlatformVariant =
  /// Standard platform for basic gameplay
  | Standard
  /// Decorative platform with visual elements
  | Decorative
  /// Platform with directional arrow indicator
  | Arrow
  /// Platform with a hole in it
  | Hole

/// Defines the size of a chunk in tiles
[<Struct>]
type ChunkSize = {
  /// Width of the chunk in tiles
  Width: int
  /// Depth of the chunk in tiles
  Depth: int
}

/// Coordinate location of a chunk in the world grid
[<Struct>]
type ChunkCoord = {
  /// X coordinate in chunk grid
  X: int
  /// Z coordinate in chunk grid
  Z: int
}

/// Represents how a chunk was created
[<Struct>]
type ChunkMode =
  /// Chunk was procedurally generated
  | Generated
  /// Chunk was loaded from saved data
  | Loaded

/// Represents the current state of a chunk
[<Struct>]
type ChunkState = {
  /// Coordinate location of the chunk
  Coord: ChunkCoord
  /// Zone type the chunk belongs to
  Zone: ZoneType
  /// Seed used for chunk generation
  Seed: int
  /// How the chunk was created
  Mode: ChunkMode
}

/// Different visual variants of gates
[<Struct>]
type GateVariant =
  /// Standard arch gate
  | Arch
  /// Taller arch gate
  | ArchTall
  /// Wider arch gate
  | ArchWide

/// Defines how accessible a gate is from ground level
[<Struct>]
type GateAccessibility =
  /// Gate is at ground level
  | GroundLevel
  /// Gate is slightly elevated above ground (height in world units)
  | SlightlyElevated of float32
  /// Gate is significantly elevated (height in world units)
  | Elevated of float32

/// Represents a gate between zones or areas
[<Struct>]
type GateType = {
  /// Unique identifier for the gate
  Id: int<GateId>
  /// Zone type this gate belongs to
  ZoneType: ZoneType
  /// World position of the gate
  Position: Vector3
  /// Direction the gate faces
  Direction: Vector3
  /// How accessible the gate is
  Accessibility: GateAccessibility
  /// Visual variant of the gate
  Variant: GateVariant
}

/// Represents a checkpoint state in the world
[<Struct>]
type CheckpointState = {
  /// Unique identifier for the checkpoint
  Id: int<CheckpointId>
  /// World position of the checkpoint
  Position: Vector3
  /// Zone type the checkpoint is in
  Zone: ZoneType
  /// Whether the checkpoint is currently active
  IsActive: bool
}

/// Player state data saved at a checkpoint
[<Struct>]
type PlayerCheckpointData = {
  /// Player's position at checkpoint
  Position: Vector3
  /// Player's rotation at checkpoint
  Rotation: Quaternion
  /// Player's velocity at checkpoint
  Velocity: Vector3
  /// When the checkpoint was saved
  Timestamp: DateTime
}

/// How world generation seeds are handled
[<Struct>]
type SeedMode =
  /// Use a single seed for the entire world
  | Single
  /// Generate dynamic seeds based on chunk coordinates
  | DynamicChunkHash

/// Complete save data for a world state
type WorldSaveData = {
  /// World generation seed
  Seed: int
  /// Seed mode used for generation
  SeedMode: SeedMode
  /// Player's current position
  PlayerPosition: Vector3
  /// Player's current rotation
  PlayerRotation: Quaternion
  /// List of currently active chunks
  ActiveChunks: ChunkCoord[]
  /// List of activated checkpoint IDs
  CheckpointsActivated: int<CheckpointId>[]
  /// When the save was created
  Timestamp: DateTime
}

/// Data about player interactions in the world
type InteractionData = {
  /// Collectibles collected with chunk coordinates and names
  CollectiblesCollected: struct (ChunkCoord * string) array
  /// Chunks where spring pads were used
  SpringPadsUsed: ChunkCoord array
  /// Player falls with origin and optional destination chunk
  Falls: struct (ChunkCoord * ChunkCoord option) array
}

/// Data about terrain features encountered
type FeatureData = {
  /// Chunks where gaps were encountered
  GapsEncountered: ChunkCoord array
  /// Ramp sequences used with chunk coordinates and names
  RampSequencesUsed: struct (ChunkCoord * string) array
  /// High points reached with chunk coordinates and heights
  HighPointsReached: struct (ChunkCoord * float32<WorldUnits>) array
}

/// Statistics for a finite level playthrough
type FiniteStats = {
  /// Total distance traveled
  TotalDistance: float32<WorldUnits>
  /// Total time taken
  TotalTime: float32<Seconds>
  /// Estimated difficulty rating
  EstimatedDifficulty: float32
}

/// Complete save data for a finite level
type FiniteLevelSaveData = {
  /// Name of the level
  Name: string
  /// When the level was completed
  Date: DateTime
  /// Total play time
  PlayTime: float32<Seconds>
  /// Entry gate position
  EntryGate: Vector3
  /// Biome/zone type of the entry gate
  GateBiome: ZoneType
  /// Original seed used for generation
  OriginalSeed: int
  /// Original seed mode used
  OriginalSeedMode: SeedMode
  /// Path of chunks taken through the level
  PathChunks: ChunkCoord[]
  /// Interaction data from the playthrough
  Interactions: InteractionData
  /// Terrain feature data from the playthrough
  TerrainFeatures: FeatureData
  /// Final statistics
  Stats: FiniteStats
}

/// Configuration for park generation
[<Struct>]
type ParkConfig = {
  /// Diameter of the circular park area
  ParkDiameter: float32<WorldUnits>
  /// Seed for world generation
  Seed: int
  /// Size of chunks in the park
  ChunkSize: ChunkSize
}

/// Player's position and velocity state
[<Struct>]
type PlayerPosition = {
  /// Current world position
  Position: Vector3
  /// Current velocity vector
  Velocity: Vector3
}

/// Result of a chunk generation operation
type ChunkGenerationResult = {
  /// Coordinate of the generated chunk
  Coord: ChunkCoord
  /// Array of tiles in the chunk
  Tiles: FsBump.Core.Tile array
  /// Zone type of the chunk
  Zone: ZoneType
  /// Seed used for generation
  Seed: int
  /// Time taken to generate
  GenerationTime: float32<Seconds>
  /// Whether generation was successful
  Success: bool
}

/// Manages chunk loading and generation state
type ChunkManager = {
  /// Dictionary of currently loaded chunks
  LoadedChunks: System.Collections.Generic.Dictionary<ChunkCoord, ChunkState>
  /// Coordinate of chunk the player is currently in
  PlayerChunk: ChunkCoord
  /// Global seed for world generation
  WorldSeed: int
  /// Seed mode being used
  SeedMode: SeedMode
  /// Radius around player to load chunks
  ViewRadiusChunks: int
  /// Radius to pre-generate chunks
  PreGeneratedRadius: int
}

/// Result of park generation
type ParkGenerationResult = {
  /// All tiles in the generated park
  ParkTiles: FsBump.Core.Tile array
  /// 2D map of zone types
  ZoneMap: ZoneType[,]
  /// Gates found in the park
  Gates: GateType array
  /// Player spawn point
  SpawnPoint: Vector3
  /// Time taken to generate the park
  GenerationTime: float32<Seconds>
}

/// Interface for noise generation
type INoiseGenerator =
  /// Generate 2D Perlin noise
  /// <param name="x">X coordinate</param>
  /// <param name="y">Y coordinate</param>
  /// <returns>Noise value between -1.0 and 1.0</returns>
  abstract member Noise2D: x: float32 * y: float32 -> float32

  /// Generate 3D Perlin noise
  /// <param name="x">X coordinate</param>
  /// <param name="y">Y coordinate</param>
  /// <param name="z">Z coordinate</param>
  /// <returns>Noise value between -1.0 and 1.0</returns>
  abstract member Noise3D: x: float32 * y: float32 * z: float32 -> float32

  /// Generate 2D octave noise with default settings
  /// <param name="x">X coordinate</param>
  /// <param name="y">Y coordinate</param>
  /// <returns>Noise value between -1.0 and 1.0</returns>
  abstract member OctaveNoise2D: x: float32 * y: float32 -> float32

  /// Generate 3D octave noise with default settings
  /// <param name="x">X coordinate</param>
  /// <param name="y">Y coordinate</param>
  /// <param name="z">Z coordinate</param>
  /// <returns>Noise value between -1.0 and 1.0</returns>
  abstract member OctaveNoise3D: x: float32 * y: float32 * z: float32 -> float32

  /// Generate 2D octave noise with custom parameters
  /// <param name="x">X coordinate</param>
  /// <param name="y">Y coordinate</param>
  /// <param name="octaves">Number of octaves to generate</param>
  /// <param name="persistence">Amplitude multiplier per octave (0.0 to 1.0)</param>
  /// <param name="lacunarity">Frequency multiplier per octave (> 1.0)</param>
  /// <returns>Noise value between -1.0 and 1.0</returns>
  abstract member OctaveNoise2D:
    x: float32 *
    y: float32 *
    octaves: int *
    persistence: float32 *
    lacunarity: float32 ->
      float32

  /// Generate 3D octave noise with custom parameters
  /// <param name="x">X coordinate</param>
  /// <param name="y">Y coordinate</param>
  /// <param name="z">Z coordinate</param>
  /// <param name="octaves">Number of octaves to generate</param>
  /// <param name="persistence">Amplitude multiplier per octave (0.0 to 1.0)</param>
  /// <param name="lacunarity">Frequency multiplier per octave (> 1.0)</param>
  /// <returns>Noise value between -1.0 and 1.0</returns>
  abstract member OctaveNoise3D:
    x: float32 *
    y: float32 *
    z: float32 *
    octaves: int *
    persistence: float32 *
    lacunarity: float32 ->
      float32

type INoiseProvider =
  abstract NoiseGenerator: INoiseGenerator

type GenerationEnv = {
  NoiseGenerator: INoiseGenerator
  WorldSeed: int
} with

  interface INoiseProvider with
    member this.NoiseGenerator = this.NoiseGenerator
