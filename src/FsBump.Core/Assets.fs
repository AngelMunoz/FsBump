namespace FsBump.Core

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

module Assets =
  open Mibo.Elmish

  [<Literal>]
  let SaturnRings = "Textures/saturn_rings"

  let load(modelStore: IModelStore) =
    modelStore.LoadSpecific Specific.PlayerBall
    modelStore.LoadSpecific Specific.Cube
    modelStore.LoadTexture "gdb-switch-2"
    modelStore.LoadTexture "Textures/saturn_rings"

  let skyboxEffect(ctx: Mibo.Elmish.GameContext) =
    Assets.effect "Effects/NightSky" ctx
