namespace FsBump.Core

open System
open Microsoft.Xna.Framework
open Mibo.Rendering.Graphics3D

// ─────────────────────────────────────────────────────────────
// Map Generator
// ─────────────────────────────────────────────────────────────

module MapGenerator =

  let createInitialState (mode: GameMode) (seed: int) =
    PathGraphSystem.initialize mode seed

  let getSpawnPoint() = Vector3(0.0f, 4.0f, 0.0f)

  module Operations =

    /// Handle map generation and cleanup
    let updateMap
      (env: #IRandomProvider & #IModelStoreProvider)
      (playerPosition: Vector3)
      (map: Tile array)
      (pathGraph: PathGraph)
      =

      let newTiles, nextGraph =
        match pathGraph.Mode with
        | Infinite ->
          let config = {
            MaxJumpHeight = 2.8f
            MaxJumpDistance = 7.0f
            SafetyBuffer = 0.1f
          }

          let obstacles =
            map
            |> Array.filter(fun t ->
              // Optimize collision check to nearby tiles
              Vector3.DistanceSquared(t.Position, playerPosition) < 2500.0f)
            |> Array.toList

          InfiniteMode.generate env pathGraph playerPosition obstacles config

        | _ ->
          // Placeholder for finite modes
          [||], pathGraph

      let updatedMap = Array.append map newTiles

      // Optimization: Cleanup tiles that are far behind or way too far ahead
      let finalMap =
        updatedMap
        |> Array.filter(fun t ->
          Vector3.DistanceSquared(t.Position, playerPosition) < 14400.0f) // 120 units radius

      if finalMap.Length <> map.Length || newTiles.Length > 0 then
        ValueSome(finalMap, nextGraph)
      else
        ValueNone

  let draw
    (env: #IModelStoreProvider)
    (frustum: BoundingFrustum)
    (playerPos: Vector3)
    (map: Tile array)
    (buffer: PipelineBuffer<RenderCommand>)
    =
    // 1. Find player's current segment context
    // We look for the tile the player is closest to or currently over
    let playerContext =
      map
      |> Array.tryFind(fun t ->
        let distSq = Vector3.DistanceSquared(t.Position, playerPos)
        distSq < 16.0f) // Within 4 units
      |> Option.map(fun t -> t.PathId, t.SegmentIndex)

    buffer
      .DrawMany(
        [|
          for i = 0 to map.Length - 1 do
            let tile = map.[i]

            // 2. Filter by platform count (at most 10 in front and behind)
            let isVisible =
              match playerContext with
              | Some(pId, sIndex) ->
                // Same path: +/- 10 platforms
                if tile.PathId = pId then
                  abs(tile.SegmentIndex - sIndex) <= 10
                else
                  // Different path (branch): visible if close to branch point (distance fallback)
                  Vector3.DistanceSquared(tile.Position, playerPos) < 400.0f
              | None ->
                // No context: fallback to distance cull
                Vector3.DistanceSquared(tile.Position, playerPos) < 1600.0f

            if isVisible then
              let halfSize = tile.Size * 0.5f

              let box =
                BoundingBox(tile.Position - halfSize, tile.Position + halfSize)

              if frustum.Intersects(box) then
                env.ModelStore.GetMesh(Assets.getAsset tile)
                |> ValueOption.bind(fun m -> draw {
                  mesh m
                  at tile.VisualOffset

                  rotatedBy(
                    Quaternion.CreateFromAxisAngle(Vector3.Up, tile.Rotation)
                  )

                  relativeTo(Matrix.CreateTranslation(tile.Position))
                })
        |]
      )
      .Submit()
