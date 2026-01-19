namespace FsBump.Core

open System
open Mibo.Elmish

type IModelStoreProvider =
  abstract ModelStore: IModelStore

type IRandomProvider =
  abstract Random: Random

[<Struct>]
type AppEnv = {
  ModelStore: IModelStore
  Rng: Random
}
with
  interface IModelStoreProvider with
    member this.ModelStore = this.ModelStore
  interface IRandomProvider with
    member this.Random = this.Rng
