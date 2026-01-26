namespace FsBump.Core

module TerrainAssets =

  type Collections = {
    Platforms: AssetDefinition[]
    Slopes: AssetDefinition[]
    Barriers: AssetDefinition[]
    Arches: AssetDefinition[]
    Pipes: AssetDefinition[]
    Interactive: AssetDefinition[]
    Collectibles: AssetDefinition[]
    Decorations: AssetDefinition[]
    Pillars: AssetDefinition[]
    NeutralSpecial: AssetDefinition[]
  }

  module Collections =
    let empty = {
      Platforms = Array.empty
      Slopes = Array.empty
      Barriers = Array.empty
      Arches = Array.empty
      Pipes = Array.empty
      Interactive = Array.empty
      Collectibles = Array.empty
      Decorations = Array.empty
      Pillars = Array.empty
      NeutralSpecial = Array.empty
    }

  [<Struct>]
  type TerrainAssets =
    | Exploration
    | Challenge
    | Infinite

  let platforms = [|
    { Name = "platform_1x1x1"; Location = Colored "platform_1x1x1" }
    { Name = "platform_2x2x1"; Location = Colored "platform_2x2x1" }
    { Name = "platform_2x2x2"; Location = Colored "platform_2x2x2" }
    { Name = "platform_2x2x4"; Location = Colored "platform_2x2x4" }
    { Name = "platform_4x2x1"; Location = Colored "platform_4x2x1" }
    { Name = "platform_4x2x2"; Location = Colored "platform_4x2x2" }
    { Name = "platform_4x2x4"; Location = Colored "platform_4x2x4" }
    { Name = "platform_4x4x1"; Location = Colored "platform_4x4x1" }
    { Name = "platform_4x4x2"; Location = Colored "platform_4x4x2" }
    { Name = "platform_4x4x4"; Location = Colored "platform_4x4x4" }
    { Name = "platform_6x2x1"; Location = Colored "platform_6x2x1" }
    { Name = "platform_6x2x2"; Location = Colored "platform_6x2x2" }
    { Name = "platform_6x2x4"; Location = Colored "platform_6x2x4" }
    { Name = "platform_6x6x1"; Location = Colored "platform_6x6x1" }
    { Name = "platform_6x6x2"; Location = Colored "platform_6x6x2" }
    { Name = "platform_6x6x4"; Location = Colored "platform_6x6x4" }
    { Name = "platform_decorative_1x1x1"; Location = Colored "platform_decorative_1x1x1" }
    { Name = "platform_decorative_2x2x2"; Location = Colored "platform_decorative_2x2x2" }
    { Name = "platform_arrow_2x2x1"; Location = Colored "platform_arrow_2x2x1" }
    { Name = "platform_arrow_4x4x1"; Location = Colored "platform_arrow_4x4x1" }
    { Name = "platform_decorative_1x1x1"; Location = Colored "platform_decorative_1x1x1" }
    { Name = "platform_decorative_2x2x2"; Location = Colored "platform_decorative_2x2x2" }
    { Name = "platform_hole_6x6x1"; Location = Colored "platform_hole_6x6x1" }
  |]

  let slopes = [|
    { Name = "platform_slope_2x2x2"; Location = Colored "platform_slope_2x2x2" }
    { Name = "platform_slope_2x4x4"; Location = Colored "platform_slope_2x4x4" }
    { Name = "platform_slope_2x6x4"; Location = Colored "platform_slope_2x6x4" }
    { Name = "platform_slope_4x2x2"; Location = Colored "platform_slope_4x2x2" }
    { Name = "platform_slope_4x4x4"; Location = Colored "platform_slope_4x4x4" }
    { Name = "platform_slope_4x6x4"; Location = Colored "platform_slope_4x6x4" }
    { Name = "platform_slope_6x2x2"; Location = Colored "platform_slope_6x2x2" }
    { Name = "platform_slope_6x4x4"; Location = Colored "platform_slope_6x4x4" }
    { Name = "platform_slope_6x6x4"; Location = Colored "platform_slope_6x6x4" }
  |]

  let barriers = [|
    { Name = "barrier_1x1x1"; Location = Colored "barrier_1x1x1" }
    { Name = "barrier_1x1x2"; Location = Colored "barrier_1x1x2" }
    { Name = "barrier_1x1x4"; Location = Colored "barrier_1x1x4" }
    { Name = "barrier_2x1x1"; Location = Colored "barrier_2x1x1" }
    { Name = "barrier_2x1x2"; Location = Colored "barrier_2x1x2" }
    { Name = "barrier_2x1x4"; Location = Colored "barrier_2x1x4" }
    { Name = "barrier_3x1x1"; Location = Colored "barrier_3x1x1" }
    { Name = "barrier_3x1x2"; Location = Colored "barrier_3x1x2" }
    { Name = "barrier_3x1x4"; Location = Colored "barrier_3x1x4" }
    { Name = "barrier_4x1x1"; Location = Colored "barrier_4x1x1" }
    { Name = "barrier_4x1x2"; Location = Colored "barrier_4x1x2" }
    { Name = "barrier_4x1x4"; Location = Colored "barrier_4x1x4" }
  |]

  let arches = [|
    { Name = "arch"; Location = Colored "arch" }
    { Name = "arch_tall"; Location = Colored "arch_tall" }
    { Name = "arch_wide"; Location = Colored "arch_wide" }
  |]

  let pipes = [|
    { Name = "pipe_straight_A"; Location = Colored "pipe_straight_A" }
    { Name = "pipe_straight_B"; Location = Colored "pipe_straight_B" }
    { Name = "pipe_end"; Location = Colored "pipe_end" }
    { Name = "pipe_90_A"; Location = Colored "pipe_90_A" }
    { Name = "pipe_90_B"; Location = Colored "pipe_90_B" }
    { Name = "pipe_180_A"; Location = Colored "pipe_180_A" }
    { Name = "pipe_180_B"; Location = Colored "pipe_180_B" }
  |]

  let interactive = [|
    { Name = "spring_pad"; Location = Colored "spring_pad" }
    { Name = "button_base"; Location = Colored "button_base" }
    { Name = "lever_floor_base"; Location = Colored "lever_floor_base" }
    { Name = "lever_wall_base_A"; Location = Colored "lever_wall_base_A" }
    { Name = "lever_wall_base_B"; Location = Colored "lever_wall_base_B" }
    { Name = "power"; Location = Colored "power" }
  |]

  let collectibles = [|
    { Name = "star"; Location = Colored "star" }
    { Name = "heart"; Location = Colored "heart" }
    { Name = "diamond"; Location = Colored "diamond" }
    { Name = "cone"; Location = Colored "cone" }
  |]

  let decorations = [|
    { Name = "flag_A"; Location = Colored "flag_A" }
    { Name = "flag_B"; Location = Colored "flag_B" }
    { Name = "flag_C"; Location = Colored "flag_C" }
    { Name = "hoop"; Location = Colored "hoop" }
    { Name = "hoop_angled"; Location = Colored "hoop_angled" }
    { Name = "railing_straight_single"; Location = Colored "railing_straight_single" }
    { Name = "railing_straight_double"; Location = Colored "railing_straight_double" }
    { Name = "railing_straight_padded"; Location = Colored "railing_straight_padded" }
    { Name = "railing_corner_single"; Location = Colored "railing_corner_single" }
    { Name = "railing_corner_double"; Location = Colored "railing_corner_double" }
    { Name = "railing_corner_padded"; Location = Colored "railing_corner_padded" }
    { Name = "bracing_small"; Location = Colored "bracing_small" }
    { Name = "bracing_medium"; Location = Colored "bracing_medium" }
    { Name = "bracing_large"; Location = Colored "bracing_large" }
    { Name = "signage_arrow_stand"; Location = Colored "signage_arrow_stand" }
    { Name = "signage_arrow_wall"; Location = Colored "signage_arrow_wall" }
    { Name = "signage_arrows_left"; Location = Colored "signage_arrows_left" }
    { Name = "signage_arrows_right"; Location = Colored "signage_arrows_right" }
  |]

  let pillars = [|
    { Name = "pillar_1x1x1"; Location = Neutral "pillar_1x1x1" }
    { Name = "pillar_1x1x2"; Location = Neutral "pillar_1x1x2" }
    { Name = "pillar_1x1x4"; Location = Neutral "pillar_1x1x4" }
    { Name = "pillar_1x1x8"; Location = Neutral "pillar_1x1x8" }
    { Name = "pillar_2x2x2"; Location = Neutral "pillar_2x2x2" }
    { Name = "pillar_2x2x4"; Location = Neutral "pillar_2x2x4" }
    { Name = "pillar_2x2x8"; Location = Neutral "pillar_2x2x8" }
  |]

  let neutralSpecial = [|
    { Name = "barrier_1x1x1"; Location = Neutral "barrier_1x1x1" }
    { Name = "barrier_1x1x2"; Location = Neutral "barrier_1x1x2" }
    { Name = "barrier_1x1x4"; Location = Neutral "barrier_1x1x4" }
    { Name = "barrier_2x1x1"; Location = Neutral "barrier_2x1x1" }
    { Name = "barrier_2x1x2"; Location = Neutral "barrier_2x1x2" }
    { Name = "barrier_2x1x4"; Location = Neutral "barrier_2x1x4" }
    { Name = "barrier_3x1x1"; Location = Neutral "barrier_3x1x1" }
    { Name = "barrier_3x1x2"; Location = Neutral "barrier_3x1x2" }
    { Name = "barrier_3x1x4"; Location = Neutral "barrier_3x1x4" }
    { Name = "barrier_4x1x1"; Location = Neutral "barrier_4x1x1" }
    { Name = "barrier_4x1x2"; Location = Neutral "barrier_4x1x2" }
    { Name = "barrier_4x1x4"; Location = Neutral "barrier_4x1x4" }
    { Name = "ball"; Location = Neutral "ball" }
    { Name = "bomb"; Location = Neutral "bomb" }
    { Name = "cone"; Location = Neutral "cone" }
    { Name = "floor_wood_1x1"; Location = Neutral "floor_wood_1x1" }
    { Name = "floor_wood_2x2"; Location = Neutral "floor_wood_2x2" }
    { Name = "floor_wood_2x6"; Location = Neutral "floor_wood_2x6" }
    { Name = "floor_wood_4x4"; Location = Neutral "floor_wood_4x4" }
    { Name = "platform_wood_1x1x1"; Location = Neutral "platform_wood_1x1x1" }
    { Name = "sign"; Location = Neutral "sign" }
    { Name = "signage_arrows_left"; Location = Neutral "signage_arrows_left" }
    { Name = "signage_arrows_right"; Location = Neutral "signage_arrows_right" }
    { Name = "signage_finish"; Location = Neutral "signage_finish" }
    { Name = "signage_finish_board"; Location = Neutral "signage_finish_board" }
    { Name = "signage_finish_wide"; Location = Neutral "signage_finish_wide" }
    { Name = "signage_finish_wide_board"; Location = Neutral "signage_finish_wide_board" }
    { Name = "spring"; Location = Neutral "spring" }
    { Name = "structure_A"; Location = Neutral "structure_A" }
    { Name = "structure_B"; Location = Neutral "structure_B" }
    { Name = "structure_C"; Location = Neutral "structure_C" }
    { Name = "strut_horizontal"; Location = Neutral "strut_horizontal" }
    { Name = "strut_vertical"; Location = Neutral "strut_vertical" }
  |]

  let getAssetsByMode(modeName: TerrainAssets) =
    match modeName with
    | Exploration ->
        {
          Collections.empty with
              Platforms = platforms
              Slopes = slopes
              Arches = arches
              Collectibles = collectibles
              Decorations = decorations
              NeutralSpecial = neutralSpecial
        }

    | Challenge ->
        {
          Collections.empty with
              Barriers = barriers
              Pipes = pipes
              Interactive = interactive
              Pillars = pillars
              NeutralSpecial = neutralSpecial
        }

    | Infinite ->
        {
          Collections.empty with
              Platforms = platforms
              Slopes = slopes
              Barriers = barriers
              Arches = arches
              Interactive = interactive
              Collectibles = collectibles
              Decorations = decorations
              Pillars = pillars
              NeutralSpecial = neutralSpecial
        }

  let getAllAssets() =
    Array.concat [
      platforms
      slopes
      barriers
      arches
      pipes
      interactive
      collectibles
      decorations
      pillars
      neutralSpecial
    ]

  let getNextColor(current: ColorVariant) =
    match current with
    | ColorVariant.Blue -> ColorVariant.Green
    | ColorVariant.Green -> ColorVariant.Red
    | ColorVariant.Red -> ColorVariant.Yellow
    | ColorVariant.Yellow -> ColorVariant.Blue
    | ColorVariant.Neutral -> ColorVariant.Blue
