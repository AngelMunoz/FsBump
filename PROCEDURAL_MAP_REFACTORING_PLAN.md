# Procedural Map Refactoring Plan

## Overview

Transform the procedural map generation system from a linear infinite track to a multi-path branching system with two distinct generation approaches:

1. **Infinite Mode**: Continuous generation with multi-path support
2. **Finite Modes (Exploration/Challenge)**: Pre-generated graph, fixed length (2-5 min), saveable

## Current State

- Single linear path with 90° turns only
- Only 10% of assets utilized (9-10 out of 348 loaded assets)
- Three segment types: flatRun, slope, platform
- Infinite scrolling with cleanup at 200 units
- Player: 10 units/s speed, 2.8f max jump height, 7.0f max jump distance

## Design Goals

- **Relaxing but challenging exploration** gameplay
- **Multi-branch paths** with meaningful choices
- **70-80% asset utilization** from 348 available assets
- **Android-optimized performance** with distance culling
- **Saveable finite modes** with 2-5 minute completion times

---

## Phase 1: Core Infrastructure

### 1.1 Expand Domain Types (`Domain.fs`)

Add new types to support multi-path and finite modes:

```fsharp
// Path identification
type PathId = PathId of Guid

// Game modes
type GameMode =
    | Infinite
    | Exploration of FiniteModeConfig
    | Challenge of FiniteModeConfig

and FiniteModeConfig = {
    Duration: int  // 2-5 minutes in seconds
    BranchComplexity: int  // 1-3, number of branch points
    Difficulty: float32  // 0.0-1.0
}

// Enhanced path state
type PathState = {
    Id: PathId
    Position: Vector3
    Direction: Vector3
    PreviousDirection: Vector3
    Width: int
    CurrentColor: int
    IsActive: bool
    IsMainPath: bool  // For finite modes: distinguish main vs branches
    ParentPathId: PathId option
    ConvergencePathId: PathId option
    DistanceFromStart: float32  // Track progress in finite modes
}

// Path graph for finite modes
type PathGraph = {
    Paths: PathState list
    StartPoint: Vector3
    EndPoint: Vector3 option  // None for infinite
    Mode: GameMode
    Seed: int  // For deterministic regeneration
}

// Segment metadata
type SegmentMetadata = {
    PathId: PathId
    StartPosition: Vector3
    EndPosition: Vector3
    SegmentType: string  // "flatRun", "slope", "platform", etc.
    Assets: string list
    IsBranchPoint: bool
    IsConvergencePoint: bool
}
```

**Why**: Foundation for multi-path, branching/convergence tracking, and save/load structure.

### 1.2 Create Path Graph System (`PathGraph.fs`)

Central module for path management:

```fsharp
module PathGraph =
    // Create new path
    let createPath id startPos direction isMain =

    // Add branch from existing path
    let addBranch graph parentPathId branchConfig =

    // Find convergence points
    let findConvergencePaths graph currentPath =

    // Find path at position (for player tracking)
    let findPathAtPosition graph pos =

    // Deactivate distant paths
    let cleanupDistantPaths graph playerPos cutoffDistance =

    // Regenerate path from metadata (for backtracking)
    let regeneratePath segmentMetadata startState env =
```

**Why**: Centralizes path management, handles branching/convergence, enables unloading/regeneration.

### 1.3 Create Terrain Assets System (`TerrainAssets.fs`)

Organize all 348 assets by category:

```fsharp
module TerrainAssets =
    // Platforms (all sizes: 1x1x1 to 6x6x4)
    let platforms = [...]

    // Slopes (9 sizes: 2x2x2 to 6x6x4)
    let slopes = [...]

    // Barriers (12 sizes per color + 9 neutral)
    let barriers = [...]

    // Arches (3 per color)
    let arches = [...]

    // Pipes (7 per color)
    let pipes = [...]

    // Interactive elements
    let interactive = [...]
    // - spring_pad, button_base, levers

    // Collectibles
    let collectibles = [...]
    // - star, heart, diamond

    // Decorative
    let decorations = [...]
    // - flags, hoops, railings, bracing, struts

    // Get assets by game mode
    let getAssetsByMode mode =
        match mode with
        | Exploration ->
            // More decorations, wider paths, arches, collectibles
        | Challenge ->
            // More barriers, narrower paths, technical platforms
        | Infinite ->
            // Balanced mix
```

**Why**: Structured access to all assets, enables mode-specific selection, makes expansion easy.

---

## Phase 2: Generation Strategies

### 2.1 Mode-Specific Generators

**ExplorationMode.fs**:
- Wide paths (width 3-4)
- Frequent decorative elements
- Many branches (3-5 per level)
- Gradual elevation changes
- Arches as landmarks every 50-80 units
- Collectibles scattered throughout
- Easier gaps (2-3 units)
- Longer flat runs (15-25 units)

**ChallengeMode.fs**:
- Narrow paths (width 1-2)
- Precise gap placements (3-4 units)
- Technical slope combinations
- Fewer but harder branches
- Higher barriers
- Spring pads for skill expression
- Shorter segments (8-15 units)
- Elevation changes requiring precision

**InfiniteMode.fs**:
- Balanced between exploration and challenge
- Branching every 60-100 units
- 2-3 active paths max
- Continuous generation
- Color cycling continues indefinitely

### 2.2 Expand Building Blocks

**EnhancedPlatforms.fs**:
- Use all platform sizes (1x1x1 to 6x6x4)
- Elevated platforms (height > 1)
- Decorative platforms for landmarks
- Arrow-marked platforms for guidance
- Width varies by mode

**AdvancedSlopes.fs**:
- All 9 slope sizes (2x2x2 to 6x6x4)
- Slope chains for gradual elevation
- Slope combinations with gaps
- Stair-like alternating slopes

**StructuralElements.fs**:
- Arches as doorways/visual markers
- Pillars as vertical obstacles
- Pipe networks for industrial theme

**DecorationSystem.fs**:
- Railings on dangerous edges
- Bracing under elevated sections
- Struts on long spans
- Signage for direction guidance

### 2.3 Finite Mode Pre-Generation

**FiniteModeGenerator.fs**:
```fsharp
let generateFullGraph mode env =
    // Calculate total distance: duration × 10 units/s
    // For 2-5 minutes: 1200-3000 units total
    // Generate main path
    // Add branches at regular intervals
    // Create convergence points
    // Place landmarks and decorations
    // Return complete PathGraph with metadata
```

**Why**: Pre-generates entire graph at startup, ensures complete saveable level.

---

## Phase 3: Rendering & Performance

### 3.1 Enhanced Culling System

Modify `MapGenerator.draw` to add distance culling:

```fsharp
let draw env frustum playerPos map buffer =
    let viewDistance = 100.0f  // Android optimization

    buffer.DrawMany([|
        for tile in map do
            let distance = Vector3.Distance(tile.Position, playerPos)
            let inFrustum = frustum.Intersects(getBoundingBox tile)

            // Only render if both frustum AND distance checks pass
            if distance < viewDistance && inFrustum then
                // Draw tile
    |])
```

### 3.2 Path-Based Tile Management

**TileManager.fs**:
```fsharp
let loadPathTiles path graph env =
    // Regenerate tiles from path metadata

let unloadPathTiles pathId tiles =
    // Remove tiles from array for inactive paths

let getActiveTiles tiles playerPos cutoff =
    // Filter tiles by distance
```

**Why**: Implements "unload distant paths" requirement, enables backtracking with regeneration.

---

## Phase 4: Save/Load System

### 4.1 Save Data Structure

**SaveState.fs**:
```fsharp
type SaveData = {
    GameMode: string  // "Infinite", "Exploration", "Challenge"
    PlayerPosition: Vector3
    PlayerRotation: Vector3
    PathGraph: PathGraph  // For finite modes
    CurrentPathId: PathId
    DistanceTraveled: float32
    Timestamp: DateTime
}

let serialize saveData =
    // Convert to JSON/protobuf

let deserialize data =
    // Load from file
```

### 4.2 Save/Load Integration

Modify `Program.fs` to add save/load commands and UI.

---

## Phase 5: Branching & Convergence

### 5.1 Branch Strategy

```fsharp
let shouldBranch pathState distance config =
    // Exploration: branch every 50-70 units
    // Challenge: branch every 80-100 units
    // Maximum branches: based on BranchComplexity

let createBranch parentPath branchConfig env =
    // Create new path from branch point
    // Determine branch direction (left/right/up/down)
    // Set convergence target
```

### 5.2 Convergence Strategy

```fsharp
let findConvergenceTarget paths currentPath =
    // Find nearest path to merge with
    // Prefer paths at similar height
    // Avoid creating loops

let createConvergence path1 path2 env =
    // Generate connecting tiles
    // Ensure heights match
    // Place visual marker (arch/wide platform)
```

---

## Phase 6: Game Integration

### 6.1 Mode Selection UI

Add to `Program.fs`:
```fsharp
type MenuState =
    | Main
    | ModeSelection
    | InGame
    | Paused

// Display: Infinite, Exploration (2min), Exploration (5min),
// Challenge (2min), Challenge (5min)
```

### 6.2 Update Map Generation Logic

Modify `MapGenerator.Operations.updateMap`:
```fsharp
let updateMap env playerPos map pathState mode =
    match mode with
    | Infinite ->
        // Continuous generation
        // Generate new segments when needed
        // Cleanup distant tiles
    | Exploration config | Challenge config ->
        // Load tiles from pre-generated graph
        // Unload distant paths
        // Regenerate paths on backtracking
        // Check if game is complete (reached end point)
```

---

## Phase 7: Testing & Balancing

### 7.1 Performance Testing (Android Targets)

- FPS: 60fps steady
- Tile count: < 2000 visible tiles
- Memory: < 200MB
- Load time: < 3 seconds for finite modes

### 7.2 Gameplay Balancing

- Gap sizes: ensure passable with 7.0f max jump distance
- Branch frequency: meaningful choices but not overwhelming
- Decoration density: visually interesting but not distracting
- Mode difficulty: exploration gets gradually interesting, challenge gets harder

---

## File Structure Changes

### New Files

- `PathGraph.fs` - Path graph management
- `TerrainAssets.fs` - Asset categorization
- `ExplorationMode.fs` - Exploration generation strategy
- `ChallengeMode.fs` - Challenge generation strategy
- `FiniteModeGenerator.fs` - Pre-generation for finite modes
- `TileManager.fs` - Tile loading/unloading
- `SaveState.fs` - Save/load system
- `BranchingStrategy.fs` - Branch/convergence logic

### Modified Files

- `Domain.fs` - Add new types
- `MapGeneration.fs` - Integrate new systems
- `Program.fs` - Add mode selection, save/load
- `Assets.fs` - May need updates for asset loading

---

## Implementation Order

### Priority 1 (Foundation)
1. Expand Domain types
2. Create PathGraph system
3. Create TerrainAssets system

### Priority 2 (Generation)
4. Implement ExplorationMode and ChallengeMode generators
5. Expand building blocks (platforms, slopes, structural elements)
6. Implement FiniteModeGenerator (pre-generation)

### Priority 3 (Performance)
7. Enhanced culling (distance + frustum)
8. TileManager (load/unload paths)

### Priority 4 (Save/Load)
5. Implement SaveState system
6. Integrate save/load into game loop

### Priority 5 (Gameplay)
7. Add mode selection UI
8. Implement branching/convergence strategy
9. Update MapGenerator to support both modes

### Priority 6 (Polish)
10. Add decorations and landmarks
11. Balance and test
12. Performance optimization for Android

---

## Android Performance Considerations

1. **Distance culling**: Render only within 100 units of player
2. **Path limit**: Keep only 2-3 active paths loaded
3. **Asset reuse**: Leverage large chunk rendering technique
4. **Memory management**: Unload distant paths, regenerate on demand
5. **Efficient collision**: Only check tiles within 40 units (already implemented)

---

## Success Metrics

- **Asset utilization**: 70-80% of 348 assets actively used
- **Path variety**: 2-3 branches per level, meaningful choices
- **Performance**: 60fps on Android with <200MB memory
- **Mode distinctness**: Exploration feels relaxed, Challenge feels technical
- **Save/Load**: Finite modes fully saveable, <2s load time

---

## Asset Inventory

### Current Usage (9-10 assets)
- `platform_1x1x1` - floor tiles
- `platform_2x2x1`, `platform_4x4x1`, `platform_6x6x1` - platforms
- `platform_slope_4x2x2` - slopes
- `barrier_1x1x1`, `barrier_1x1x2`, `barrier_2x1x1` - barriers
- `flag_A`, `flag_B` - decorations
- `signage_arrow_stand` - signage
- `cone`, `button_base` - edge details

### Available but Unused (330+ assets)

**Platforms** (21 more):
- `platform_2x2x2`, `platform_2x2x4`
- `platform_4x2x1`, `platform_4x2x2`, `platform_4x2x4`
- `platform_4x4x2`, `platform_4x4x4`
- `platform_6x2x1`, `platform_6x2x2`, `platform_6x2x4`
- `platform_6x6x2`, `platform_6x6x4`
- `platform_decorative_*`
- `platform_arrow_*`
- `platform_hole_6x6x1`

**Slopes** (8 more):
- All `platform_slope_*` variants (9 total, using 1)

**Barriers** (21 more):
- All `barrier_*` variants (12 per color, using 3)

**Arches** (3 per color):
- `arch`, `arch_tall`, `arch_wide`

**Pipes** (7 per color):
- `pipe_straight_A`, `pipe_straight_B`, `pipe_end`
- `pipe_90_A`, `pipe_90_B`
- `pipe_180_A`, `pipe_180_B`

**Interactive** (6 per color):
- `button_base`, `button_base_xxx`
- `spring_pad` (jump boosters!)
- `lever_floor_base`, `lever_floor_base_xxx`
- `lever_wall_base_A`, `lever_wall_base_A_xxx`
- `lever_wall_base_B`, `lever_wall_base_B_xxx`
- `power`

**Collectibles** (4 per color):
- `star`, `heart`, `diamond`, `cone`

**Decorations** (20+ per color):
- `flag_C`
- `hoop`, `hoop_angled`
- `railing_straight_single`, `railing_straight_double`, `railing_straight_padded`
- `railing_corner_single`, `railing_corner_double`, `railing_corner_padded`
- `bracing_small`, `bracing_medium`, `bracing_large`
- `bomb_A`, `bomb_B`
- `structure_A`, `structure_B`, `structure_C`
- `strut_horizontal`, `strut_vertical`

**Structural** (4 pillars):
- `pillar_1x1x1`, `pillar_1x1x2`, `pillar_1x1x4`, `pillar_1x1x8`
- `pillar_2x2x2`, `pillar_2x2x4`, `pillar_2x2x8`

**Signage** (5 per color + 6 neutral):
- `signage_arrow_wall`
- `signage_arrows_left`, `signage_arrows_right`
- `signage_finish` series (neutral)

---

## Technical Notes

### Current Constraints

- **Grid alignment**: All positions use integer coordinates (1-unit grid)
- **Cardinal directions**: Movement restricted to ±X, ±Z axes
- **Color cycling**: 4 colors repeated sequentially
- **Segment length**: 10-20 units (flat/slope) or variable (platform)
- **Path width**: Fixed at 2 units (default)
- **Overlap prevention**: 0.1 unit safety buffer
- **Retry logic**: 5 attempts before falling back to flatRun

### Physics Constants

- `Gravity = -40.0f`
- `JumpSpeed = 15.0f`
- `StepHeight = 0.5f`
- `MoveSpeed = 10.0f`
- `PlayerRadius = 0.5f`

### Generation Parameters

- `MaxJumpHeight = 2.8f`
- `MaxJumpDistance = 7.0f`
- `SafetyBuffer = 0.1f`
- `CutoffDistanceSq = 1600` (40 units for collision)
- `CleanupDistanceSq = 40000` (200 units for removal)

---

## Questions for Implementation

1. Should different modes use different color schemes, or keep the 4-color cycle?
2. For backtracking in finite modes, should we keep path metadata or full tile data?
3. Should collectibles be part of the save state, or regenerate on load?
4. How should we handle mode switching in-game (if at all)?

---

## Next Steps

1. Review and approve this plan
2. Begin with Priority 1 (Foundation)
3. Implement incrementally with testing at each phase
4. Optimize for Android throughout development
5. Gather playtest feedback for balancing
