namespace FsBump.Core

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input.Touch
open Mibo.Input

module TouchLogic =

  type TouchZone =
    | LeftSide // Joystick
    | RightSide // Jump

  type JoystickState = {
    Center: Vector2
    Current: Vector2
    ActiveId: int option
  }

  type State = {
    Joystick: JoystickState
    JumpTriggered: bool
    ScreenSize: Vector2
  }

  let init(screenSize: Vector2) = {
    Joystick = {
      Center = Vector2.Zero
      Current = Vector2.Zero
      ActiveId = None
    }
    JumpTriggered = false
    ScreenSize = screenSize
  }

  let private getZone (pos: Vector2) (screenSize: Vector2) =
    if pos.X < screenSize.X * 0.5f then LeftSide else RightSide

  let update (screenSize: Vector2) (state: State) =
    let touches = TouchPanel.GetState()
    let mutable nextJoystick = state.Joystick
    let mutable jumpTriggered = false

    // Reset joystick if touch ended
    match state.Joystick.ActiveId with
    | Some id ->
      let found =
        touches
        |> Seq.tryFind(fun t ->
          t.Id = id && t.State <> TouchLocationState.Released)

      match found with
      | Some t ->
        nextJoystick <- {
          nextJoystick with
              Current = t.Position
        }
      | None ->
        nextJoystick <- {
          Center = Vector2.Zero
          Current = Vector2.Zero
          ActiveId = None
        }
    | None -> ()

    // Process new touches
    for touch in touches do
      match touch.State with
      | TouchLocationState.Pressed ->
        match getZone touch.Position screenSize with
        | LeftSide when nextJoystick.ActiveId.IsNone ->
          nextJoystick <- {
            Center = touch.Position
            Current = touch.Position
            ActiveId = Some touch.Id
          }
        | RightSide -> jumpTriggered <- true
        | _ -> ()
      | _ -> ()

    {
      state with
          Joystick = nextJoystick
          JumpTriggered = jumpTriggered
          ScreenSize = screenSize
    }

  /// Map touch state to PlayerAction state
  let toActionState(state: State) =
    let mutable held = Set.empty
    let mutable started = Set.empty

    // 1. Joystick Movement
    match state.Joystick.ActiveId with
    | Some _ ->
      let diff = state.Joystick.Current - state.Joystick.Center
      let deadzone = 10.0f
      let maxDist = 100.0f

      if diff.Length() > deadzone then
        if diff.Y < -deadzone then
          held <- held.Add MoveForward

        if diff.Y > deadzone then
          held <- held.Add MoveBackward

        if diff.X < -deadzone then
          held <- held.Add MoveLeft

        if diff.X > deadzone then
          held <- held.Add MoveRight
    | None -> ()

    // 2. Jump
    if state.JumpTriggered then
      started <- started.Add Jump

    let baseInput: ActionState<PlayerAction> = ActionState.empty

    {
      baseInput with
          Held = held
          Started = started
    }

open Mibo.Elmish.Graphics3D

module TouchUI =
  open Mibo.Elmish.Graphics2D

  let private tileSize = 16

  // Helper to get source rect assuming 16 columns (256px width)
  // Adjust column count if texture is different width
  let private getKeyRect (row: int) (col: int) =
    Rectangle(col * tileSize, row * tileSize, tileSize, tileSize)

  // Key Mappings for gdb-switch-2 (Approximations)
  // Row 0: B, A, Y, X
  // Row 1: Pressed versions
  // D-Pad is further down. Let's use arrows for movement directions.
  // Arrows seem to be around row 7 or 8.
  let private arrowUp = getKeyRect 7 2
  let private arrowLeft = getKeyRect 7 0
  let private arrowDown = getKeyRect 7 1
  let private arrowRight = getKeyRect 7 3

  let private analogStick = getKeyRect 1 4 // Left Stick

  let private buttonA = getKeyRect 0 1 // A button for Jump

  let draw
    (env: #IModelStoreProvider)
    (screenSize: Vector2)
    (state: TouchLogic.State)
    (buffer: RenderBuffer<RenderCmd2D>)
    =
    env.ModelStore.GetTexture "gdb-switch-2"
    |> Option.iter(fun texture ->
      // Draw Joystick
      match state.Joystick.ActiveId with
      | Some _ ->
        let center = state.Joystick.Center
        let current = state.Joystick.Current
        let offset = 48.0f

        // Draw center (current position)
        // Draw2D.sprite expects Rectangle. We'll create one centered at position.
        let destRect (pos: Vector2) (scale: float32) =
          let size = float32 tileSize * scale

          Rectangle(
            int(pos.X - size * 0.5f),
            int(pos.Y - size * 0.5f),
            int size,
            int size
          )

        Draw2D.sprite texture (destRect current 2.0f)
        |> Draw2D.withSource analogStick
        |> Draw2D.withColor Color.White
        |> Draw2D.submit buffer

        // Draw Direction Hints
        Draw2D.sprite texture (destRect (center + Vector2(0.0f, -offset)) 1.5f)
        |> Draw2D.withSource arrowUp
        |> Draw2D.submit buffer

        Draw2D.sprite texture (destRect (center + Vector2(-offset, 0.0f)) 1.5f)
        |> Draw2D.withSource arrowLeft
        |> Draw2D.submit buffer

        Draw2D.sprite texture (destRect (center + Vector2(0.0f, offset)) 1.5f)
        |> Draw2D.withSource arrowDown
        |> Draw2D.submit buffer

        Draw2D.sprite texture (destRect (center + Vector2(offset, 0.0f)) 1.5f)
        |> Draw2D.withSource arrowRight
        |> Draw2D.submit buffer

      | None -> ()

      // Draw Jump Button Hint (Bottom Right)
      let jumpPos = screenSize - Vector2(96.0f, 96.0f)
      let jumpColor = if state.JumpTriggered then Color.Gray else Color.White

      let jumpSize = float32 tileSize * 3.0f

      let jumpRect =
        Rectangle(
          int(jumpPos.X - jumpSize * 0.5f),
          int(jumpPos.Y - jumpSize * 0.5f),
          int jumpSize,
          int jumpSize
        )

      Draw2D.sprite texture jumpRect
      |> Draw2D.withSource buttonA
      |> Draw2D.withColor jumpColor
      |> Draw2D.submit buffer)
