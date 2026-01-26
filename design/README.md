# Design Notes

## Infinite Park World Design

The original `PROCEDURAL_MAP_REFACTORING_PLAN.md` described a linear path-based approach. After design discussion, we're pivoting to an **open-world park-like platformer** with:

- **Large starting park (300-500 units)** - contains all three biomes mixed at relaxed intensity
- Park exploration within park freely, gates only at park boundaries
- **Gates are visual indicators** - not portals, mark where biome rules become stricter
- Interleaved biomes (Lowland/Midland/Highland) within park
- Rolling continuous terrain (slopes you can roll over)
- Implicit difficulty through biome characteristics (no difficulty setting)
- Chunk-based infinite generation with async Elmish messages
- Checkpoint system with zone-appropriate punishment
- Two save types: world state + finite mode recording

**Full design document:** `infinite-park-world-design.md`

---

## Design Evolution

### Original Approach (Superseded)
- Linear infinite track with branching
- Single path with 90° turns
- 9-10 assets utilized (~2.7%)
- Explicit difficulty settings
- Finite modes only

### New Approach (Current Design)
- Open-world park-like terrain
- Multiple exploration directions
- 75%+ asset utilization target
- Implicit difficulty (biome characteristics)
- Infinite + finite modes (record exploration as finite level)

---

## Key Design Decisions

### 1. Biomes Instead of Modes
- **Old:** Infinite, Exploration, Challenge modes
- **New:** Lowland, Midland, Highland biomes (interleaved in single world)

### 2. Gate Selection Over Mode Selection
- **Old:** UI mode menu
- **New:** Physical gates in hub, run through to choose

### 3. Implicit Over Explicit Difficulty
- **Old:** Difficulty slider/setting
- **New:** Gate accessibility + biome characteristics communicate difficulty

### 4. Chunk-Based Over Path-Based
- **Old:** Path graph with nodes/edges
- **New:** Chunk grid (100x100 units) with zone-based generation

### 5. Hybrid Generation Strategy
- **Old:** On-demand only
- **New:** Pre-generate 1-3 chunks + async ahead (seamless, no blank spaces)

---

## Status

**Design Phase:** ✅ COMPLETE
**Implementation Phase:** 🔴 NOT STARTED

**Next Steps:**
1. Review and approve `infinite-park-world-design.md`
2. Review and approve `implementation-plan.md` (technical specification)
3. Begin Phase 1 implementation (Foundation Types)
4. Implement phases incrementally with testing on Android

**Technical Documents:**
- `infinite-park-world-design.md` - Design specification (954 lines)
- `implementation-plan.md` - Type-driven implementation plan (500+ lines, concise)

**Type-Driven Approach:**
- Closed sets (small variants) use DU (e.g., GateVariant = Arch | Tall | Wide)
- Open sets use unit-of-measure (e.g., position: Vector3<WorldUnits>)
- No code in plan - types define system behavior
- Types sufficient to guide implementation naturally

---

## Files

- `infinite-park-world-design.md` - Full design specification
- `PROCEDURAL_MAP_REFACTORING_PLAN.md` - Original plan (superseded, kept for reference)
