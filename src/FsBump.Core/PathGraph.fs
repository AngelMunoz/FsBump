namespace FsBump.Core

open System
open Microsoft.Xna.Framework
open FSharp.UMX

module PathGraphSystem =

  let createPath
    (id: Guid<PathId>)
    (startPos: Vector3)
    (direction: Vector3)
    (isMain: bool)
    (parent: Guid<PathId> voption)
    =
    {
      Id = id
      Position = startPos
      Direction = direction
      PreviousDirection = direction
      Width = 2
      CurrentColor = ColorVariant.Blue
      IsActive = true
      IsMainPath = isMain
      ParentPathId = parent
      ConvergencePathId = ValueNone
      DistanceFromStart = 0.0f
      NextSegmentIndex = 0
    }

  let initialize (mode: GameMode) (seed: int) =
    let startPos = Vector3(0.0f, 4.0f, 0.0f)
    let mainId = %Guid.NewGuid()
    let mainPath = createPath mainId startPos (-Vector3.UnitZ) true ValueNone

    {
      Paths = [| mainPath |]
      StartPoint = startPos
      EndPoint = None
      Mode = mode
      Seed = seed
      Metadata = [||]
    }

  let addBranch
    (graph: PathGraph)
    (parentPathId: Guid<PathId>)
    (startPos: Vector3)
    (direction: Vector3)
    =
    let newId = %Guid.NewGuid()

    let newPath =
      createPath newId startPos direction false (ValueSome parentPathId)

    {
      graph with
          Paths = Array.append graph.Paths [| newPath |]
    }

  let findConvergencePaths (graph: PathGraph) (currentPath: PathState) =
    // Placeholder: find paths that are close to the current path's position
    // and have a similar direction
    graph.Paths
    |> Array.filter(fun p ->
      p.Id <> currentPath.Id
      && p.IsActive
      && Vector3.Distance(p.Position, currentPath.Position) < 20.0f)

  let findPathAtPosition (graph: PathGraph) (pos: Vector3) =
    graph.Paths
    |> Array.tryFind(fun p ->
      Vector3.Distance(p.Position, pos) < 10.0f && p.IsActive)

  let cleanupDistantPaths
    (graph: PathGraph)
    (playerPos: Vector3)
    (cutoffDistance: float32)
    =
    let updatedPaths =
      graph.Paths
      |> Array.map(fun p ->
        if
          p.IsActive
          && Vector3.Distance(p.Position, playerPos) > cutoffDistance
        then
          { p with IsActive = false }
        else
          p)

    { graph with Paths = updatedPaths }

  let updatePathState (graph: PathGraph) (updatedPath: PathState) =
    let newPaths =
      graph.Paths
      |> Array.map(fun p -> if p.Id = updatedPath.Id then updatedPath else p)

    { graph with Paths = newPaths }

  let getActivePaths(graph: PathGraph) =
    graph.Paths |> Array.filter(fun p -> p.IsActive)
