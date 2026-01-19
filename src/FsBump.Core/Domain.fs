namespace FsBump.Core

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Mibo.Input
open Mibo.Rendering.Graphics3D
open System

type CollisionType =
  | Solid
  | Passthrough
  | Climbable
  | Slope

type TileType =
  | Floor
  | Wall
  | Platform
  | Spikes
  | Collectible
  | StartPoint
  | Decoration
  | SlopeTile

type Tile = {
  Type: TileType
  Collision: CollisionType
  Position: Vector3
  Rotation: float32
  Variant: int
  Size: Vector3
  Style: int
  AssetName: string
  VisualOffset: Vector3
}

type PathState = {
  Position: Vector3
  Direction: Vector3
  PreviousDirection: Vector3
  Width: int
  CurrentColor: int
}

type GenerationConfig = {
  MaxJumpHeight: float32
  MaxJumpDistance: float32
  SafetyBuffer: float32
}

type Body = {
  Position: Vector3
  Velocity: Vector3
  Radius: float32
}

type PlayerAction =
  | MoveForward
  | MoveBackward
  | MoveLeft
  | MoveRight
  | Jump

type PlayerModel = {
  Body: Body
  Input: ActionState<PlayerAction>
  IsGrounded: bool
  Rotation: Quaternion
  LastSafePosition: Vector3
}

type ModelGeometry = { Vertices: Vector3[]; Indices: int[] }

[<Struct>]
type AudioId =
  | AmbientMusic
  | JumpSound


type IModelStore =
  abstract member Load: string -> unit
  abstract member Get: string -> Model option
  abstract member GetMesh: string -> Mesh option
  abstract member GetBounds: string -> BoundingBox option
  abstract member GetGeometry: string -> ModelGeometry option
  abstract member LoadTexture: string -> unit
  abstract member GetTexture: string -> Texture2D option

type IModelStoreProvider =
  abstract ModelStore: IModelStore

type IRandomProvider =
  abstract Random: Random

type IAudioProvider =
  abstract Play: AudioId -> unit

module RootComposition =

  [<Struct>]
  type AppEnv = {
    ModelStore: IModelStore
    Rng: Random
    Audio: IAudioProvider
  } with

    interface IModelStoreProvider with
      member this.ModelStore = this.ModelStore

    interface IRandomProvider with
      member this.Random = this.Rng

    interface IAudioProvider with
      member this.Play audioId = this.Audio.Play audioId
