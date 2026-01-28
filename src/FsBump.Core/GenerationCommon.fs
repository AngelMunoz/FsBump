namespace FsBump.Core

open System
open Microsoft.Xna.Framework

// ─────────────────────────────────────────────────────────────
// Validation
// ─────────────────────────────────────────────────────────────

module Validation =
  let getTileBounds (t: Tile) (buffer: float32) =
    let half = t.Size * 0.5f + Vector3(buffer, buffer, buffer)
    BoundingBox(t.Position - half, t.Position + half)

  let checkOverlap
    (newTiles: Tile array)
    (obstacles: Tile array)
    (config: GenerationConfig)
    =
    let obstacleBoxes =
      obstacles |> Array.map(fun t -> getTileBounds t config.SafetyBuffer)

    let checkBoxes = newTiles |> Array.map(fun t -> getTileBounds t 0.0f)

    checkBoxes
    |> Array.exists(fun cb ->
      obstacleBoxes |> Array.exists(fun ob -> cb.Intersects(ob)))

// ─────────────────────────────────────────────────────────────
// State Operations
// ─────────────────────────────────────────────────────────────

module StateOps =
  let advance dist (state: PathState) = {
    state with
        Position = state.Position + state.Direction * dist
        DistanceFromStart = state.DistanceFromStart + dist
  }

  let moveY offset (state: PathState) = {
    state with
        Position = state.Position + Vector3(0.0f, offset, 0.0f)
  }

  let turn angle (state: PathState) =
    let nextDir =
      Vector3.Transform(state.Direction, Matrix.CreateRotationY(angle))

    let roundedDir =
      Vector3(
        MathF.Round nextDir.X,
        MathF.Round nextDir.Y,
        MathF.Round nextDir.Z
      )

    {
      state with
          Direction = roundedDir
          PreviousDirection = state.Direction
    }
