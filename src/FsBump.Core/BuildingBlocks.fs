namespace FsBump.Core

open System
open Microsoft.Xna.Framework
open FSharp.UMX
// ─────────────────────────────────────────────────────────────
// Tile Construction
// ─────────────────────────────────────────────────────────────

module TileBuilder =

  let getAssetData (asset: AssetDefinition) (env: #IModelStoreProvider) =
    env.ModelStore.Load asset

    match env.ModelStore.GetBounds asset with
    | ValueSome b -> b.Max - b.Min, (b.Min + b.Max) * -0.5f, asset
    | ValueNone -> Vector3.One, Vector3(0.0f, -0.5f, 0.0f), asset

  let floor pos color (env: #IModelStoreProvider) pathId segIndex =
    let asset = {
      Name = "platform_1x1x1"
      Location = AssetNamingPattern.createName "platform_1x1x1" color
    }

    let size, offset, asset = getAssetData asset env

    {
      Type = TileType.Floor
      Collision = CollisionType.Solid
      Position = pos
      Rotation = 0.0f
      Variant = color
      Size = size
      AssetDefinition = asset
      VisualOffset = offset
      PathId = pathId
      SegmentIndex = segIndex
    }

  let wall pos color size asset offset pathId segIndex = {
    Type = TileType.Wall
    Collision = CollisionType.Solid
    Position = pos
    Rotation = 0.0f
    Variant = color
    Size = size
    AssetDefinition = asset
    VisualOffset = offset
    PathId = pathId
    SegmentIndex = segIndex
  }

  let decoration
    pos
    rot
    color
    size
    asset
    offset
    type'
    collision
    pathId
    segIndex
    =
    {
      Type = type'
      Collision = collision
      Position = pos
      Rotation = rot
      Variant = color
      Size = size
      AssetDefinition = asset
      VisualOffset = offset
      PathId = pathId
      SegmentIndex = segIndex
    }

  let platform pos color size assetName offset pathId segIndex = {
    Type = TileType.Platform
    Collision = CollisionType.Solid
    Position = pos
    Rotation = 0.0f
    Variant = color
    Size = size
    AssetDefinition = assetName
    VisualOffset = offset
    PathId = pathId
    SegmentIndex = segIndex
  }

  let collectible pos color size asset offset pathId segIndex = {
    Type = TileType.Collectible
    Collision = CollisionType.Passthrough
    Position = pos
    Rotation = 0.0f
    Variant = color
    Size = size
    AssetDefinition = asset
    VisualOffset = offset
    PathId = pathId
    SegmentIndex = segIndex
  }

  let slope pos rot color size asset offset pathId segIndex = {
    Type = TileType.SlopeTile
    Collision = CollisionType.Slope
    Position = pos
    Rotation = rot
    Variant = color
    Size = size
    AssetDefinition = asset
    VisualOffset = offset
    PathId = pathId
    SegmentIndex = segIndex
  }

// ─────────────────────────────────────────────────────────────
// Building Blocks
// ─────────────────────────────────────────────────────────────

module BuildingBlocks =

  let addEdgeDetails
    (env: #IRandomProvider & #IModelStoreProvider)
    (pos: Vector3)
    (color: ColorVariant)
    (isLeft: bool)
    (tiles: ResizeArray<Tile>)
    (mode: TerrainAssets.TerrainAssets)
    (pathId: Guid<PathId>)
    (segIndex: int)
    =
    let roll = env.Random.NextDouble()

    let struct (barrierProb, decoProb) =
      match mode with
      | TerrainAssets.Challenge -> 0.3, 0.15
      | TerrainAssets.Infinite
      | TerrainAssets.Exploration -> 0.5, 0.3


    let assets = TerrainAssets.getAssetsByMode mode

    if roll < barrierProb then
      let assets = assets.Barriers

      let asset =
        if assets.Length > 0 then
          let struct (kind, name) = assets[env.Random.Next assets.Length]

          {
            Name = name
            Location = Colored(color, kind, name)
          }
        else

        {
          Name = "barrier_1x1x1"
          Location = Colored(color, Standard, "barrier_1x1x1")
        }

      let size, offset, assetName = TileBuilder.getAssetData asset env

      let shiftY = size.Y * 0.5f + 0.5f
      let shift = Vector3(0.0f, shiftY, 0.0f)
      let finalPos = pos + shift

      tiles.Add(
        TileBuilder.wall finalPos color size assetName offset pathId segIndex
      )
    elif roll < decoProb then
      let assets = assets.Decorations

      let asset =
        if assets.Length > 0 then
          let struct (kind, name) = assets[env.Random.Next assets.Length]

          {
            Name = name
            Location = Colored(color, kind, name)
          }
        else
          {
            Name = "flag_A"
            Location = Colored(color, Standard, "flag_A")
          }

      let size, offset, assetName = TileBuilder.getAssetData asset env
      let rot = if isLeft then -MathHelper.PiOver2 else MathHelper.PiOver2

      let shiftY = size.Y * 0.5f + 0.5f
      let shift = Vector3(0.0f, shiftY, 0.0f)
      let finalPos = pos + shift

      tiles.Add(
        TileBuilder.decoration
          finalPos
          rot
          color
          size
          assetName
          offset
          TileType.Decoration
          CollisionType.Passthrough
          pathId
          segIndex
      )

  let addRow
    (env: #IRandomProvider & #IModelStoreProvider)
    (tiles: ResizeArray<Tile>)
    (state: PathState)
    (mode: TerrainAssets.TerrainAssets)
    =
    let nextState = state |> StateOps.advance 1.0f
    let right = Vector3.Cross(Vector3.Up, state.Direction)

    for w in -state.Width .. state.Width do
      let fw = float32 w
      let offset = right * fw
      let tilePos = nextState.Position + offset

      tiles.Add(
        TileBuilder.floor
          tilePos
          state.CurrentColor
          env
          state.Id
          state.NextSegmentIndex
      )

      if abs w = state.Width then
        addEdgeDetails
          env
          tilePos
          state.CurrentColor
          (w < 0)
          tiles
          mode
          state.Id
          state.NextSegmentIndex

    nextState

  let addSlope
    (env: #IRandomProvider & #IModelStoreProvider)
    (tiles: ResizeArray<Tile>)
    (prevHalf: float32)
    (state: PathState)
    (mode: TerrainAssets.TerrainAssets)
    =
    let goUp = env.Random.NextDouble() > 0.5
    let assets = TerrainAssets.getAssetsByMode mode

    let slopeAssets = assets.Slopes

    let assetName =
      if slopeAssets.Length > 0 then
        let struct (kind, name) =
          slopeAssets[env.Random.Next slopeAssets.Length]

        {
          Name = name
          Location = Colored(state.CurrentColor, kind, name)
        }
      else

        {
          Name = "platform_slope_4x2x2"
          Location =
            Colored(state.CurrentColor, Standard, "platform_slope_4x2x2")
        }

    let size, offset, assetBase = TileBuilder.getAssetData assetName env

    let currentHalf = size.Z * 0.5f

    let gapChance = if mode.IsChallenge then 0.4 else 0.2
    let gap = if env.Random.NextDouble() < gapChance then 2.0f else -0.25f

    let dist = prevHalf + currentHalf + gap

    let yOffset = if goUp then 0.0f else -size.Y
    let nextYOffset = if goUp then size.Y else -size.Y

    let rot =
      if state.Direction = -Vector3.UnitZ then
        0.0f
      elif state.Direction = Vector3.UnitZ then
        MathHelper.Pi
      elif state.Direction = -Vector3.UnitX then
        MathHelper.PiOver2
      else
        -MathHelper.PiOver2

    let finalRot = if goUp then rot else rot + MathHelper.Pi

    let moveVec = state.Direction * dist
    let vertVec = Vector3(0.0f, size.Y * 0.5f + 0.5f + yOffset, 0.0f)

    let slopeCenter = state.Position + moveVec + vertVec

    tiles.Add(
      TileBuilder.slope
        slopeCenter
        finalRot
        state.CurrentColor
        size
        assetBase
        offset
        state.Id
        state.NextSegmentIndex
    )

    let intermediateState = state |> StateOps.advance dist
    let finalState = intermediateState |> StateOps.moveY nextYOffset

    finalState, currentHalf

  let addPlatform
    (env: #IRandomProvider & #IModelStoreProvider)
    (tiles: ResizeArray<Tile>)
    (state: PathState)
    (mode: TerrainAssets.TerrainAssets)
    =
    let gapSize =
      match mode with
      | TerrainAssets.Challenge -> env.Random.Next(3, 6)
      | TerrainAssets.Infinite
      | TerrainAssets.Exploration -> env.Random.Next(1, 5)


    let assets = TerrainAssets.getAssetsByMode mode
    let platformAssets = assets.Platforms


    let asset =
      if platformAssets.Length > 0 then
        let struct (kind, name) =
          platformAssets[env.Random.Next platformAssets.Length]

        {
          Name = name
          Location = Colored(state.CurrentColor, kind, name)
        }
      else

        {
          Name = "platform_4x4x1"
          Location = Colored(state.CurrentColor, Standard, "platform_4x4x1")
        }

    let size, offset, assetBase = TileBuilder.getAssetData asset env

    let heightChange = float32(env.Random.Next(-1, 2)) * 2.0f
    let dist = 0.5f + float32 gapSize + size.Z * 0.5f

    // Adjust Y to align the top surface of the platform with the path height
    let verticalAdjust = 0.5f - (size.Y * 0.5f)
    let totalHeightChange = heightChange + verticalAdjust

    let moveVec = state.Direction * dist
    let vertVec = Vector3(0.0f, totalHeightChange, 0.0f)
    let nextPos = state.Position + moveVec + vertVec

    tiles.Add(
      TileBuilder.platform
        nextPos
        state.CurrentColor
        size
        assetBase
        offset
        state.Id
        state.NextSegmentIndex
    )

    let logicalMoveVec = state.Direction * dist
    let logicalVertVec = Vector3(0.0f, heightChange, 0.0f)

    let logicalNextPos = state.Position + logicalMoveVec + logicalVertVec

    { state with Position = logicalNextPos }
    |> StateOps.advance(size.Z * 0.5f - 0.5f)

// ─────────────────────────────────────────────────────────────
// Segment Strategies
// ─────────────────────────────────────────────────────────────

module SegmentStrategies =

  let flatRun
    (env: #IRandomProvider & #IModelStoreProvider)
    (length: int)
    (state: PathState)
    (mode: TerrainAssets.TerrainAssets)
    =
    let tiles = ResizeArray<Tile>()
    let mutable s = state

    if s.Direction <> s.PreviousDirection then
      for _ in 1 .. s.Width * 2 do
        s <- BuildingBlocks.addRow env tiles s mode

    for _ in 1..length do
      s <- BuildingBlocks.addRow env tiles s mode

    tiles.ToArray(),
    {
      s with
          NextSegmentIndex = s.NextSegmentIndex + 1
    }

  let slope
    (env: #IRandomProvider & #IModelStoreProvider)
    (length: int)
    (state: PathState)
    (mode: TerrainAssets.TerrainAssets)
    =
    let tiles = ResizeArray<Tile>()
    let mutable s = state
    let mutable ph = 0.5f

    for _ in 1 .. Math.Max(1, length / 2) do
      let nextS, nextPh = BuildingBlocks.addSlope env tiles ph s mode
      s <- nextS
      ph <- nextPh

    tiles.ToArray(),
    {
      s with
          NextSegmentIndex = s.NextSegmentIndex + 1
    }

  let platform
    (env: #IRandomProvider & #IModelStoreProvider)
    (count: int)
    (state: PathState)
    (mode: TerrainAssets.TerrainAssets)
    =
    let tiles = ResizeArray<Tile>()
    let mutable s = state

    for _ in 1..count do
      s <- BuildingBlocks.addPlatform env tiles s mode

    tiles.ToArray(),
    {
      s with
          NextSegmentIndex = s.NextSegmentIndex + 1
    }
