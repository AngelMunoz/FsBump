namespace FsBump.Core

open Mibo.Elmish
open Mibo.Input
open Microsoft.Xna.Framework
open Mibo.Rendering.Graphics3D
open FsBump.Core.RootComposition

module Player =
  open Microsoft.Xna.Framework.Graphics

  module Config =
    module Physics =
      let Radius = 0.5f
      let CutoffDistanceSq = 1600.0f
      let SafePositionCheckDistSq = 16.0f
      let KillFloorOffset = 15.0f
      let KillFloorDefault = -20.0f

    module Visuals =
      let RingRotationSpeed = 1.5f
      let RingTilt = MathHelper.ToRadians 10.0f
      // Pulse
      let PulseSpeed = 4.0f
      let PulseIntensityBase = 2.0f
      let PulseIntensityRange = 2.5f
      // Colors
      let ColorDark = Color.SaddleBrown
      let ColorLight = Color.SandyBrown
      // Rings
      let Ring1BaseScale = 4.0f
      let Ring1PulseSpeed = 2.0f
      let Ring1PulseAmount = 0.1f
      let Ring2BaseScale = 4.5f
      let Ring2PulseSpeed = 1.5f
      let Ring2PulseAmount = 0.2f
      let Ring2Opacity = 0.6f

  module Input =
    open Microsoft.Xna.Framework.Input

    let config =
      InputMap.empty
      |> InputMap.key MoveForward Keys.W
      |> InputMap.key MoveForward Keys.Up
      |> InputMap.key MoveBackward Keys.S
      |> InputMap.key MoveBackward Keys.Down
      |> InputMap.key MoveLeft Keys.A
      |> InputMap.key MoveLeft Keys.Left
      |> InputMap.key MoveRight Keys.D
      |> InputMap.key MoveRight Keys.Right
      |> InputMap.key Jump Keys.Space

  type Msg =
    | InputChanged of ActionState<PlayerAction>
    | PlaySound of AudioId

  module Logic =
    /// Initialize player state
    let create initialPos = {
      Body = {
        Position = initialPos
        Velocity = Vector3.Zero
        Radius = Config.Physics.Radius
      }
      Input = ActionState.empty
      AnalogDir = Microsoft.Xna.Framework.Vector2.Zero
      IsGrounded = false
      Rotation = Quaternion.Identity
      LastSafePosition = initialPos
      RingRotation = 0.0f
      Time = 0.0f
    }

    /// Orchestrate one tick of player logic
    let updateTick
      (dt: float32)
      (cameraYaw: float32)
      (env: AppEnv)
      (map: Tile array)
      (model: PlayerModel)
      =
      let jumpRequested = model.Input.Started.Contains Jump

      // 1. Movement System
      let bodyAfterMovement: Body =
        Movement.update dt cameraYaw model.Input model.AnalogDir model.Body

      // 2. Physics System
      env.CollisionBuffer.Clear()

      let cutoffSq = Config.Physics.CutoffDistanceSq
      let pos = bodyAfterMovement.Position

      // Efficient filtering using ResizeArray
      for i = 0 to map.Length - 1 do
        let t = map.[i]

        if Vector3.DistanceSquared(t.Position, pos) < cutoffSq then
          env.CollisionBuffer.Add(t)

      let nearbyTiles = env.CollisionBuffer

      let struct (bodyAfterPhysics, isGrounded, didJump) =
        Physics.updateBody
          dt
          bodyAfterMovement
          nearbyTiles
          env
          jumpRequested
          model.IsGrounded

      // 3. Rotation System
      let newRotation =
        Rotation.update dt bodyAfterPhysics.Velocity model.Rotation

      let newRingRotation =
        model.RingRotation + (dt * Config.Visuals.RingRotationSpeed)

      // 4. Safety & Respawn logic
      let killFloor =
        if map.Length = 0 then
          Config.Physics.KillFloorDefault
        else
          let minY = map |> Array.minBy(fun t -> t.Position.Y) |> _.Position.Y
          minY - Config.Physics.KillFloorOffset

      let mutable nextLastSafePos = model.LastSafePosition

      if isGrounded then
        let mutable foundSafe = false

        for i = 0 to nearbyTiles.Count - 1 do
          if not foundSafe then
            let t = nearbyTiles.[i]

            if
              Vector3.DistanceSquared(t.Position, bodyAfterPhysics.Position) < Config.Physics.SafePositionCheckDistSq
            then
              nextLastSafePos <- t.Position + Vector3.Up * 1.0f
              foundSafe <- true

      let model =
        if bodyAfterPhysics.Position.Y < killFloor then
          {
            model with
                Body = {
                  bodyAfterPhysics with
                      Position = model.LastSafePosition + Vector3.Up * 2.5f
                      Velocity = Vector3.Zero
                }
                IsGrounded = false
                Rotation = Quaternion.Identity
                Input = {
                  model.Input with
                      Held = Set.empty
                      Started = Set.empty
                }
                RingRotation = 0.0f
                Time = model.Time + dt
          }
        else
          {
            model with
                Body = bodyAfterPhysics
                IsGrounded = isGrounded
                Rotation = newRotation
                LastSafePosition = nextLastSafePos
                RingRotation = newRingRotation
                Time = model.Time + dt
          }

      let cmd = if didJump then Cmd.ofMsg(PlaySound JumpSound) else Cmd.none

      model, cmd

  let init initialPos = Logic.create initialPos, Cmd.none

  let update msg (model: PlayerModel) =
    match msg with
    | InputChanged input -> { model with Input = input }, Cmd.none
    | PlaySound _ -> model, Cmd.none

  module View =
    let getLight(model: PlayerModel) =
      Light.Spot {
        Position = model.Body.Position + Vector3(0.0f, 8.0f, 2.0f)
        Direction = Vector3.Normalize(Vector3(0.0f, -1.0f, -0.1f))
        Color = Color.Cyan
        Intensity = 2.0f
        Range = 100.0f
        InnerConeAngle = MathHelper.ToRadians 30.f
        OuterConeAngle = MathHelper.ToRadians 80.f
        Shadow = ValueSome ShadowSettings.defaults
        SourceRadius = 0.1f
      }

    let drawPlayer
      (model: PlayerModel)
      (buffer: PipelineBuffer<RenderCommand>)
      (playerMesh: Mesh)
      =
      // Pulse Logic
      let t = sin(model.Time * Config.Visuals.PulseSpeed) * 0.5f + 0.5f

      let pulseColor =
        Color.Lerp(Config.Visuals.ColorDark, Config.Visuals.ColorLight, t)

      let pulseIntensity =
        Config.Visuals.PulseIntensityBase
        + t * Config.Visuals.PulseIntensityRange

      buffer
        .Draw(
          draw {
            mesh playerMesh
            scaledBy(model.Body.Radius * 2.0f)
            rotatedBy model.Rotation
            at model.Body.Position
            withEmissive pulseColor pulseIntensity
          }
        )
        .Submit()

    let drawRings
      (model: PlayerModel)
      (buffer: PipelineBuffer<RenderCommand>)
      (texture: Texture2D)
      =
      let tilt =
        Quaternion.CreateFromAxisAngle(Vector3.UnitX, Config.Visuals.RingTilt)

      let spin =
        Quaternion.CreateFromAxisAngle(Vector3.UnitY, model.RingRotation)

      let ringMatrix =
        Matrix.CreateFromQuaternion(spin * tilt)
        * Matrix.CreateTranslation model.Body.Position

      // Ring 1: Main fast ring
      let s1 =
        Config.Visuals.Ring1BaseScale
        + sin(model.Time * Config.Visuals.Ring1PulseSpeed)
          * Config.Visuals.Ring1PulseAmount

      buffer
        .QuadTransparent(
          texture,
          quad {
            at Vector3.Zero
            onXZ(Vector2(s1, s1))
            relativeTo ringMatrix
            color Color.White
          }
        )
        .Submit()

      // Ring 2: Slower ghost ring
      let s2 =
        Config.Visuals.Ring2BaseScale
        + cos(model.Time * Config.Visuals.Ring2PulseSpeed)
          * Config.Visuals.Ring2PulseAmount

      buffer
        .QuadTransparent(
          texture,
          quad {
            at Vector3.Zero
            onXZ(Vector2(s2, s2))
            relativeTo ringMatrix
            color(Color.White * Config.Visuals.Ring2Opacity)
          }
        )
        .Submit()

  let draw
    (env: #IModelStoreProvider)
    (model: PlayerModel)
    (buffer: PipelineBuffer<RenderCommand>)
    =
    env.ModelStore.GetMesh Assets.PlayerBall
    |> ValueOption.iter(View.drawPlayer model buffer)

    env.ModelStore.GetTexture Assets.SaturnRings
    |> ValueOption.iter(View.drawRings model buffer)
