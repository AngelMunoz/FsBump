namespace FsBump.Core

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

module Assets =
  open Mibo.Elmish

  [<Literal>]
  let PlayerBall = "kaykit_platformer/blue/ball_blue"

  [<Literal>]
  let SaturnRings = "Textures/saturn_rings"

  let load(modelStore: IModelStore) =
    let allAssetDefs = TerrainAssets.getAllAssets()

    for assetDef in allAssetDefs do
      match assetDef.Location with
      | Colored _ ->
        for color in [ColorVariant.Blue; ColorVariant.Green; ColorVariant.Red; ColorVariant.Yellow] do
          let path = AssetDefinition.getLoadPath assetDef color
          modelStore.Load path
      | Neutral _ ->
        let path = AssetDefinition.getLoadPath assetDef ColorVariant.Neutral
        modelStore.Load path

    modelStore.Load PlayerBall
    modelStore.Load "cube"
    modelStore.LoadTexture "gdb-switch-2"
    modelStore.LoadTexture "Textures/saturn_rings"

  let skyboxEffect(ctx: Mibo.Elmish.GameContext) =
    Assets.effect "Effects/NightSky" ctx

  let getAsset(tile: Tile) =
    if not(String.IsNullOrEmpty tile.AssetName) then
      let colorStr = ColorVariant.asString tile.Variant
      $"kaykit_platformer/%s{colorStr}/%s{tile.AssetName}_%s{colorStr}"
    else
      let color = ColorVariant.asString tile.Variant
      match tile.Type with
      | TileType.Floor ->
        $"kaykit_platformer/%s{color}/platform_1x1x1_%s{color}"
      | _ -> ""
