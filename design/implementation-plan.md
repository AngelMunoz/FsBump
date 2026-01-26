# Infinite Park World - Implementation Plan

**Version:** 2.0
**Last Updated:** 2026-01-26
**Status:** Technical Specification - Ready for Implementation

---

## Implementation Principles

### 1. Type/Domain Driven
**Types drive implementation** - code should follow from type definitions naturally.
Use FSharp.UMX measures for all physical quantities.
Use DUs (discriminated unions) for closed sets with small variant counts.

### 2. Closed Sets vs Open Sets

**Closed sets (small, fixed variants):** Use DU
```fsharp
type GateVariant =
  | Standard
  | Tall
  | Wide
```

**Open sets (potentially infinite):** Use int/unit-of-measure
```fsharp
[<Measure>] type PlatformIndex
let platformCount: int<PlatformIndex> = 100
```

### 3. No Code in Plan
Implementation plan defines **types and their relationships** only.
No code examples, no algorithm details.
Types should be self-documenting enough that implementation follows naturally.

### 4. Namespace/Directory Separation
All generation code in `FsBump.WorldGeneration` namespace.
New directory: `src/FsBump.WorldGeneration/`
Separate library that can be tested independently.

### 5. Self-Contained Library
Minimal dependencies (XNA, FSharp.UMX, math).
Elmish-compatible message types for async generation.
Can be used in other projects.

---

## File Structure

```
src/
  FsBump.Core/
    Domain.fs (extend with generation types)
    Program.fs (extend with generation messages)
    ...

  FsBump.WorldGeneration/ (NEW library)
    Types.fs              (Core types, measures, DUs)
    Noise.fs              (Noise interfaces)
    Zone.fs               (Zone definitions)
    Chunk.fs              (Chunk management)
    Park.fs               (Park generation)
    Gates.fs              (Gate system)
    Generation.fs          (Orchestrator)
    Checkpoints.fs         (Checkpoint system)
    SaveSystem.fs         (Save/load, finite recording)
```

---

## Core Types

### Units of Measure

```fsharp
[<Measure>] type ChunkId
[<Measure>] type ZoneId
[<Measure>] type GateId
[<Measure>] type CheckpointId
[<Measure>] type PlatformIndex
[<Measure>] type WorldUnits
[<Measure>] type Degrees
[<Measure>] type Seconds

[<Measure>] type WorldUnitsPerSecond = WorldUnits / Seconds
```

### Zone/Biome Types

```fsharp
type ZoneType =
  | Lowland
  | Midland
  | Highland
  | Transition of From: ZoneType * To: ZoneType * BlendFactor: float32

// Zone characteristics - type-safe, no magic numbers
type ZoneCharacteristics = {
  HeightRange: WorldUnits * WorldUnits
  TerrainSmoothness: float32
  PlatformDensity: float32
  GapFrequency: float32
  ConnectivityTarget: float32
  VerticalMix: VerticalMix
}

type VerticalMix = {
  Gentle: float32  // 0.0 - 1.0
  Moderate: float32  // 0.0 - 1.0
  Vertical: float32  // 0.0 - 1.0
}
```

### Platform and Tile Types

```fsharp
type PlatformSize = {
  Width: int<PlatformIndex>
  Depth: int<PlatformIndex>
  Height: int<PlatformIndex>
}

type PlatformVariant = DU (closed set - 3 variants)
  | Standard
  | Decorative
  | Arrow
  | Hole
```

### Chunk Types

```fsharp
type ChunkSize = {
  Width: int<WorldUnits>
  Depth: int<WorldUnits>
}

type ChunkCoord = {
  X: int
  Z: int
}

type ChunkState = {
  Coord: ChunkCoord
  Zone: ZoneType
  Seed: int
  IsGenerated: bool
  IsLoaded: bool
}
```

### Gate Types

```fsharp
type GateVariant = DU (closed set - 3 variants)
  | Arch
  | ArchTall
  | ArchWide

type GateAccessibility = DU
  | GroundLevel
  | SlightlyElevated of WorldUnits
  | Elevated of WorldUnits

type GateType = {
  Id: int<GateId>
  ZoneType: ZoneType
  Position: Vector3<WorldUnits>
  Direction: Vector3<WorldUnits>
  Accessibility: GateAccessibility
  Variant: GateVariant
}
```

### Checkpoint Types

```fsharp
type CheckpointState = {
  Id: int<CheckpointId>
  Position: Vector3<WorldUnits>
  Zone: ZoneType
  IsActive: bool
}

type PlayerCheckpointData = {
  Position: Vector3<WorldUnits>
  Rotation: Quaternion
  Velocity: Vector3<WorldUnits/Seconds>
  Timestamp: DateTime
}
```

### Save Types

```fsharp
type SeedMode = DU
  | Single
  | DynamicChunkHash

type WorldSaveData = {
  Seed: int
  SeedMode: SeedMode
  PlayerPosition: Vector3<WorldUnits>
  PlayerRotation: Quaternion
  ActiveChunks: ChunkCoord[]
  CheckpointsActivated: int<CheckpointId>[]
  Timestamp: DateTime
}

type FiniteLevelSaveData = {
  Name: string
  Date: DateTime
  PlayTime: float32<Seconds>
  EntryGate: Vector3<WorldUnits>
  GateBiome: ZoneType
  OriginalSeed: int
  OriginalSeedMode: SeedMode
  PathChunks: ChunkCoord[]
  Interactions: InteractionData
  TerrainFeatures: FeatureData
  Stats: FiniteStats
}

type InteractionData = {
  CollectiblesCollected: (ChunkCoord * string) array
  SpringPadsUsed: ChunkCoord array
  Falls: (ChunkCoord * ChunkCoord option) array
}

type FeatureData = {
  GapsEncountered: ChunkCoord array
  RampSequencesUsed: (ChunkCoord * string) array
  HighPointsReached: (ChunkCoord * float32<WorldUnits>) array
}

type FiniteStats = {
  TotalDistance: float32<WorldUnits>
  TotalTime: float32<Seconds>
  EstimatedDifficulty: float32
}
```

### Generation Message Types

```fsharp
type GenerationMsg =
  | InitializePark of ParkConfig
  | GenerateChunk of ChunkCoord
  | ChunkGenerated of ChunkCoord * FsBump.Core.Tile array
  | CheckGateCrossing of PlayerPosition * PlayerVelocity
  | ResetToPark of Vector3<WorldUnits>
  | SaveWorld of WorldSaveData
  | LoadWorld of WorldSaveData
  | SaveFiniteLevel of FiniteLevelSaveData
  | LoadFiniteLevel of FiniteLevelSaveData

type PlayerPosition = {
  Position: Vector3<WorldUnits>
  Velocity: Vector3<WorldUnits/Seconds>
}

type ParkConfig = {
  ParkDiameter: float32<WorldUnits>
  Seed: int
  ChunkSize: ChunkSize
}
```

### Chunk Management Types

```fsharp
type ChunkGenerationResult = {
  Coord: ChunkCoord
  Tiles: FsBump.Core.Tile array
  Zone: ZoneType
  Seed: int
  GenerationTime: float32<Seconds>
  Success: bool
}

type ChunkManager = {
  LoadedChunks: Map<ChunkCoord, ChunkState>
  PlayerChunk: ChunkCoord
  WorldSeed: int
  SeedMode: SeedMode
  ViewRadiusChunks: int
  PreGeneratedRadius: int
}
```

### Park Generation Types

```fsharp
type ParkGenerationResult = {
  ParkTiles: FsBump.Core.Tile array
  ZoneMap: ZoneType[,]
  Gates: GateType array
  SpawnPoint: Vector3<WorldUnits>
  GenerationTime: float32<Seconds>
}
```

### Generation Model Types

```fsharp
type GenerationModel = {
  ParkResult: ParkGenerationResult option
  ChunkManager: ChunkManager
  ActiveBiomeIntensity: ZoneType
  LastGateCrossed: GateType option
  Checkpoints: CheckpointState array
}

type GenerationCmd =
  | UpdateTiles of FsBump.Core.Tile array
  | UpdateBiomeIntensity of ZoneType
  | UpdateCheckpoints of CheckpointState array
```

---

## Zone System

### Zone Characteristics

**Lowland (Safe, Relaxed):**
```fsharp
{
  HeightRange = 2.0f<WorldUnits>, 4.0f<WorldUnits>
  TerrainSmoothness = 0.9f
  PlatformDensity = 0.85f
  GapFrequency = 0.05f
  ConnectivityTarget = 0.90f
  VerticalMix = { Gentle = 0.70f; Moderate = 0.20f; Vertical = 0.10f }
}
```

**Midland (Balanced):**
```fsharp
{
  HeightRange = 4.0f<WorldUnits>, 7.0f<WorldUnits>
  TerrainSmoothness = 0.6f
  PlatformDensity = 0.65f
  GapFrequency = 0.15f
  ConnectivityTarget = 0.70f
  VerticalMix = { Gentle = 0.30f; Moderate = 0.50f; Vertical = 0.20f }
}
```

**Highland (Challenging, Vertical):**
```fsharp
{
  HeightRange = 7.0f<WorldUnits>, 12.0f<WorldUnits>
  TerrainSmoothness = 0.3f
  PlatformDensity = 0.45f
  GapFrequency = 0.30f
  ConnectivityTarget = 0.50f
  VerticalMix = { Gentle = 0.10f; Moderate = 0.30f; Vertical = 0.60f }
}
```

### Zone Map

```fsharp
// Type that represents zone distribution across world
type ZoneMap = {
  Zones: ZoneType[,]
  Transitions: ZoneTransition array
}

type ZoneTransition = {
  From: ZoneType
  To: ZoneType
  FromPosition: ChunkCoord
  ToPosition: ChunkCoord
  BlendFactor: float32
}
```

### Zone System Behavior

**Park Zone (300-500 units from origin):**
- Contains all three biomes (Lowland, Midland, Highland)
- All biomes at **relaxed intensity** (density/gaps as defined above)
- No gates within park
- Free exploration without pressure

**Beyond Park (after crossing gates):**
- Entering a biome means that biome's rules become **stricter**
- Intensity can increase cumulatively (gate crossings accumulate)
- Player can always walk back through gates to return to relaxed park

**Zone Interleaving:**
- Biomes are mixed throughout park and beyond
- Transitions blend biome characteristics
- No monolithic blocks of single biome

---

## Gate System

### Gate Detection

```fsharp
// Detect if player crossed a gate
type GateCrossingEvent = {
  Gate: GateType
  Time: DateTime
  PreviousBiome: ZoneType
  NewBiome: ZoneType
}
```

### Gate Behavior

**Gates as Visual Indicators:**
- Not portals or teleporters
- Mark biome boundaries
- Communicate biome intensity through:
  - Accessibility (GroundLevel vs Elevated)
  - Preview platform width (Wide = easy, Narrow = hard)
  - Arch variant (Standard = relaxed, Tall/Wide = challenging)

**Gate Crossing Effects:**
- Crossing gate → biome intensity updates (becomes stricter)
- Walk back through gate → biome intensity reverts
- No commitment - player can freely explore

---

## Chunk System

### Chunk Definition

```fsharp
type Chunk = {
  Coord: ChunkCoord
  Zone: ZoneType
  Tiles: FsBump.Core.Tile array
  Seed: int
}
```

### Chunk Loading Strategy

**Hybrid Strategy:**
1. **Pre-generate 1-3 chunks** immediately on spawn
2. **Generate on-demand** as player explores
3. **Async generation** for chunks player is approaching
4. **Distance-based unloading** (unload chunks > 2 radius away)

**Chunk Size:** 100x100 WorldUnits

**View Radius:** 2 chunks (load 5x5 area)

**Pre-gen Radius:** 1 chunk (load 3x3 area immediately)

---

## Park Generation

### Park Definition

```fsharp
type ParkConfig = {
  Diameter: float32<WorldUnits>  // 300-500
  Seed: int
  ChunkSize: ChunkSize
}
```

### Park Result

```fsharp
type ParkGenerationResult = {
  ParkTiles: FsBump.Core.Tile array
  ZoneMap: ZoneType[,]
  Gates: GateType array
  SpawnPoint: Vector3<WorldUnits>
}
```

### Park Composition

**Size:** 300-500 units diameter from origin

**Biomes:**
- All three types present (Lowland, Midland, Highland)
- Interleaved distribution
- Each biome at **relaxed intensity** (no initial strictness)

**Features:**
- **Rolling ramps** (continuous slopes, no jumps)
- **Elevated platforms** (jump-off points, sparse in Highland)
- **Collectibles** (abundant in Lowland, moderate elsewhere)
- **Landmarks** (arches, flags spaced throughout)
- **No gaps** within park (0% frequency)

**Gates:**
- Located at **park boundaries** (not within)
- Number depends on zone distribution (typically 6-8)
- Mark where biome becomes stricter if crossed

**Spawn:** At park center, facing toward biomes

---

## Checkpoint System

### Checkpoint Activation Rules (Zone-Dependent)

**Lowland (Easy):**
- Every platform advanced = checkpoint
- Frequent checkpoints, no frustration

**Midland (Moderate):**
- Every 10-15 units = checkpoint
- Mid-way intervals, balanced

**Highland (Hard):**
- No checkpoints (or very rare at major landmarks)
- High stakes, severe punishment for mistakes

### Reset Behavior

**Void Fall:**
- Lowland: Reset to last checkpoint (every platform)
- Midland: Reset to mid-way checkpoint
- Highland: Reset to park origin

**Manual Reset:**
- Player can reset to park center anytime
- Or reset to last checkpoint

---

## Save System

### World Save

```fsharp
type WorldSaveData = {
  Seed: int
  SeedMode: SeedMode
  PlayerPosition: Vector3<WorldUnits>
  PlayerRotation: Quaternion
  ActiveChunks: ChunkCoord[]
  CheckpointsActivated: int<CheckpointId>[]
  Timestamp: DateTime
}
```

**Purpose:** Save current exploration state to resume later

### Finite Level Save

```fsharp
type FiniteLevelSaveData = {
  Name: string
  Date: DateTime
  PlayTime: float32<Seconds>
  EntryGate: Vector3<WorldUnits>
  GateBiome: ZoneType
  OriginalSeed: int
  OriginalSeedMode: SeedMode
  PathChunks: ChunkCoord[]
  Interactions: InteractionData
  TerrainFeatures: FeatureData
  Stats: FiniteStats
}
```

**Purpose:** Record exploration journey as replayable level

**Replay Modes:**
- **Fixed:** Regenerate same terrain, same path
- **Challenge:** Same terrain, different path options, time trial
- **Variant:** Same biome, different path layout

---

## Performance Targets

### Generation
- **Chunk generation:** < 500ms (Android target)
- **Park generation:** < 2s

### Runtime
- **60fps** steady on Android
- **Visible tiles:** < 2000
- **Memory:** < 200MB

### Save/Load
- **World save:** < 10KB file size
- **World load:** < 1s
- **Finite save:** < 100KB file size
- **Finite load:** < 2s

---

## File Structure Summary

### New Library: FsBump.WorldGeneration

```
src/FsBump.WorldGeneration/
  Types.fs              (UMX measures, all core types)
  Noise.fs              (Noise interfaces)
  Zone.fs               (Zone definitions, characteristics)
  Chunk.fs              (Chunk management, loading)
  Park.fs               (Park generation logic)
  Gates.fs              (Gate system)
  Generation.fs          (Orchestrator, Elmish integration)
  Checkpoints.fs         (Checkpoint system)
  SaveSystem.fs         (Save/load, finite recording)
```

### Updated FsBump.Core

```
src/FsBump.Core/
  Domain.fs             (Add UMX measures, extend with generation types)
  Program.fs             (Add GenerationMsg, GenerationModel to Model)
```

---

## Success Metrics

### Type Safety
- [ ] All closed sets use DU (not int + comments)
- [ ] All open sets use unit-of-measure
- [ ] No magic numbers/strings in types
- [ ] FSharp.UMX measures used throughout

### Completeness
- [ ] 9 files in FsBump.WorldGeneration
- [ ] All types defined, self-documenting
- [ ] Zone system covers all 3 biomes + transitions
- [ ] Chunk system supports hybrid loading
- [ ] Gate system detects crossings and updates biome
- [ ] Checkpoint system respects zone rules
- [ ] Save system supports both world and finite modes

### Integration
- [ ] Elmish-compatible messages for async generation
- [ ] Can merge incrementally without breaking game
- [ ] Separate library, minimal dependencies
- [ ] Clear separation from game logic

---

## Conclusion

This implementation plan provides a **concise, type-driven specification** for the infinite park world design:

- **Types drive implementation** - DU for closed sets, measures for open sets
- **No code in plan** - types define behavior naturally
- **9 focused files** - each with clear responsibility
- **Phase-based development** - each self-contained, mergeable independently
- **Elmish-compatible** - async generation via messages

The types defined above should be sufficient to guide implementation without needing algorithm details or code examples.
