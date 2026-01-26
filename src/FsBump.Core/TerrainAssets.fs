namespace FsBump.Core

module TerrainAssets =

  type Collections = {
    Platforms: string[]
    Slopes: string[]
    Barriers: string[]
    Arches: string[]
    Pipes: string[]
    Interactive: string[]
    Collectibles: string[]
    Decorations: string[]
    Pillars: string[]
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
    }

  [<Struct>]
  type TerrainAssets =
    | Exploration
    | Challenge
    | Infinite

  // Platforms (all sizes: 1x1x1 to 6x6x4)
  let platforms = [|
    "platform_1x1x1"
    "platform_1x1x2"
    "platform_1x1x4"
    "platform_2x2x1"
    "platform_2x2x2"
    "platform_2x2x4"
    "platform_4x2x1"
    "platform_4x2x2"
    "platform_4x2x4"
    "platform_4x4x1"
    "platform_4x4x2"
    "platform_4x4x4"
    "platform_6x2x1"
    "platform_6x2x2"
    "platform_6x2x4"
    "platform_6x6x1"
    "platform_6x6x2"
    "platform_6x6x4"
    "platform_decorative_1x1x1"
    "platform_decorative_2x2x2"
    "platform_arrow_2x2x1"
    "platform_arrow_4x4x1"
    "platform_arrow_6x6x1"
    "platform_hole_6x6x1"
  |]

  // Slopes (9 sizes: 2x2x2 to 6x6x4)
  let slopes = [|
    "platform_slope_2x2x2"
    "platform_slope_2x4x4"
    "platform_slope_2x6x4"
    "platform_slope_4x2x2"
    "platform_slope_4x4x4"
    "platform_slope_4x6x4"
    "platform_slope_6x2x2"
    "platform_slope_6x4x4"
    "platform_slope_6x6x4"
  |]

  // Barriers (12 sizes per color + 9 neutral)
  let barriers = [|
    "barrier_1x1x1"
    "barrier_1x1x2"
    "barrier_1x1x4"
    "barrier_2x1x1"
    "barrier_2x1x2"
    "barrier_2x1x4"
    "barrier_3x1x1"
    "barrier_3x1x2"
    "barrier_3x1x4"
    "barrier_4x1x1"
    "barrier_4x1x2"
    "barrier_4x1x4"
    "barrier_column_1x1x2"
    "barrier_column_1x1x4"
    "barrier_corner_1x1x1"
    "barrier_corner_1x1x2"
    "barrier_corner_1x1x4"
  |]

  // Arches (3 per color)
  let arches = [| "arch"; "arch_tall"; "arch_wide" |]

  // Pipes (7 per color)
  let pipes = [|
    "pipe_straight_A"
    "pipe_straight_B"
    "pipe_end"
    "pipe_90_A"
    "pipe_90_B"
    "pipe_180_A"
    "pipe_180_B"
  |]

  // Interactive elements
  let interactive = [|
    "spring_pad"
    "button_base"
    "button_base_small"
    "lever_floor_base"
    "lever_wall_base_A"
    "lever_wall_base_B"
    "power"
  |]

  // Collectibles
  let collectibles = [| "star"; "heart"; "diamond"; "cone" |]

  // Decorative
  let decorations = [|
    "flag_A"
    "flag_B"
    "flag_C"
    "hoop"
    "hoop_angled"
    "railing_straight_single"
    "railing_straight_double"
    "railing_straight_padded"
    "railing_corner_single"
    "railing_corner_double"
    "railing_corner_padded"
    "bracing_small"
    "bracing_medium"
    "bracing_large"
    "bomb_A"
    "bomb_B"
    "structure_A"
    "structure_B"
    "structure_C"
    "strut_horizontal"
    "strut_vertical"
    "signage_arrow_stand"
    "signage_arrow_wall"
    "signage_arrows_left"
    "signage_arrows_right"
  |]

  let pillars = [|
    "pillar_1x1x1"
    "pillar_1x1x2"
    "pillar_1x1x4"
    "pillar_1x1x8"
    "pillar_2x2x2"
    "pillar_2x2x4"
    "pillar_2x2x8"
  |]

  // Get assets by game mode
  let getAssetsByMode(modeName: TerrainAssets) =
    // Placeholder for phase 2 logic
    match modeName with
    | Exploration ->
        // More decorations, wider paths, arches, collectibles
        {
          Collections.empty with
              Platforms = platforms
              Slopes = slopes
              Arches = arches
              Collectibles = collectibles
              Decorations = decorations
        }

    | Challenge ->
        // More barriers, narrower paths, technical platforms

        {
          Collections.empty with
              Barriers = barriers
              Pipes = pipes
              Interactive = interactive
              Pillars = pillars
        }

    | Infinite ->
        // Balanced mix
        {
          Collections.empty with
              Platforms = platforms
              Slopes = slopes
              Barriers = barriers
              Arches = arches
              Interactive = interactive
              Collectibles = collectibles
              Decorations = decorations
        }


  let getNextColor(current: ColorVariant) =
    match current with
    | ColorVariant.Blue -> ColorVariant.Green
    | ColorVariant.Green -> ColorVariant.Red
    | ColorVariant.Red -> ColorVariant.Yellow
    | ColorVariant.Yellow -> ColorVariant.Blue
    | ColorVariant.Neutral -> ColorVariant.Blue
