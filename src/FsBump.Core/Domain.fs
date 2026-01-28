namespace FsBump.Core

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Mibo.Input
open Mibo.Rendering.Graphics3D
open System
open FSharp.UMX


[<Struct>]
type CollisionType =
  | Solid
  | Passthrough
  | Climbable
  | Slope

[<Struct>]
type TileType =
  | Floor
  | Wall
  | Platform
  | Spikes
  | Collectible
  | StartPoint
  | Decoration
  | SlopeTile

// ─────────────────────────────────────────────────────────────
// Multi-Path & Game Mode Types
// ─────────────────────────────────────────────────────────────
[<Measure>]
type PathId

type BranchComplexity =
  | Low
  | Medium
  | High

type ColorVariant =
  | Blue
  | Green
  | Red
  | Yellow
  | Neutral

module ColorVariant =
  let asString =
    function
    | Blue -> "blue"
    | Green -> "green"
    | Red -> "red"
    | Yellow -> "yellow"
    | Neutral -> "neutral"

type AssetNamingPattern =
  | Standard
  | ButtonBase
  | LeverFloorBase
  | LeverWallBaseA
  | LeverWallBaseB


type AssetLocation =
  | Colored of ColorVariant * AssetNamingPattern * string
  | Neutral of string

module AssetNamingPattern =
  let inline createName name color = Colored(color, Standard, name)

  let inline createButton name color = Colored(color, ButtonBase, name)
  let inline createLeverFloor name color = Colored(color, LeverFloorBase, name)
  let inline createLeverWallA name color = Colored(color, LeverWallBaseA, name)
  let inline createLeverWallB name color = Colored(color, LeverWallBaseB, name)
  let inline createNeutral name = Neutral name


type AssetDefinition = {
  Name: string
  Location: AssetLocation
}

module AssetDefinition =
  let getLoadPath(asset: AssetDefinition) =

    match asset.Location with
    | Colored(color, pattern, name) ->
      let colorStr = ColorVariant.asString color

      match pattern with
      | Standard ->
        sprintf
          "kaykit_platformer/%s/%s_%s"
          (ColorVariant.asString color)
          name
          (ColorVariant.asString color)
      | ButtonBase ->
        sprintf
          "kaykit_platformer/%s/button_base_%s_button_%s"
          colorStr
          colorStr
          colorStr
      | LeverFloorBase ->
        sprintf
          "kaykit_platformer/%s/lever_floor_base_%s_lever_floor_%s"
          colorStr
          colorStr
          colorStr
      | LeverWallBaseA ->
        sprintf
          "kaykit_platformer/%s/lever_wall_base_A_%s_lever_wall_A_%s"
          colorStr
          colorStr
          colorStr
      | LeverWallBaseB ->
        sprintf
          "kaykit_platformer/%s/lever_wall_base_B_%s_lever_wall_B_%s"
          colorStr
          colorStr
          colorStr
    | Neutral name -> sprintf "kaykit_platformer/neutral/%s" name

type FiniteModeConfig = {
  Duration: TimeSpan // Duration in seconds (e.g., 120-300)
  BranchComplexity: BranchComplexity
  Difficulty: float32 // 0.0-1.0
}

type GenerationType =
  | PathBased
  | ZoneBased

type GameMode =
  | Infinite
  | Exploration of FiniteModeConfig
  | Challenge of FiniteModeConfig

type PathState = {
  Id: Guid<PathId>
  Position: Vector3
  Direction: Vector3
  PreviousDirection: Vector3
  Width: int
  CurrentColor: ColorVariant
  IsActive: bool
  IsMainPath: bool
  ParentPathId: Guid<PathId> voption
  ConvergencePathId: Guid<PathId> voption
  DistanceFromStart: float32
  NextSegmentIndex: int
}

[<Struct>]
type Tile = {
  Type: TileType
  Collision: CollisionType
  Position: Vector3
  Rotation: float32
  Variant: ColorVariant
  Size: Vector3
  AssetDefinition: AssetDefinition
  VisualOffset: Vector3
  PathId: Guid<PathId>
  SegmentIndex: int
}

type SegmentMetadata = {
  PathId: Guid<PathId>
  StartPosition: Vector3
  EndPosition: Vector3
  SegmentType: string
  Assets: string array
  IsBranchPoint: bool
  IsConvergencePoint: bool
}

type PathGraph = {
  Paths: PathState array
  StartPoint: Vector3
  EndPoint: Vector3 option
  Mode: GameMode
  Seed: int
  Metadata: SegmentMetadata array
}

[<Struct>]
type GenerationConfig = {
  MaxJumpHeight: float32
  MaxJumpDistance: float32
  SafetyBuffer: float32
}

[<Struct>]
type Body = {
  Position: Vector3
  Velocity: Vector3
  Radius: float32
}

[<Struct>]
type PlayerAction =
  | MoveForward
  | MoveBackward
  | MoveLeft
  | MoveRight
  | Jump

[<Struct>]
type PlayerModel = {
  Body: Body
  Input: ActionState<PlayerAction>
  AnalogDir: Vector2
  IsGrounded: bool
  Rotation: Quaternion
  LastSafePosition: Vector3
  RingRotation: float32
  Time: float32
}

type ModelGeometry = { Vertices: Vector3[]; Indices: int[] }

[<Struct>]
type AudioId =
  | AmbientMusic
  | JumpSound

type Specific =
  | PlayerBall
  | Cube

type IModelStore =
  abstract member Load: AssetDefinition -> unit
  abstract member LoadSpecific: Specific -> unit
  abstract member Bake: unit -> unit
  abstract member Get: AssetDefinition -> Model voption
  abstract member GetSpecific: Specific -> Model voption
  abstract member GetMesh: AssetDefinition -> Mesh voption
  abstract member GetSpecificMesh: Specific -> Mesh voption
  abstract member GetBounds: AssetDefinition -> BoundingBox voption
  abstract member GetSpecificBounds: Specific -> BoundingBox voption
  abstract member GetGeometry: AssetDefinition -> ModelGeometry voption
  abstract member GetSpecificGeometry: Specific -> ModelGeometry voption
  abstract member LoadTexture: string -> unit

  abstract member GetTexture: string -> Texture2D voption

type IModelStoreProvider =
  abstract ModelStore: IModelStore

type IRandomProvider =
  abstract Random: Random

type IAudioProvider =
  abstract Play: AudioId -> unit

module RootComposition =
  open System.Collections.Generic

  type AppEnv = {
    ModelStore: IModelStore
    Rng: Random
    Audio: IAudioProvider
    CollisionBuffer: ResizeArray<Tile>
    RenderBuffer: ResizeArray<RenderCommand>
  } with

    interface IModelStoreProvider with
      member this.ModelStore = this.ModelStore

    interface IRandomProvider with
      member this.Random = this.Rng

    interface IAudioProvider with
      member this.Play audioId = this.Audio.Play audioId
