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

type FiniteModeConfig = {
  Duration: TimeSpan // Duration in seconds (e.g., 120-300)
  BranchComplexity: BranchComplexity
  Difficulty: float32 // 0.0-1.0
}

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
  Style: int
  AssetName: string
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


type IModelStore =
  abstract member Load: string -> unit
  abstract member Bake: unit -> unit
  abstract member Get: string -> Model voption
  abstract member GetMesh: string -> Mesh voption
  abstract member GetBounds: string -> BoundingBox voption
  abstract member GetGeometry: string -> ModelGeometry voption
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
