namespace FsBump.Core

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
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

  [<Struct>]
  type Model = {
    Player: PlayerModel
    Map: Tile array
    PathGraph: PathGraph
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
      CollisionBuffer = ResizeArray<Tile>(100)
      RenderBuffer = ResizeArray<RenderCommand>(200)
    }

    env.Audio.Play AmbientMusic

    let skyEffect = Assets.skyboxEffect ctx
    let skyState = Skybox.init()

    let sSize, sOffset, sAsset = TileBuilder.getAssetData "platform_4x4x1" 0 env

    // Initialize Infinite Mode
    let initialGraph = MapGenerator.createInitialState GameMode.Infinite (Random.Shared.Next())
    let mainPathId = initialGraph.Paths.[0].Id

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
      PathId = mainPathId
      SegmentIndex = -1 // Special index for start
    }
    
    // Generate initial map (about 60 units worth)
    let mutable currentGraph = initialGraph
    let mutable currentMap = [| startPlatform |]
    
    // We need to simulate generation to get a decent start
    // Force a few update cycles
    for i in 1..3 do
        let result = MapGenerator.Operations.updateMap env (VectorMath.add Vector3.Zero (VectorMath.create 0.0f 0.0f (float32 (-i * 20)))) currentMap currentGraph
        match result with
        | ValueSome(map, graph) ->
            currentMap <- map
            currentGraph <- graph
        | ValueNone -> ()

    let spawnVec = MapGenerator.getSpawnPoint()
    let player, pCmd = Player.init spawnVec

    let vp = ctx.GraphicsDevice.Viewport
    let screenSize = Vector2(float32 vp.Width, float32 vp.Height)

    {
      Player = player
      Map = currentMap
      PathGraph = currentGraph
      Env = env
      Camera = Camera.init spawnVec
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
          model.PathGraph

      match result with
      | ValueSome(map', graph') ->
        {
          model with
              Map = map'
              PathGraph = graph'
        },
        Cmd.none
      | ValueNone -> model, Cmd.none
    | Tick gt ->
      let dt = float32 gt.ElapsedGameTime.TotalSeconds

      // 1. Update transient touch logic
      let touchState' =
        TouchLogic.update model.TouchState.ScreenSize model.TouchState

      // 2. Compute effective input for THIS frame (do not save merged state back to model)
      let struct (touchInput, analogInput) =
        TouchLogic.getEffectiveInput touchState'

      let effectiveInput = mergeInput model.Player.Input touchInput

      // 3. Update Camera (Trailing)
      let camera' =
        Camera.update
          dt
          model.Player.Body.Position
          model.Player.Body.Velocity
          (not effectiveInput.Held.IsEmpty
           || analogInput.LengthSquared() > 0.001f)
          model.Camera

      // 4. Update player physics using effective input and Camera Yaw
      let playerModelForUpdate = {
        model.Player with
            Input = effectiveInput
            AnalogDir = analogInput
      }

      let player', pCmd =
        Player.Logic.updateTick
          dt
          camera'.Yaw
          model.Env
          model.Map
          playerModelForUpdate

      let sky' = Skybox.update dt model.Skybox

      // Simple check to trigger generation: if any active path end is near (80 units)
      let needsUpdate = 
          PathGraphSystem.getActivePaths model.PathGraph
          |> Array.exists (fun p -> Vector3.Distance(player'.Body.Position, p.Position) < 80.0f)

      let genCmd =
        if needsUpdate then
          Cmd.deferNextFrame(Cmd.ofMsg GenerateMap)
        else
          Cmd.none

      {
        model with
            Player = {
              player' with
                  Input = model.Player.Input
            }
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
          Lights = [|
            Player.View.getLight model.Player

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


    MapGenerator.draw model.Env frustum model.Player.Body.Position model.Map buffer
    Player.draw model.Env model.Player buffer


  let create() =
    Program.mkProgram init update
    |> Program.withAssets
    |> Program.withPipeline
      (PipelineConfig.defaults
       |> PipelineConfig.withShadows(
         ShadowConfig.defaults |> ShadowConfig.withBias 0.001f 0.005f
       )
       |> PipelineConfig.withPostProcess(
         PostProcessConfig.defaults
         |> PostProcessConfig.withBloom {
           BloomConfig.defaults with
               Threshold = 0.85f
               Intensity = 1.2f
               Scatter = 0.8f
         }
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

      // Set default resolution
      graphics.PreferredBackBufferWidth <- 1280
      graphics.PreferredBackBufferHeight <- 720
      // Use borderless full screen to preserve desktop resolution
      graphics.HardwareModeSwitch <- false

      graphics.PreferredDepthStencilFormat <- DepthFormat.Depth24
      graphics.PreferMultiSampling <- true
      graphics.SynchronizeWithVerticalRetrace <- true

      graphics.PreparingDeviceSettings.Add(fun e ->
        let pp = e.GraphicsDeviceInformation.PresentationParameters
        pp.MultiSampleCount <- 2
        e.GraphicsDeviceInformation.GraphicsProfile <- GraphicsProfile.HiDef))