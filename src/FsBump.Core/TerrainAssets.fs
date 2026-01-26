namespace FsBump.Core

module TerrainAssets =

  type Collections = {
    Platforms: struct (AssetNamingPattern * string) array
    Slopes: struct (AssetNamingPattern * string) array
    Barriers: struct (AssetNamingPattern * string) array
    Arches: struct (AssetNamingPattern * string) array
    Pipes: struct (AssetNamingPattern * string) array
    Interactive: struct (AssetNamingPattern * string) array
    Collectibles: struct (AssetNamingPattern * string) array
    Decorations: struct (AssetNamingPattern * string) array
    Pillars: AssetLocation array
    NeutralSpecial: AssetLocation array
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

  let platforms: struct (AssetNamingPattern * string) array = [|
    Standard, "platform_1x1x1"
    Standard, "platform_2x2x1"
    Standard, "platform_2x2x2"
    Standard, "platform_2x2x4"
    Standard, "platform_4x2x1"
    Standard, "platform_4x2x2"
    Standard, "platform_4x2x4"
    Standard, "platform_4x4x1"
    Standard, "platform_4x4x2"
    Standard, "platform_4x4x4"
    Standard, "platform_6x2x1"
    Standard, "platform_6x2x2"
    Standard, "platform_6x2x4"
    Standard, "platform_6x6x1"
    Standard, "platform_6x6x2"
    Standard, "platform_6x6x4"
    Standard, "platform_decorative_1x1x1"
    Standard, "platform_decorative_2x2x2"
    Standard, "platform_arrow_2x2x1"
    Standard, "platform_arrow_4x4x1"
    Standard, "platform_hole_6x6x1"
  |]

  let slopes: struct (AssetNamingPattern * string) array = [|
    Standard, "platform_slope_2x2x2"
    Standard, "platform_slope_2x4x4"
    Standard, "platform_slope_2x6x4"
    Standard, "platform_slope_4x2x2"
    Standard, "platform_slope_4x4x4"
    Standard, "platform_slope_4x6x4"
    Standard, "platform_slope_6x2x2"
    Standard, "platform_slope_6x4x4"
    Standard, "platform_slope_6x6x4"
  |]


  let barriers: struct (AssetNamingPattern * string) array = [|
    Standard, "barrier_1x1x1"
    Standard, "barrier_1x1x2"
    Standard, "barrier_1x1x4"
    Standard, "barrier_2x1x1"
    Standard, "barrier_2x1x2"
    Standard, "barrier_2x1x4"
    Standard, "barrier_3x1x1"
    Standard, "barrier_3x1x2"
    Standard, "barrier_3x1x4"
    Standard, "barrier_4x1x1"
    Standard, "barrier_4x1x2"
    Standard, "barrier_4x1x4"
  |]

  let arches: struct (AssetNamingPattern * string) array = [|
    Standard, "arch"
    Standard, "arch_tall"
    Standard, "arch_wide"
  |]

  let pipes: struct (AssetNamingPattern * string) array = [|
    Standard, "pipe_straight_A"
    Standard, "pipe_straight_B"
    Standard, "pipe_end"
    Standard, "pipe_90_A"
    Standard, "pipe_90_B"
    Standard, "pipe_180_A"
    Standard, "pipe_180_B"
  |]

  let interactive: struct (AssetNamingPattern * string) array = [|
    Standard, "spring_pad"
    ButtonBase, "button_base"
    LeverFloorBase, "lever_floor_base"
    LeverWallBaseA, "lever_wall_base_A"
    LeverWallBaseB, "lever_wall_base_B"
    Standard, "power"
  |]

  let collectibles: struct (AssetNamingPattern * string) array = [|
    Standard, "star"
    Standard, "heart"
    Standard, "diamond"
    Standard, "cone"
    Standard, "ball"
  |]

  let decorations: struct (AssetNamingPattern * string) array = [|
    Standard, "flag_A"
    Standard, "flag_B"
    Standard, "flag_C"
    Standard, "hoop"
    Standard, "hoop_angled"
    Standard, "railing_straight_single"
    Standard, "railing_straight_double"
    Standard, "railing_straight_padded"
    Standard, "railing_corner_single"
    Standard, "railing_corner_double"
    Standard, "railing_corner_padded"
    Standard, "bracing_small"
    Standard, "bracing_medium"
    Standard, "bracing_large"
    Standard, "bomb_A"
    Standard, "bomb_B"
    Standard, "signage_arrow_stand"
    Standard, "signage_arrow_wall"
    Standard, "signage_arrows_left"
    Standard, "signage_arrows_right"
  |]

  let neutralSpecial: AssetLocation array = [|
    Neutral "barrier_1x1x1"
    Neutral "barrier_1x1x2"
    Neutral "barrier_1x1x4"
    Neutral "barrier_2x1x1"
    Neutral "barrier_2x1x2"
    Neutral "barrier_2x1x4"
    Neutral "barrier_3x1x1"
    Neutral "barrier_3x1x2"
    Neutral "barrier_3x1x4"
    Neutral "barrier_4x1x1"
    Neutral "barrier_4x1x2"
    Neutral "barrier_4x1x4"
    Neutral "ball"
    Neutral "bomb"
    Neutral "cone"
    Neutral "floor_wood_1x1"
    Neutral "floor_wood_2x2"
    Neutral "floor_wood_2x6"
    Neutral "floor_wood_4x4"
    Neutral "platform_wood_1x1x1"
    Neutral "sign"
    Neutral "signage_arrows_left"
    Neutral "signage_arrows_right"
    Neutral "signage_finish"
    Neutral "signage_finish_board"
    Neutral "signage_finish_wide"
    Neutral "signage_finish_wide_board"
    Neutral "spring"
    Neutral "structure_A"
    Neutral "structure_B"
    Neutral "structure_C"
    Neutral "strut_horizontal"
    Neutral "strut_vertical"
  |]

  let pillars: AssetLocation array = [|
    Neutral "pillar_1x1x1"
    Neutral "pillar_1x1x2"
    Neutral "pillar_1x1x4"
    Neutral "pillar_1x1x8"
    Neutral "pillar_2x2x2"
    Neutral "pillar_2x2x4"
    Neutral "pillar_2x2x8"
  |]

  let getAssetsByMode(modeName: TerrainAssets) =
    match modeName with
    | Exploration -> {
        Collections.empty with
            Platforms = platforms
            Slopes = slopes
            Arches = arches
            Collectibles = collectibles
            Decorations = decorations
            NeutralSpecial = neutralSpecial
      }

    | Challenge -> {
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

  let getAllAssets() = {
    Collections.empty with
        Platforms = platforms
        Slopes = slopes
        Barriers = barriers
        Arches = arches
        Pipes = pipes
        Interactive = interactive
        Collectibles = collectibles
        Decorations = decorations
        Pillars = pillars
        NeutralSpecial = neutralSpecial
  }


  let getNextColor(current: ColorVariant) =
    match current with
    | ColorVariant.Blue -> ColorVariant.Green
    | ColorVariant.Green -> ColorVariant.Red
    | ColorVariant.Red -> ColorVariant.Yellow
    | ColorVariant.Yellow -> ColorVariant.Blue
    | ColorVariant.Neutral -> ColorVariant.Blue
