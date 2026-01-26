namespace FsBump.Core

open System
open Microsoft.Xna.Framework

module InfiniteMode =

  let generate
    (env: #IRandomProvider & #IModelStoreProvider)
    (pathGraph: PathGraph)
    (playerPos: Vector3)
    (obstacles: Tile[])
    (config: GenerationConfig)
    =

    let mutable graph = pathGraph
    let newTiles = ResizeArray<Tile>()

    // 1. Cleanup distant paths (paths far behind or too far ahead on side branches)
    graph <- PathGraphSystem.cleanupDistantPaths graph playerPos 150.0f

    // 2. Process active paths
    let activePaths = PathGraphSystem.getActivePaths graph

    for path in activePaths do
      // Only generate if player is relatively close to the end of this path branch
      // 80 units is roughly 10-15 typical platforms/tiles
      if Vector3.Distance(playerPos, path.Position) < 80.0f then

        let rec retry attempt =
          let segmentType = env.Random.Next(0, 4)
          let length = env.Random.Next(10, 20)
          let modeName = TerrainAssets.Infinite

          let tiles, endState =
            match segmentType with
            | 1 -> SegmentStrategies.slope env length path modeName
            | 2 -> SegmentStrategies.platform env (length / 2) path modeName
            | _ -> SegmentStrategies.flatRun env length path modeName

          // Turn logic
          let nextState =
            {
              endState with
                  CurrentColor = TerrainAssets.getNextColor path.CurrentColor
            }
            |> StateOps.turn(
              match env.Random.Next(0, 3) with
              | 1 -> MathHelper.PiOver2
              | 2 -> -MathHelper.PiOver2
              | _ -> 0.0f
            )

          // Overlap check
          if not(Validation.checkOverlap (tiles) obstacles config) then
            // Success
            newTiles.AddRange(tiles)
            graph <- PathGraphSystem.updatePathState graph nextState

            // Branching logic: 10% chance, but cap at 3 active paths
            if activePaths.Length < 3 && env.Random.NextDouble() < 0.1 then
              let branchDir =
                if env.Random.NextDouble() < 0.5 then
                  Vector3.Transform(
                    nextState.Direction,
                    Matrix.CreateRotationY(MathHelper.PiOver2)
                  )
                else
                  Vector3.Transform(
                    nextState.Direction,
                    Matrix.CreateRotationY(-MathHelper.PiOver2)
                  )

              let branchDirRounded =
                Vector3(
                  MathF.Round branchDir.X,
                  MathF.Round branchDir.Y,
                  MathF.Round branchDir.Z
                )

              graph <-
                PathGraphSystem.addBranch
                  graph
                  nextState.Id
                  nextState.Position
                  branchDirRounded

          elif attempt < 5 then
            retry(attempt + 1)
          else
            // Fallback: short flat run
            let t, s = SegmentStrategies.flatRun env 5 path modeName
            newTiles.AddRange(t)

            graph <-
              PathGraphSystem.updatePathState graph {
                s with
                    PreviousDirection = path.Direction
              }

        retry 0

    newTiles.ToArray(), graph
