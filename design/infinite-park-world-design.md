# Infinite Park World - Design Document

**Version:** 1.0
**Last Updated:** 2026-01-26
**Status:** Design Complete - Ready for Implementation

---

## Overview

Transform from procedural map generation from linear infinite track to an **open-world park-like platformer** with:
- Continuous rolling terrain (slopes you can roll over, not jump between)
- Multiple exploration directions (free roam, not just forward)
- High places to jump from with fall-to-terrain or fall-to-void
- Occasional gaps as features (not main mechanic)
- Seed-based reproducibility (same seed = same world feel)
- Zoned biomes with interleaved mixing
- **The Park IS starting area** - large (300-500 units), contains all three biomes mixed at relaxed intensity
- **Gates mark park boundaries** - signal where biome rules become stricter when venturing beyond park
- **Exploration freedom:** Player explores within park freely, gates only when ready to venture further

---

## Clarification: Park vs. Beyond Park

**Key Design Principle:**

**The Park (300-500 units from origin):**
- **All three biomes exist** - Lowland, Midland, Highland
- **Relaxed intensity** - Biomes at their easiest/densest parameters
- **Full exploration freedom** - Player freely explores without gates
- **No pressure** - No gates within park, player moves at own pace
- **Discovery** - Player naturally discovers all biome types without restriction

**Beyond Park (past gates at 300-500 units):**
- **Biome-specific exploration** - Traveling further means entering a particular biome's stricter rules
- **Gates as indicators** - Not portals, just visual markers showing "from here, this biome gets more strict"
- **Intensity increase** - Each gate crossing may further increase biome parameters
- **Player choice** - Free to go back through gates, can explore park again anytime

**Example Flow:**
1. Spawn in park (all biomes at relaxed intensity)
2. Explore freely for 100-200 units (no gates)
3. See gate ahead (marks boundary)
4. Choose: stay in park OR go through gate
5. If go through gate: beyond that point, biome rules become stricter
6. Can always walk back through gate to return to relaxed park exploration

---

## Core Design Philosophy

### Natural Difficulty
**No explicit difficulty setting.** Difficulty emerges from:
- Biome characteristics (height, gap frequency, platform density)
- Gate accessibility (harder gates require more effort to reach)
- Terrain characteristics (ramp steepness, platform sizes)
- Checkpoint scarcity (harder areas = fewer checkpoints)

### Player Choice
- **Park exploration = natural biome discovery** - player encounters all biomes within the park
- **Gate passage = biome intensity increase** - beyond gate, that biome follows stricter rules
- Visual communication: Gate visibility + terrain preview communicates intensity increase
- No UI difficulty sliders - game world itself communicates biome intensity through gates

### Healthy Zone Mix
- **Park contains all three biomes** - Lowland, Midland, Highland exist together from spawn
- Biomes interleave naturally (not monolithic blocks)
- Transitions blend biome characteristics
- **Gates mark biome edges** - not entry points, but transition indicators
- Player experiences variety within single exploration session

---

## The Park (Starting Area)

### Starting Point
- **Spawn Position:** (0, 4, 0) in park-like terrain
- **Initial Feel:** Clear you're in a park, not infinite void
- **Park Aesthetic:** Mixed biomes (Lowland/Midland/Highland), rolling terrain, visible gates

### Park Terrain
- **Zone Composition:** All three biomes exist from spawn (Lowland, Midland, Highland)
- **Biome Distribution:** Interleaved within 100-150 units of origin
- **Platform Density:** 75% (dense, but varied by biome)
- **Height Range:** 2-7 units (mix of all biome heights)
- **Gaps:** 5% (rare, mostly in Highland areas)
- **Connectivity:** 90% (high connectivity, especially near origin)
- **Features:**
  - Collectibles: 15% density (abundant, guiding exploration)
  - Gates: Multiple visible from spawn (at biome boundaries)
  - Spring pads: 2% density (minimal, gentle terrain doesn't need much)
  - Landmarks: Flags, arches every 30-40 units
  - **Biome mixing:** Player experiences all three biomes naturally

### Park Size
- **Diameter:** 300-500 units from origin (large exploration area)
- **Purpose:** Full exploration area containing all three biomes at relaxed intensity
- **Exploration:** Player can freely explore within park before deciding to venture further
- **Gate placement:** Gates at park boundaries (300-500 units from origin)
- **Return point:** Player can reset to park center after falling into void in distant areas

### Park Characteristics
- **All three biomes exist** (Lowland, Midland, Highland) from spawn
- **Relaxed intensity:** Biomes are at their easiest/densest parameters
  - Lowland: Density 85%, Gaps 5%, Heights 2-4
  - Midland: Density 65%, Gaps 15%, Heights 4-7
  - Highland: Density 45%, Gaps 30%, Heights 7-12
- **Healthy mix:** Biomes interleave naturally within park
- **No pressure:** Player can explore at own pace, gates only when ready to venture further

---

## Gate System

### What Gates Are
- **Physical arches** placed at biome boundaries/edges (3 arch variants: arch, arch_tall, arch_wide)
- Each arch marks the **edge of a biome area**
- Gates are **visual indicators**, not portals or teleporters
- Gates signal: "Beyond this point, biome rules become more strict/intense"
- **Gates have code indicators** for generation system to detect biome changes

### Gate Location in Park
- **Placement:** At boundaries of park (300-500 units from origin)
- **Detection:** Based on zone map - where biome boundaries exist
- **Spacing:** One gate per boundary (multiple boundaries = multiple gates)
- **Visibility:** Gates are prominent landmarks, visible from 150-200 units within park
- **Park-internal exploration:** No gates within park - player freely explores all three biomes
- **Placement logic:**
  - Generate zone map (Lowland/Midland/Highland distribution)
  - Find boundaries between zones
  - Place arch at boundary center (or multiple arches along long boundaries)

### Gate Count
- **Total Gates:** Determined by biome distribution, not fixed number
- **Typical range:** 6-12 gates in initial park (200-300 units from origin)
- **Increases as player explores:** More gates discovered at further biome boundaries

### Gate Communication (Biome Intensity Indicator)

**Gates Indicate Biome Intensity, Not Selection:**

Park starts with **all three biomes mixed** at "relaxed" intensity levels:
- Lowland: Safe, dense (85% density)
- Midland: Balanced (65% density)
- Highland: Vertical, sparse (45% density)

**Gates mark biome edges where intensity increases:**

**Lowland Edge Gates:**
- Visual: Standard arch at ground level
- Behind gate: Lowland becomes **more strict** (density 85% → 75%, gaps 5% → 10%)
- Preview: Slightly wider platform, terrain hints at minor increase in gaps
- Communication: "Gentle terrain becomes slightly more challenging"

**Midland Edge Gates:**
- Visual: Medium-sized arch (arch or arch_tall)
- Behind gate: Midland becomes **more strict** (density 65% → 55%, gaps 15% → 25%, heights increase)
- Preview: Moderate platform, visible gap ahead
- Communication: "Balanced terrain becomes noticeably more challenging"

**Highland Edge Gates:**
- Visual: Tall/wide arch, potentially slightly elevated (requires effort to reach)
- Behind gate: Highland at **full intensity** (density 45%, gaps 30%, heights 7-12 units)
- Preview: Narrow platform, steep slope or large gap visible
- Communication: "You're entering the most challenging terrain - be prepared"

**Gate Accessibility as Intensity Indicator:**
- Ground-level gates = minimal intensity increase
- Slightly elevated gates = significant intensity increase
- **Intention:** Player can reach gate, terrain on both sides tells them what to expect
- **No teleportation:** Gates are purely visual markers, player can walk back through

### Gate Functionality

**Code Indicators:**
- Each gate has code/tag for generation system
- Tags: `GATE_LOWLAND_INCREASE`, `GATE_MIDLAND_INCREASE`, `GATE_HIGHLAND_INCREASE`
- **Gate crossing detection:** Player passes through arch bounding box
- On crossing: Update current biome parameters to "stricter" version

**Biome Intensity Transition:**

**Park Starting State (before crossing any gates):**
- Lowland: Density 85%, Gaps 5%, Heights 2-4
- Midland: Density 65%, Gaps 15%, Heights 4-7
- Highland: Density 45%, Gaps 30%, Heights 7-12

**After Crossing Lowland Gate:**
- **If entering Lowland area:** Density 85% → 75%, Gaps 5% → 10%
- **If crossing multiple Lowland gates:** Parameters can degrade further (cumulative intensity)
- **Duration:** Lasts until Lowland edge gate or biome transition

**After Crossing Midland Gate:**
- **If entering Midland area:** Density 65% → 55%, Gaps 15% → 25%, Heights 4-7 → 5-9
- **Duration:** Lasts until Midland edge gate or biome transition

**After Crossing Highland Gate:**
- **If entering Highland area:** Density 45%, Gaps 30%, Heights 7-12 (already at full intensity)
- **Duration:** Lasts until Highland edge gate or biome transition

**Reversibility:**
- **Player can walk back through gate** any time
- Crossing back: Parameters revert to previous intensity
- **No commitment:** Player is free to explore park and return through gates
- **Natural exploration:** Gates don't force player, they just indicate biome state

### Bifurcation Gates

**On exploration path (beyond initial park):**
- Occasional gates appear at biome boundaries (like in park)
- Allow intensity increase/decrease based on direction traveled
- **Purpose:** Player can discover new areas with different biome intensities
- **Discovery:** Gates appear as player approaches biome edges (not pre-known)

---

## Zone/Biome Design

### Zone Types

#### 1. Lowland Zone (Safe Exploration)

**Characteristics:**
- **Height Range:** 2-4 units (low, safe)
- **Terrain Smoothness:** 0.9 (very smooth rolling)
- **Platform Density:** 85% (dense platforms)
- **Gap Frequency:** 5% (rare gaps, only at edges/boundaries)
- **Connectivity:** 90% (highly connected, easy traversal)
- **Vertical Variety:** 70% gentle, 20% moderate, 10% vertical

**Platform Sizes:**
- Larger platforms (2x2, 4x4, 6x6)
- Decorative platforms (platform_decorative_1x1x1, platform_decorative_2x2x2)
- Arrow platforms (platform_arrow_2x2x1, platform_arrow_4x4x1)

**Slopes:**
- 70% gentle (1-3 unit rise over 4-6 units)
- Assets: platform_slope_2x2x2, platform_slope_2x4x4

**Features:**
- Collectibles: 15% density (abundant, guide exploration)
- Landmarks: Short arches, flags every 60 units
- Spring pads: 0% (not needed)
- Barriers: Minimal (10% of edges)
- Decorations: Flags, hoops, railings (20% density)

**Punishment:**
- Minimal - falling from low heights = short fall
- Usually lands on safe platform (dense coverage)

**Checkpoint System:**
- **Checkpoint density:** Every platform advanced = checkpoint
- **Rationale:** Easy levels, frequent checkpoints, no frustration

---

#### 2. Midland Zone (Balanced Gameplay)

**Characteristics:**
- **Height Range:** 4-7 units (mid-level)
- **Terrain Smoothness:** 0.6 (moderate rolling)
- **Platform Density:** 65% (moderate density)
- **Gap Frequency:** 15% (moderate gaps, distributed throughout)
- **Connectivity:** 70% (balanced connectivity, some backtracking needed)
- **Vertical Variety:** 30% gentle, 50% moderate, 20% vertical

**Platform Sizes:**
- Mix of all sizes (1x1, 2x2, 4x4, 6x6)
- Arrow platforms (platform_arrow_2x2x1, platform_arrow_4x4x1)
- Hole platforms (platform_hole_6x6x1 - rare)

**Slopes:**
- 30% gentle, 50% moderate, 20% steeper
- Assets: All slope variants (2x2x2, 4x4x4, 6x6x4, etc.)

**Features:**
- Collectibles: 10% density (guiding, not overwhelming)
- Spring pads: 3% density (for vertical traversal when needed)
- Landmarks: Mix of arches, flags, pipes
- Barriers: Moderate (15% of edges)
- Decorations: Flags, hoops, bracing, railings (8% density)
- Pipes: 5% density (structural elements)

**Punishment:**
- Moderate - longer falls, may land in gap
- May require platform to catch fall

**Checkpoint System:**
- **Checkpoint density:** Every 10-15 units (mid-way between landmarks)
- **Rationale:** Moderate difficulty, checkpoints at reasonable intervals

---

#### 3. Highland Zone (Challenging Verticality)

**Characteristics:**
- **Height Range:** 7-12 units (high, elevated)
- **Terrain Smoothness:** 0.3 (rougher, more vertical)
- **Platform Density:** 45% (sparse platforms)
- **Gap Frequency:** 30% (frequent gaps, throughout)
- **Connectivity:** 50% (challenging, many isolated islands)
- **Vertical Variety:** 10% gentle, 30% moderate, 60% vertical

**Platform Sizes:**
- Smaller platforms (1x1, 2x2)
- Decorative platforms (platform_decorative_1x1x1)
- Arrow platforms (platform_arrow_2x2x1 - guidance)

**Slopes:**
- 60% steep/vertical, 30% moderate, 10% gentle
- Assets: Steeper slopes (platform_slope_6x2x2, platform_slope_6x4x4, etc.)

**Features:**
- Collectibles: 12% density (reward exploration, high points)
- Spring pads: 8% density (essential for vertical traversal)
- Elevated platforms: 10% density (jump-off points, 2.5 units above terrain)
- Landmarks: Tall arches, pillars (landmarks for navigation)
- Barriers: High (20% of edges, harder to bypass)
- Decorations: Bracing, struts, structural (5% density)
- Pipes: 8% density (industrial feel, structural)

**Punishment:**
- Severe - long falls (up to 12 units)
- High likelihood of falling to void (sparse coverage)
- Requires precise platforming

**Checkpoint System:**
- **Checkpoint density:** No checkpoints (or very rare, major landmarks only)
- **Rationale:** Hardest levels, no checkpoints, high stakes

---

### 4. Transition Zone (Blending)

**Purpose:** Smoothly blend biome characteristics at zone boundaries

**Characteristics:**
- Linear interpolation of zone parameters
- Blend factor: 0.0 (FromZone) to 1.0 (ToZone)
- Transition length: 2-3 chunks (200-300 units)

**Blended Parameters:**
- Height: Lerp between zone ranges
- Platform density: Lerp between densities
- Gap frequency: Lerp between frequencies
- Smoothness: Lerp between smoothness values
- Platform size mix: Blend between zone-specific mixes

**Visual Indication:**
- Subtle terrain change (not abrupt)
- Arch landmarks at transition boundaries
- Collectible density changes gradually

---

### Zone Placement Strategy (Interleaved Biomes)

**Multi-Layer Noise for Zone Distribution:**

**Layer 1: Base Zone Distribution** (Coarse noise)
- Frequency: 0.02
- Purpose: Determine general biome regions
- Result: Large swaths of Lowland/Midland/Highland

**Layer 2: Fine Detail** (Higher frequency noise)
- Frequency: 0.1
- Purpose: Interleave biomes, avoid monolithic blocks
- Result: Biome changes within 50-100 units (healthy mix)

**Zone Determination:**
- Combine noise layers for zone type per position
- Not monolithic: Biomes interleave naturally
- Healthy mix: Player experiences all three biomes in single exploration

**Transition Detection:**
- Where zone grid values differ between adjacent cells
- Automatically generate transition zone
- Blend parameters over 2-3 chunks

---

## Infinite Generation System

### Generation Flow

```
Seed
  ↓
Zone Map Generation (multi-layer noise)
  ├─ Heightmap: base terrain height
  ├─ ZoneMap: zone type per position (Lowland/Midland/Highland)
  └─ FeatureMap: platform density, gap frequency
  ↓
Chunk-Based Generation (100x100 units per chunk)
  ├─ For each chunk: determine zone type
  ├─ Generate platforms based on zone characteristics
  ├─ Generate rolling ramps (continuous slopes)
  ├─ Generate elevated platforms (jump-off points)
  ├─ Generate gaps (based on zone gap frequency)
  └─ Add decorations/landmarks/collectibles
  ↓
Connectivity Validation
  ├─ Ensure no isolated platforms (below target connectivity)
  ├─ Add connectors between disconnected areas
  └─ Validate reachable paths
  ↓
Final Tile Array (render)
```

### Rolling Ramp Generation

**Goal:** Continuous slopes you can roll over, not jump between

**Generation Rules:**
- Find platform edges with height differences (0.5 to 3.0 units)
- Determine ramp direction between platforms
- Select ramp asset based on height difference:
  - Gentle (< 1.0 unit): platform_slope_2x2x2
  - Moderate (1.0-2.0 units): platform_slope_4x2x2
  - Steeper (2.0-3.0 units): platform_slope_6x2x2
- Create continuous ramp (multiple slope pieces for smooth rolling)
- Connect start and end platforms seamlessly

**Continuous Ramp Construction:**
- Calculate distance between platforms
- Determine number of slope pieces needed
- Place slope pieces with overlapping ends (no gaps)
- Adjust height for smooth transition (lerp Y positions)
- Optional: Add slight sine wave offset for smoother feel

### Elevated Platforms (Jump-Off Points)

**Purpose:** High places to jump from, can fall to terrain or void

**Generation Rules:**
- Only in higher zones (Midland/Highland)
- Select high platforms (> 6 units in Midland, > 8 units in Highland)
- Check space above is clear (no collision)
- Create elevated platform 2.5 units above terrain
- Platform size: Smaller (1x1 or 2x2)
- Mark as jump-off point (for gameplay logic)

**Jump-Off Mechanics:**
- Player can jump from elevated platform
- Fall back to terrain (if lucky, lands on platform)
- Fall to void (if unlucky or misjudged jump)
- High risk, high reward (collectibles on elevated platforms)

### Gap Generation

**Based on Zone:**

**Lowland:**
- Frequency: 5%
- Placement: Only at edges/boundaries (zone transitions, terrain changes)
- Size: 1-2 units (small gaps, easy to cross)
- Safety: Dense platform coverage means landing likely

**Midland:**
- Frequency: 15%
- Placement: Sprinkled throughout, not concentrated
- Size: 2-4 units (moderate gaps, require precision)
- Safety: Moderate coverage, some gaps risky

**Highland:**
- Frequency: 30%
- Placement: Throughout (frequent gaps, dangerous)
- Size: 3-6 units (large gaps, require skill)
- Safety: Sparse coverage, many gaps lead to void

**Critical Path Protection:**
- Don't create gaps that would isolate areas
- Don't remove tiles that are only connectors between regions
- Maintain minimum connectivity per zone target

---

## Chunk System

### Chunk Specifications
- **Chunk Size:** 100x100 units
- **Chunk Shape:** Square
- **Coordinate System:** (chunkX, chunkZ) for chunk position in world

### Chunk Loading Strategy: Hybrid (C)
- **Pre-generate 1-3 chunks** around player on spawn
- **Generate on-demand** for new chunks during exploration
- **Async load ahead:** Generate chunks player is approaching (not visible yet)
- **Result:** Balance between seamless experience and memory usage

### Chunk Generation Requirements
- **Speed:** "Fast AF" - Not perceivable to user on Android
- **Target Generation Time:** < 500ms per chunk
- **Optimization:**
  - Chunk caching (don't regenerate same chunk unless reset)
  - Terrain heightmap caching (reuse noise calculations)
  - Simplified collision checks during generation
  - Pre-calculated platform patterns per zone

### Chunk Management

**Load Logic:**
- Player enters new chunk → check if neighbors are generated
- If neighbor not generated → trigger async generation
- Return immediately (don't block on chunk generation)
- Show loading indicator if player approaches unloaded chunk

**Unload Logic:**
- Keep 3x3 chunk area around player (9 chunks)
- Unload distant chunks (> 2 chunks away)
- Save chunk state if modified (e.g., collectibles collected)
- Graceful fallback: If generation fails, show placeholder terrain

**Seed-Based Chunk Generation:**
- **Base seed:** Global world seed
- **Chunk seed offset:** `seed + chunkX * 1000 + chunkZ`
- **Dynamic mode (optional):** `seed + chunkX * 1000 + chunkZ + adjacentChunkHashes`
- **Reproducibility:** Same chunk coordinates + same seed = identical terrain

---

## Checkpoint System

### Reset Triggers
1. **Manual Reset:** Button/UI option (quick return to last checkpoint or hub)
2. **Fall to Void:** Fall below Y=0 or Y=-5 (punishment for misjudged jump)
3. **Checkpoint Activation:** Player lands on checkpoint-marked platform

### Reset Behavior
- **Lowland (Easy):** Reset to last checkpoint platform
  - Every platform advanced = checkpoint
  - Minimal punishment, frustration-free
- **Midland (Moderate):** Reset to mid-way checkpoint
  - Checkpoints every 10-15 units
  - Moderate punishment
- **Highland (Hard):** Reset to hub origin (or rare major checkpoint)
  - No checkpoints (or very rare at major landmarks)
  - Severe punishment for mistakes

### Checkpoint Implementation
- **Checkpoint Markers:** Invisible (no visual clutter)
- **Activation:** Automatic when player lands on platform
- **Storage:** Platform ID + position + player state (rotation, velocity)
- **Reset Transition:** Fade out → fade in at checkpoint (smooth)

### Reset to Hub
- **Trigger:** Player chooses "Return to Hub" from UI or falls from void in hub
- **Behavior:** Instant teleport to (0, 4, 0)
- **State:** Clear any accumulated progress (but keep player stats)
- **Visual:** Fade effect + camera pan from current position back to hub

---

## Save System

### Save Type 1: Current World State

**What Gets Saved:**
- **Seed:** Main world seed
- **Seed Mode:** Single seed vs. chunk hash (dynamic)
- **Player Position:** Exact (X, Y, Z)
- **Player Rotation:** Current facing direction
- **Camera State:** Position and target (for consistent view)
- **Loaded Chunks:** List of chunk coordinates (for quick reload)
- **Chunk States:** For each chunk, store:
  - Chunk seed
  - Zone type for each chunk
  - Checkpoints activated (platform IDs)
  - Collectibles collected (if tracking per-chunk)

**What DOESN'T Get Saved:**
- Full tile arrays (too large, regenerate from chunk seed)
- Decoration states (regenerate from chunk seed)
- Physics state (re-simulated on load)
- Player velocity/momentum (reset to zero on load)

**Save Size Estimate:** ~1-10 KB (very small)

**Load Behavior:**
1. Read seed, seed mode, player position
2. Load chunks around player position (quickly regenerate from cached chunk seeds)
3. Place player at saved position
4. Restore checkpoint state
5. Continue gameplay seamlessly (< 500ms load time)

---

### Save Type 2: Finite Mode Recording

**Concept:** "Snapshot of exploration journey" as a replayable level

**What Gets Recorded:**

**Core Path Data:**
- Entry gate (which arch passed through, biome type)
- Sequence of chunks visited (chunk coordinates)
- **Checkpoint-level granularity:** Which checkpoints were reached
- Key decision points: When player chose which branch/path to take
- Timestamp or distance markers (for pacing)

**Platform/Tile "Highlights":**
- For each chunk visited, record which **platform types** were used
- Not tile-level detail, but chunk-level categorization:
  - Primary path platforms (stepped on)
  - Secondary path platforms (visible but unused)
  - Elevated platforms (jump-off points reached)
- This captures "wider blocks around player's path" without storing every tile

**Interactions:**
- Collectibles collected (which ones, which chunks)
- Spring pads used (which positions)
- Special platforms (arches, elevated platforms) interacted with
- Falls taken (from position, to position or void)

**Terrain Features Along Path:**
- Gaps encountered (which chunks)
- Ramp sequences used (slope types, lengths)
- High points reached (Y positions)
- "Near misses" (platforms player almost stepped on but didn't)

**Metadata:**
- Total distance traveled
- Time elapsed
- Biome sequence (if multi-biome world)
- Estimated difficulty (inferred from terrain encountered)
- Gate entry point (which arch started the journey)

**Storage Format:**
```
FiniteLevel {
  Name: string  // User-given or auto-generated ("Lowland Exploration #3")
  Date: DateTime
  PlayTime: TimeSpan
  Biome: ZoneType (or array if multi-biome)
  OriginalSeed: int
  OriginalChunkMode: Single | Dynamic

  Path: {
    EntryGate: Vector3
    GateBiome: ZoneType
    CheckpointsReached: ChunkCoord[]
    KeyPoints: {Position: Vector3, Type: DecisionPoint|Landmark}[]
    PrimaryChunks: ChunkCoord[]  // Chunks with primary path
    SecondaryChunks: ChunkCoord[]  // Chunks visible but unused
  }

  Interactions: {
    CollectiblesCollected: {ChunkCoord, CollectibleId}[]
    SpringPadsUsed: ChunkCoord[]
    Falls: {FromChunk: ChunkCoord, ToChunk: ChunkCoord option}[]  // None if void
  }

  TerrainFeatures: {
    GapsEncountered: ChunkCoord[]
    RampSequencesUsed: {ChunkCoord, SlopeType, Length}[]
    HighPointsReached: {ChunkCoord, Height: float32}[]
  }

  Stats: {
    TotalDistance: float32
    TotalTime: float32
    EstimatedDifficulty: float32  // 0.0 to 1.0 inferred
  }
}
```

**Save Size Estimate:** ~10-100 KB (depends on path length)

**Load Behavior (Finite Mode Playback):**

1. **Regenerate Terrain:**
   - Use original seed + path constraints
   - Generate chunks for primary path (high priority)
   - Generate secondary chunks with lower density
   - Ensure gaps/ramps match recorded sequence

2. **Place Player:**
   - At entry gate position
   - With appropriate rotation (facing direction)

3. **Replay Modes Available:**

   **A) Fixed Replay:**
   - Regenerate exactly same terrain
   - Same checkpoint sequence
   - Player can replay exact journey

   **B) Challenge Replay:**
   - Same terrain, same primary path
   - Secondary terrain may have different layout (but same biome)
   - Try to find faster/better path through secondary terrain
   - Time trial mode (race against clock)

   **C) Variant Replay:**
   - Same biome, but different path options
   - Primary path constraints relaxed
   - Secondary terrain fully regenerated
   - Exploration mode (discover new paths)

   **D) All Modes:**
   - Player chooses replay type on load
   - Menu: "Replay Exact" | "Challenge" | "Explore Variant"

**Advantages:**
- Captures player's unique exploration
- Shareable with others
- Different ways to replay (fixed, challenge, variant)
- Deterministic (same seed + same checkpoints = same terrain)

**Disadvantages:**
- More complex save format
- Requires good checkpoint tracking during play

---

## Seed System

### Configurable Seed Modes

**Mode 1: Single Seed (Default)**
- **Description:** Entire infinite world generated from one seed
- **Reproducibility:** Perfect (same seed = identical world)
- **Chunk Generation:** `chunkSeed = worldSeed + chunkX * 1000 + chunkZ`
- **Advantage:** Perfect reproducibility, easy sharing
- **Disadvantage:** Less variety between chunks

**Mode 2: Dynamic Chunk Hash (Optional)**
- **Description:** Each chunk has unique seed based on coordinates
- **Reproducibility:** High (same coordinates = same chunk, but chunks differ from each other)
- **Chunk Generation:** `chunkSeed = worldSeed + hash(chunkX, chunkZ, adjacentChunks)`
- **Hash Function:**
  - Option A: Hash only chunk coordinates
    ```fsharp
    hash(chunkX, chunkZ) = (chunkX * 73856093 + chunkZ) ^ 31
    chunkSeed = worldSeed + hash(chunkX, chunkZ)
    ```
  - Option B: Hash chunk + time (not recommended - breaks reproducibility)
  - Option C: Hash chunk + adjacent chunk hashes (smooth transitions)
    ```fsharp
    hash = worldSeed + hash(chunkX, chunkZ) + hash(north) + hash(east) + hash(south) + hash(west)
    ```
- **Advantage:** More variety, dynamic feel
- **Disadvantage:** Slightly more complex, larger save state

**User Selection:**
- **Default:** Single seed mode
- **Optional:** Dynamic mode for more variety
- **Choice Made:** At game start or hub (not mid-game)

---

## Asset Utilization

### Platform Usage by Zone

**Lowland (67% utilization in this zone):**
- Platforms: 2x2, 4x4, 6x6 (all heights: 1, 2)
- Decorative: platform_decorative_1x1x1, platform_decorative_2x2x2
- Arrows: platform_arrow_2x2x1, platform_arrow_4x4x1
- Slopes: platform_slope_2x2x2, platform_slope_2x4x4

**Midland (80% utilization in this zone):**
- Platforms: All sizes (1x1 through 6x6, all heights)
- Arrows: platform_arrow_2x2x1, platform_arrow_4x4x1
- Hole: platform_hole_6x6x1 (rare)
- Slopes: All 9 slope variants

**Highland (50% utilization in this zone):**
- Platforms: 1x1, 2x2, 4x2 (all heights: 1, 2)
- Decorative: platform_decorative_1x1x1
- Arrows: platform_arrow_2x2x1 (guidance)
- Slopes: Steeper variants (platform_slope_6x2x2, platform_slope_6x4x4)

### Feature Usage by Zone

**Lowland:**
- Arches: Short arches (arch) - every 60 units as landmarks
- Flags: flag_A, flag_B, flag_C - every 30 units
- Hoops: hoop, hoop_angled - rare (5% of decorations)
- Barriers: barrier_1x1x1, barrier_1x1x2 - minimal edges
- Collectibles: star, heart, diamond - abundant (15% density)

**Midland:**
- Arches: Mix of arch, arch_tall, arch_wide - landmarks every 80 units
- Flags: flag_A, flag_B, flag_C - moderate density
- Pipes: pipe_straight_A/B, pipe_90_A/B - structural (5% density)
- Spring pads: spring_pad - vertical traversal (3% density)
- Collectibles: star, heart, diamond - guiding (10% density)
- Railings: railing_straight_single, railing_corner_single - moderate (8% density)
- Bracing: bracing_small, bracing_medium - structural (5% density)

**Highland:**
- Arches: Tall arches (arch_tall, arch_wide) - navigation aids
- Pillars: pillar_1x1x1 through pillar_2x2x8 - landmarks
- Spring pads: spring_pad - essential (8% density)
- Collectibles: star, heart, diamond - reward (12% density)
- Pipes: pipe_straight_A/B, pipe_end - industrial feel (8% density)
- Barriers: barrier_2x1x1, barrier_3x1x2 - high obstacles (20% of edges)
- Bracing: bracing_medium, bracing_large - structural support (5% density)
- Struts: strut_horizontal, strut_vertical - high platforms (3% density)

### Overall Utilization Target
- **Lowland Zone:** ~70% of 373 assets
- **Midland Zone:** ~80% of 373 assets
- **Highland Zone:** ~60% of 373 assets (fewer platform types, more structural)
- **Overall Infinite World:** ~75% average utilization

---

## Technical Requirements

### Performance Targets (Android)
- **Chunk Generation:** < 500ms (not perceivable to user)
- **Chunk Loading:** Seamless (no visible loading, async in background)
- **Visible Tiles:** < 2000 active tiles (distance culling)
- **Memory:** < 200MB total
- **Frame Rate:** 60fps steady on target devices
- **Load Time (Save/Load):** < 2s for finite modes

### Elmish Integration
- **Async Generation:** Dispatch messages to generate chunks asynchronously
- **Quick Resolution:** Chunks must resolve quickly to avoid blank spaces
- **Hybrid Strategy:** Pre-generate 1-3 chunks immediately, async load ahead
- **Message Types:**
  - `GenerateChunk` (coord, seed, zoneType)
  - `ChunkGenerated` (coord, tiles[])
  - `UnloadChunk` (coord)
  - `SaveWorld` (worldState)
  - `LoadWorld` (saveData)

### Distance Culling
- **View Distance:** 150 units (based on player position)
- **Frustum Culling:** Only render tiles in camera view
- **Path-based Filtering:** Limit tiles per path to +/- 10 segments from player
- **Result:** ~1000-1500 visible tiles (well under 2000 target)

---

## Implementation Priorities

### Phase 1: Core Infrastructure (Foundation)
1. Zone type definitions (Lowland, Midland, Highland, Transition)
2. Multi-layer noise system (heightmap, zone map, feature map)
3. Chunk generation framework (100x100 units, fast generation)
4. Platform placement logic (density-based, zone-specific)
5. Asset selection by zone (platform sizes, decoration types)

### Phase 2: Hub & Gates
6. Hub generation (park-like, dense, safe)
7. Gate placement (6-8 gates, biome combinations)
8. Gate accessibility mechanics (ground-level vs. elevated)
9. Preview platform generation (5-10 units past each gate)
10. Gate transition logic (biome change over 3-5 chunks)

### Phase 3: Terrain Generation
11. Rolling ramp generation (continuous slopes)
12. Elevated platform generation (jump-off points)
13. Gap generation (zone-based frequency)
14. Connectivity validation (no isolated platforms)
15. Connector generation (between disconnected areas)

### Phase 4: Features & Decorations
16. Collectible placement (density-based, zone-specific)
17. Landmark generation (arches, flags, pipes, pillars)
18. Spring pad generation (vertical traversal, zone-specific)
19. Barrier/bracing placement (edge details, zone-specific)
20. Interactive elements (buttons, levers - optional for future)

### Phase 5: Chunk System
21. Chunk management (load/unload, caching)
22. Hybrid generation strategy (pre-generate + async)
23. Seed-based chunk generation (single + dynamic modes)
24. Chunk state persistence (checkpoints, collectibles)
25. Distance culling optimization

### Phase 6: Save/Load System
26. World state save format (seed, position, chunks, checkpoints)
27. Finite mode recording format (path, interactions, features)
28. Save UI/integration (save type selection)
29. Load UI/integration (replay mode selection)
30. Persistence layer (file I/O, serialization)

### Phase 7: Checkpoint System
31. Checkpoint activation logic (zone-based density)
32. Reset behavior (lowland/midland/highland differences)
33. Reset to hub (void fall punishment)
34. Checkpoint persistence (in save data)
35. Reset transitions (fade effects, camera pan)

### Phase 8: Polish & Optimization
36. Asset utilization tuning (reach 75%+ target)
37. Performance profiling (Android target devices)
38. Generation speed optimization (< 500ms per chunk)
39. Visual polish (fade transitions, loading indicators)
40. Testing & balancing (biome characteristics, difficulty feel)

---

## Questions for Implementation

1. **Chunk hash algorithm:** Option A (coords only), B (coords + time - break reproducibility), or C (coords + neighbors)?
2. **Gate transition visual effect:** Fade, particle effect, or seamless?
3. **Checkpoint persistence:** Should checkpoints save between game sessions?
4. **Finite mode shareability:** Can players share saved finite levels with others?
5. **Dynamic mode default:** Should dynamic mode be opt-in or default for new players?
6. **Arch asset usage:** All 3 arch variants (arch, arch_tall, arch_wide) or specific variants for specific purposes?

---

## Success Metrics

### Gameplay
- **Park-like feel:** Rolling terrain, exploration, not linear track
- **Biome variety:** Player experiences all three biomes within 1000 units of exploration
- **Implicit difficulty:** Player understands difficulty through biome characteristics (no UI setting needed)
- **Gate communication:** Higher gates clearly indicate difficulty through accessibility and preview
- **Healthy mix:** Interleaved biomes, not monolithic blocks

### Performance
- **Chunk generation:** < 500ms (Android target)
- **Seamless loading:** No blank spaces visible to player
- **60fps:** Steady frame rate with < 2000 visible tiles
- **Memory:** < 200MB on Android
- **Save/Load:** < 2s load time for finite modes

### Asset Utilization
- **Overall:** 75% of 373 assets (280+ assets used)
- **By Zone:**
  - Lowland: 70% (platforms, decorations, collectibles)
  - Midland: 80% (all platforms, slopes, pipes, spring pads)
  - Highland: 60% (smaller platforms, pillars, structural elements)

### Save System
- **World save:** < 10KB, < 1s load time
- **Finite save:** < 100KB, < 2s load time
- **Replayability:** 3 modes (fixed, challenge, variant)
- **Reproducibility:** Same seed = same world (or chunk, if dynamic)

---

## Conclusion

This design creates a cohesive **infinite park-like platformer** experience with:

- **Natural exploration:** Open world, multiple directions, rolling terrain
- **Implicit difficulty:** Biome characteristics communicate difficulty without explicit settings
- **Hub-based structure:** Central starting area with gate choices
- **Interleaved biomes:** Healthy mix of Lowland/Midland/Highland
- **Checkpoint system:** Zone-appropriate punishment (frequent to none)
- **Save flexibility:** World state + finite mode recording
- **Performance:** Fast chunk generation, seamless async loading

The design balances **player choice** (gate selection) with **natural consequences** (biome characteristics) to create meaningful exploration without arbitrary difficulty settings.
