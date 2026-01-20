namespace FsBump.Core

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Mibo.Elmish
open Mibo.Rendering.Graphics3D
open Mibo.Elmish.Graphics2D
open Mibo.Input
open FsBump.Core.Audio
open FsBump.Core.RootComposition

module Program =

  // ─────────────────────────────────────────────────────────────
  // Model
  // ─────────────────────────────────────────────────────────────

  type Model = {
    Player: PlayerModel
    Map: Tile list
    PathState: PathState
    Env: AppEnv
    Camera: Camera.State
    Skybox: Skybox.State
    SkyboxEffect: Effect
    TouchState: TouchLogic.State
  }

  let mergeInput
    (a: ActionState<PlayerAction>)
    (b: ActionState<PlayerAction>)
    : ActionState<PlayerAction> =
    {
      a with
          Held = Set.union a.Held b.Held
          Started = Set.union a.Started b.Started
          Released = Set.union a.Released b.Released
    }

  // ─────────────────────────────────────────────────────────────
  // Messages
  // ─────────────────────────────────────────────────────────────

  type Msg =
    | Tick of GameTime
    | PlayerMsg of Player.Msg
    | InputChanged of ActionState<PlayerAction>
    | GenerateMap
    | PlayAudio of AudioId
    | BakeGeometry

  // ─────────────────────────────────────────────────────────────
  // Init
  // ─────────────────────────────────────────────────────────────

  let init(ctx: GameContext) : struct (Model * Cmd<Msg>) =
    let modelStore = ModelStore.create ctx
    Assets.load modelStore

    let env = {
      ModelStore = modelStore
      Rng = Random.Shared
      Audio = Audio.create ctx
    }

    env.Audio.Play AmbientMusic

    let skyEffect = Assets.skyboxEffect ctx
    let skyState = Skybox.init()

    let sSize, sOffset, sAsset = TileBuilder.getAssetData "platform_4x4x1" 0 env

    let startPlatform = {
      Type = TileType.Platform
      Collision = CollisionType.Solid
      Position = Vector3.Zero
      Rotation = 0.0f
      Variant = 0
      Size = sSize
      Style = 0
      AssetName = sAsset
      VisualOffset = sOffset
    }

    let genConfig = {
      MaxJumpHeight = 2.8f
      MaxJumpDistance = 7.0f
      SafetyBuffer = 0.1f
    }

    let initialPath = {
      MapGenerator.createInitialState() with
          Position = Vector3(0.0f, 0.0f, -2.0f)
    }

    let t1, st1 =
      MapGenerator.generateSegment env initialPath [ startPlatform ] genConfig

    let t2, st2 =
      MapGenerator.generateSegment env st1 (startPlatform :: t1) genConfig

    let t3, st3 =
      MapGenerator.generateSegment env st2 (startPlatform :: t1 @ t2) genConfig

    let spawnVec = MapGenerator.getSpawnPoint()
    let player, pCmd = Player.init spawnVec

    let vp = ctx.GraphicsDevice.Viewport
    let screenSize = Vector2(float32 vp.Width, float32 vp.Height)

    {
      Player = player
      Map = [ startPlatform ] @ t1 @ t2 @ t3
      PathState = st3
      Env = env
      Camera = {
        Position = spawnVec + Vector3(0.0f, 10.0f, 10.0f)
        Target = spawnVec
      }
      Skybox = skyState
      SkyboxEffect = skyEffect
      TouchState = TouchLogic.init screenSize
    },
    Cmd.map PlayerMsg pCmd

  // ─────────────────────────────────────────────────────────────
  // Update
  // ─────────────────────────────────────────────────────────────

  let update (msg: Msg) (model: Model) : struct (Model * Cmd<Msg>) =
    match msg with
    | InputChanged input ->
      {
        model with
            Player = { model.Player with Input = input }
      },
      Cmd.none
    | GenerateMap ->
      let result =
        MapGenerator.Operations.updateMap
          model.Env
          model.Player.Body.Position
          model.Map
          model.PathState

      match result with
      | Some(map', path') ->
        {
          model with
              Map = map'
              PathState = path'
        },
        Cmd.none
      | None -> model, Cmd.none
    | Tick gt ->
      let dt = float32 gt.ElapsedGameTime.TotalSeconds

      // 1. Update transient touch logic
      let touchState' =
        TouchLogic.update model.TouchState.ScreenSize model.TouchState

      // 2. Compute effective input for THIS frame (do not save merged state back to model)
      let touchInput = TouchLogic.toActionState touchState'
      let effectiveInput = mergeInput model.Player.Input touchInput

      // 3. Update player physics using effective input
      let player', pCmd =
        Player.Operations.updateTick dt model.Env model.Map {
          model.Player with
              Input = effectiveInput
        }

      let camera' = Camera.update dt player'.Body.Position model.Camera
      let sky' = Skybox.update dt model.Skybox

      let genCmd =
        if
          MapGenerator.Operations.needsUpdate
            player'.Body.Position
            model.PathState
        then
          Cmd.deferNextFrame(Cmd.ofMsg GenerateMap)
        else
          Cmd.none

      {
        model with
            // Preserve the original persistent input (keyboard), 
            // but update the body/state from physics result
            Player = { player' with Input = model.Player.Input }
            Camera = camera'
            Skybox = sky'
            TouchState = touchState'
      },
      Cmd.batch2(genCmd, Cmd.map PlayerMsg pCmd)
    | PlayerMsg pMsg ->
      let nextPlayer, pCmd = Player.update pMsg model.Player

      let interceptedCmd =
        match pMsg with
        | Player.PlaySound audioId -> Cmd.ofMsg(PlayAudio audioId)
        | _ -> Cmd.none

      { model with Player = nextPlayer },
      Cmd.batch2(Cmd.map PlayerMsg pCmd, interceptedCmd)
    | PlayAudio audioId ->
      model.Env.Audio.Play(audioId)
      model, Cmd.none
    | BakeGeometry ->
      model.Env.ModelStore.Bake()
      model, Cmd.none

  // ─────────────────────────────────────────────────────────────
  // View
  // ─────────────────────────────────────────────────────────────

  let viewUI
    (ctx: GameContext)
    (model: Model)
    (buffer: Mibo.Elmish.RenderBuffer<int<RenderLayer>, RenderCmd2D>)
    =
    let vp = ctx.GraphicsDevice.Viewport
    let screenSize = Vector2(float32 vp.Width, float32 vp.Height)
    TouchUI.draw model.Env screenSize model.TouchState buffer

  let view
    (ctx: GameContext)
    (model: Model)
    (buffer: PipelineBuffer<RenderCommand>)
    =
    let camera =
      Camera.perspective
        model.Camera.Position
        model.Camera.Target
        Vector3.Up
        (MathHelper.ToRadians 45.f)
        (float32 ctx.GraphicsDevice.Viewport.AspectRatio)
        0.1f
        2000.f

    let lighting = {
      Lighting.defaultSunlight with
          AmbientColor = Color.DarkSlateBlue
          Lights = [|
            Player.getLight model.Player

            Light.Directional {
              Direction = Vector3.Normalize(Vector3(-1.0f, -1.0f, -0.5f))
              Color = Color.White
              Intensity = 1.5f
              Shadow = ValueNone
              CascadeCount = 0
              CascadeSplits = [||]
              SourceRadius = 0.0f
            }
          |]
    }

    let frustum = BoundingFrustum(camera.View * camera.Projection)

    buffer
      .Camera(camera)
      .Lighting(lighting)
      .Clear(Color.CornflowerBlue)
      .ClearDepth()
      .Custom(
        Skybox.draw
          model.Env
          model.Camera.Position
          model.SkyboxEffect
          model.Skybox
      )
      .Submit()


    MapGenerator.draw model.Env frustum model.Map buffer
    Player.draw model.Env model.Player buffer


  let create() =
    Program.mkProgram init update
    |> Program.withAssets
    |> Program.withPipeline
      (PipelineConfig.defaults
       |> PipelineConfig.withShadows(
         ShadowConfig.defaults |> ShadowConfig.withBias 0.0001f 0.0005f
       )
       |> PipelineConfig.withShader
         ShaderBase.ShadowCaster
         "Effects/ShadowCaster"
       |> PipelineConfig.withShader ShaderBase.PBRForward "Effects/PBR")
      view
    |> Program.withInput
    |> Program.withSubscription(fun ctx _ ->
      InputMapper.subscribeStatic
        Player.Input.config
        (fun input -> InputChanged input)
        ctx)
    |> Program.withTick Tick
    |> Program.withConfig(fun (game, graphics) ->
      game.Content.RootDirectory <- "Content"
      game.Window.Title <- "Procedural Map"
      game.IsMouseVisible <- true
      graphics.PreferMultiSampling <- true
      graphics.SynchronizeWithVerticalRetrace <- true

      graphics.PreparingDeviceSettings.Add(fun e ->
        let pp = e.GraphicsDeviceInformation.PresentationParameters
        pp.MultiSampleCount <- 8))
