namespace FsBump.Core

open Mibo.Elmish
open Mibo.Input
open Microsoft.Xna.Framework
open Mibo.Rendering.Graphics3D
open FsBump.Core.RootComposition

module Player =

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

  module Operations =
    /// Initialize player state
    let create initialPos = {
      Body = {
        Position = initialPos
        Velocity = Vector3.Zero
        Radius = 0.5f
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

      let cutoffSq = 1600.0f
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

      let newRingRotation = model.RingRotation + (dt * 1.5f)

      // 4. Safety & Respawn logic
      let killFloor =
        if map.Length = 0 then
          -20.0f
        else
          let mutable minY = map.[0].Position.Y

          for i = 1 to map.Length - 1 do
            if map.[i].Position.Y < minY then
              minY <- map.[i].Position.Y

          minY - 15.0f

      let mutable nextLastSafePos = model.LastSafePosition

      if isGrounded then
        let mutable foundSafe = false

        for i = 0 to nearbyTiles.Count - 1 do
          if not foundSafe then
            let t = nearbyTiles.[i]

            if
              Vector3.DistanceSquared(t.Position, bodyAfterPhysics.Position) < 16.0f
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

  let init initialPos = Operations.create initialPos, Cmd.none

  let update msg (model: PlayerModel) =
    match msg with
    | InputChanged input -> { model with Input = input }, Cmd.none
    | PlaySound _ -> model, Cmd.none

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

  let draw
    (env: #IModelStoreProvider)
    (model: PlayerModel)
    (buffer: PipelineBuffer<RenderCommand>)
    =
    env.ModelStore.GetMesh Assets.PlayerBall
    |> ValueOption.iter(fun playerMesh ->
      // Pulse Logic
      let t = sin(model.Time * 4.0f) * 0.5f + 0.5f // 0.0 to 1.0
      let pulseColor = Color.Lerp(Color.SaddleBrown, Color.SandyBrown, t)
      let pulseIntensity = 2.0f + (t * 2.5f) // Ranges 2.0 to 4.5

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
        .Submit())

    env.ModelStore.GetTexture "Textures/saturn_rings"
    |> ValueOption.iter(fun ringsTexture ->
      // Ring 1: Main fast ring
      let s1 = 4.0f + sin(model.Time * 2.0f) * 0.1f

      let ringMatrix1 =
        Matrix.CreateRotationX(MathHelper.ToRadians 10f)
        * Matrix.CreateRotationY model.RingRotation
        * Matrix.CreateTranslation model.Body.Position

      buffer
        .QuadTransparent(
          ringsTexture,
          quad {
            at Vector3.Zero
            onXZ(Vector2(s1, s1))
            color Color.White
            relativeTo ringMatrix1
          }
        )
        .Submit()

      // Ring 2: Slower, larger, " ghost" ring for volume
      let s2 = 4.5f + cos(model.Time * 1.5f) * 0.2f

      let ringMatrix2 =
        Matrix.CreateRotationX(MathHelper.ToRadians 10f)
        * Matrix.CreateRotationY model.RingRotation
        * Matrix.CreateTranslation model.Body.Position

      buffer
        .QuadTransparent(
          ringsTexture,
          quad {
            at Vector3.Zero
            onXZ(Vector2(s2, s2))
            color(Color.White * 0.6f) // Semi-transparent ghost
            relativeTo ringMatrix2
          }
        )
        .Submit())
