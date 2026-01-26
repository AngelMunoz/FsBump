namespace FsBump.Core

open System
open Microsoft.Xna.Framework

// ─────────────────────────────────────────────────────────────
// Vector Math Helpers (Explicit to avoid ambiguity)
// ─────────────────────────────────────────────────────────────

module VectorMath =
  let mul (v: Vector3) (f: float32) = Vector3(v.X * f, v.Y * f, v.Z * f)

  let add (a: Vector3) (b: Vector3) =
    Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z)

  let create (x: float32) (y: float32) (z: float32) = Vector3(x, y, z)

// ─────────────────────────────────────────────────────────────
// Validation
// ─────────────────────────────────────────────────────────────

module Validation =
  let getTileBounds (t: Tile) (buffer: float32) =
    let half = t.Size * 0.5f + VectorMath.create buffer buffer buffer
    BoundingBox(t.Position - half, t.Position + half)

  let checkOverlap
    (newTiles: Tile list)
    (obstacles: Tile list)
    (config: GenerationConfig)
    =
    let obstacleBoxes =
      obstacles |> List.map(fun t -> getTileBounds t config.SafetyBuffer)

    let checkBoxes = newTiles |> List.map(fun t -> getTileBounds t 0.0f)

    checkBoxes
    |> List.exists(fun cb ->
      obstacleBoxes |> List.exists(fun ob -> cb.Intersects(ob)))

// ─────────────────────────────────────────────────────────────
// State Operations
// ─────────────────────────────────────────────────────────────

module StateOps =
  let advance dist (state: PathState) = {
    state with
        Position =
          VectorMath.add state.Position (VectorMath.mul state.Direction dist)
        DistanceFromStart = state.DistanceFromStart + dist
  }

  let moveY offset (state: PathState) = {
    state with
        Position =
          VectorMath.add state.Position (VectorMath.create 0.0f offset 0.0f)
  }

  let turn angle (state: PathState) =
    let nextDir =
      Vector3.Transform(state.Direction, Matrix.CreateRotationY(angle))

    let roundedDir =
      VectorMath.create
        (MathF.Round(nextDir.X))
        (MathF.Round(nextDir.Y))
        (MathF.Round(nextDir.Z))

    {
      state with
          Direction = roundedDir
          PreviousDirection = state.Direction
    }
