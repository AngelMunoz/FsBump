namespace FsBump.Core

open System
open Microsoft.Xna.Framework

// ─────────────────────────────────────────────────────────────
// Tile Construction
// ─────────────────────────────────────────────────────────────

module TileBuilder =

    let create type' collision pos rot variant size style asset offset pathId segIndex = {
        Type = type'
        Collision = collision
        Position = pos
        Rotation = rot
        Variant = variant
        Size = size
        Style = style
        AssetName = asset
        VisualOffset = offset
        PathId = pathId
        SegmentIndex = segIndex
    }

    let getAssetData asset color (env: #IModelStoreProvider) =
        let colorStr =
            match color % 4 with
            | 0 -> "blue"
            | 1 -> "green"
            | 2 -> "red"
            | _ -> "yellow"

        let full = sprintf "kaykit_platformer/%s/%s_%s" colorStr asset colorStr

        match env.ModelStore.GetBounds full with
        | ValueSome b -> b.Max - b.Min, VectorMath.mul (VectorMath.add b.Min b.Max) -0.5f, asset
        | ValueNone -> 
            // Fallback for neutral assets (no color suffix)
            match env.ModelStore.GetBounds (sprintf "kaykit_platformer/neutral/%s" asset) with
            | ValueSome b -> b.Max - b.Min, VectorMath.mul (VectorMath.add b.Min b.Max) -0.5f, asset
            | ValueNone -> Vector3.One, VectorMath.create 0.0f -0.5f 0.0f, asset

    let floor pos color (env: #IModelStoreProvider) pathId segIndex =
        let size, offset, asset = getAssetData "platform_1x1x1" color env
        create TileType.Floor CollisionType.Solid pos 0.0f color size 0 asset offset pathId segIndex

    let wall pos color size asset offset pathId segIndex =
        create TileType.Wall CollisionType.Solid pos 0.0f color size 0 asset offset pathId segIndex

    let decoration pos rot color size asset offset type' collision pathId segIndex =
        create type' collision pos rot color size 0 asset offset pathId segIndex

    let platform pos color size assetName offset pathId segIndex =
        create TileType.Platform CollisionType.Solid pos 0.0f color size 0 assetName offset pathId segIndex

    let collectible pos color size asset offset pathId segIndex =
        create TileType.Collectible CollisionType.Passthrough pos 0.0f color size 0 asset offset pathId segIndex

    let slope pos rot color size asset offset pathId segIndex =
        create TileType.SlopeTile CollisionType.Slope pos rot color size 0 asset offset pathId segIndex

// ─────────────────────────────────────────────────────────────
// Building Blocks
// ─────────────────────────────────────────────────────────────

module BuildingBlocks =

    let addEdgeDetails
        (env: #IRandomProvider & #IModelStoreProvider)
        (pos: Vector3)
        (color: int)
        (isLeft: bool)
        (tiles: ResizeArray<Tile>)
        (mode: string)
        (pathId: PathId)
        (segIndex: int)
        =
        let roll = env.Random.NextDouble()
        
        let barrierProb = if mode = "Challenge" then 0.3 else 0.15
        let decoProb = if mode = "Exploration" then 0.5 else 0.3

        if roll < barrierProb then
            let assets = TerrainAssets.getAssetsByMode mode |> Array.filter (fun a -> a.StartsWith("barrier"))
            let asset = if assets.Length > 0 then assets.[env.Random.Next(assets.Length)] else "barrier_1x1x1"

            let size, offset, assetName = TileBuilder.getAssetData asset color env
            
            let shiftY = size.Y * 0.5f + 0.5f
            let shift = VectorMath.create 0.0f shiftY 0.0f
            let finalPos = VectorMath.add pos shift

            tiles.Add(
                TileBuilder.wall
                    finalPos
                    color
                    size
                    assetName
                    offset
                    pathId
                    segIndex
            )
        elif roll < decoProb then
            let assets = TerrainAssets.getAssetsByMode mode |> Array.filter (fun a -> 
                a.StartsWith("flag") || a.StartsWith("signage") || a.StartsWith("railing"))
            let asset = if assets.Length > 0 then assets.[env.Random.Next(assets.Length)] else "flag_A"

            let size, offset, assetName = TileBuilder.getAssetData asset color env
            let rot = if isLeft then -MathHelper.PiOver2 else MathHelper.PiOver2

            let shiftY = size.Y * 0.5f + 0.5f
            let shift = VectorMath.create 0.0f shiftY 0.0f
            let finalPos = VectorMath.add pos shift

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
        (mode: string)
        =
        let nextState = state |> StateOps.advance 1.0f
        let right = Vector3.Cross(Vector3.Up, state.Direction)
        
        for w in -state.Width .. state.Width do
            let fw = float32 w
            let offset = VectorMath.mul right fw
            let tilePos = VectorMath.add nextState.Position offset
            
            tiles.Add(TileBuilder.floor tilePos state.CurrentColor env state.Id state.NextSegmentIndex)

            if abs w = state.Width then
                addEdgeDetails env tilePos state.CurrentColor (w < 0) tiles mode state.Id state.NextSegmentIndex

        nextState

    let addSlope
        (env: #IRandomProvider & #IModelStoreProvider)
        (tiles: ResizeArray<Tile>)
        (prevHalf: float32)
        (state: PathState)
        (mode: string)
        =
        let goUp = env.Random.NextDouble() > 0.5
        
        let slopeAssets = TerrainAssets.getAssetsByMode mode |> Array.filter (fun a -> a.Contains("slope"))
        let assetName = if slopeAssets.Length > 0 then slopeAssets.[env.Random.Next(slopeAssets.Length)] else "platform_slope_4x2x2"

        let size, offset, assetBase = TileBuilder.getAssetData assetName state.CurrentColor env

        let currentHalf = size.Z * 0.5f
        
        let gapChance = if mode = "Challenge" then 0.4 else 0.2
        let gap = if env.Random.NextDouble() < gapChance then 2.0f else -0.25f
        
        let dist = prevHalf + currentHalf + gap

        let yOffset = if goUp then 0.0f else -size.Y
        let nextYOffset = if goUp then size.Y else -size.Y

        let rot =
            if state.Direction = -Vector3.UnitZ then 0.0f
            elif state.Direction = Vector3.UnitZ then MathHelper.Pi
            elif state.Direction = -Vector3.UnitX then MathHelper.PiOver2
            else -MathHelper.PiOver2

        let finalRot = if goUp then rot else rot + MathHelper.Pi

        let moveVec = VectorMath.mul state.Direction dist
        let vertVec = VectorMath.create 0.0f (size.Y * 0.5f + 0.5f + yOffset) 0.0f
        let slopeCenter = VectorMath.add (VectorMath.add state.Position moveVec) vertVec

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
        (mode: string)
        =
        let gapSize =
            if mode = "Challenge" then
                env.Random.Next(3, 6) // Harder jumps
            else
                if env.Random.NextDouble() < 0.8 then env.Random.Next(1, 3) else env.Random.Next(3, 5)

        let platformAssets = TerrainAssets.getAssetsByMode mode |> Array.filter (fun a -> a.StartsWith("platform") && not (a.Contains("slope")))
        let asset = if platformAssets.Length > 0 then platformAssets.[env.Random.Next(platformAssets.Length)] else "platform_4x4x1"

        let size, offset, assetBase = TileBuilder.getAssetData asset state.CurrentColor env

        let heightChange = float32(env.Random.Next(-1, 2)) * 2.0f
        let dist = 0.5f + float32 gapSize + size.Z * 0.5f

        // Adjust Y to align the top surface of the platform with the path height
        let verticalAdjust = 0.5f - (size.Y * 0.5f)
        let totalHeightChange = heightChange + verticalAdjust

        let moveVec = VectorMath.mul state.Direction dist
        let vertVec = VectorMath.create 0.0f totalHeightChange 0.0f
        let nextPos = VectorMath.add (VectorMath.add state.Position moveVec) vertVec

        tiles.Add(
            TileBuilder.platform nextPos state.CurrentColor size assetBase offset state.Id state.NextSegmentIndex
        )

        let logicalMoveVec = VectorMath.mul state.Direction dist
        let logicalVertVec = VectorMath.create 0.0f heightChange 0.0f
        let logicalNextPos = VectorMath.add (VectorMath.add state.Position logicalMoveVec) logicalVertVec

        { state with Position = logicalNextPos } |> StateOps.advance(size.Z * 0.5f - 0.5f)

// ─────────────────────────────────────────────────────────────
// Segment Strategies
// ─────────────────────────────────────────────────────────────

module SegmentStrategies =

    let flatRun
        (env: #IRandomProvider & #IModelStoreProvider)
        (length: int)
        (state: PathState)
        (mode: string)
        =
        let tiles = ResizeArray<Tile>()
        let mutable s = state

        if s.Direction <> s.PreviousDirection then
            for _ in 1 .. s.Width * 2 do
                s <- BuildingBlocks.addRow env tiles s mode

        for _ in 1..length do
            s <- BuildingBlocks.addRow env tiles s mode

        tiles.ToArray(), { s with NextSegmentIndex = s.NextSegmentIndex + 1 }

    let slope
        (env: #IRandomProvider & #IModelStoreProvider)
        (length: int)
        (state: PathState)
        (mode: string)
        =
        let tiles = ResizeArray<Tile>()
        let mutable s = state
        let mutable ph = 0.5f

        for _ in 1 .. Math.Max(1, length / 2) do
            let nextS, nextPh = BuildingBlocks.addSlope env tiles ph s mode
            s <- nextS
            ph <- nextPh

        tiles.ToArray(), { s with NextSegmentIndex = s.NextSegmentIndex + 1 }

    let platform
        (env: #IRandomProvider & #IModelStoreProvider)
        (count: int)
        (state: PathState)
        (mode: string)
        =
        let tiles = ResizeArray<Tile>()
        let mutable s = state

        for _ in 1..count do
            s <- BuildingBlocks.addPlatform env tiles s mode

        tiles.ToArray(), { s with NextSegmentIndex = s.NextSegmentIndex + 1 }
