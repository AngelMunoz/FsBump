namespace FsBump.Core

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input.Touch
open Mibo.Input

module TouchLogic =

  [<Struct>]
  type TouchZone =
    | LeftSide // Joystick
    | RightSide // Jump

  [<Struct>]
  type JoystickState = {
    Center: Vector2
    Current: Vector2
    ActiveId: int voption
  }

  [<Struct>]
  type State = {
    Joystick: JoystickState
    JumpTouchId: int voption
    IsNewJump: bool
    ScreenSize: Vector2
  }

  let init(screenSize: Vector2) = {
    Joystick = {
      Center = Vector2.Zero
      Current = Vector2.Zero
      ActiveId = ValueNone
    }
    JumpTouchId = ValueNone
    IsNewJump = false
    ScreenSize = screenSize
  }

  let private getZone (pos: Vector2) (screenSize: Vector2) =
    if pos.X < screenSize.X * 0.5f then LeftSide else RightSide

  let update (screenSize: Vector2) (state: State) =
    let touches = TouchPanel.GetState()
    let mutable nextJoystick = state.Joystick
    let mutable nextJumpId = state.JumpTouchId
    let mutable isNewJump = false

    let maxJoystickRadius = 100.0f

    // 1. Update Joystick
    match nextJoystick.ActiveId with
    | ValueSome id ->
      match touches |> Seq.tryFind(fun t -> t.Id = id) with
      | Some t when t.State <> TouchLocationState.Released ->
        let currentPos = t.Position
        let diff = currentPos - nextJoystick.Center
        let dist = diff.Length()

        let nextCenter =
          if dist > maxJoystickRadius then
            currentPos - (Vector2.Normalize(diff) * maxJoystickRadius)
          else
            nextJoystick.Center

        nextJoystick <- {
          nextJoystick with
              Current = currentPos
              Center = nextCenter
        }
      | _ ->
        nextJoystick <- {
          Center = Vector2.Zero
          Current = Vector2.Zero
          ActiveId = ValueNone
        }
    | ValueNone -> ()

    // 2. Update Jump
    match nextJumpId with
    | ValueSome id ->
      if
        not(
          touches
          |> Seq.exists(fun t ->
            t.Id = id && t.State <> TouchLocationState.Released)
        )
      then
        nextJumpId <- ValueNone
    | ValueNone -> ()

    // 3. Process New/Missed Touches
    for touch in touches do
      if touch.State <> TouchLocationState.Released then
        match getZone touch.Position screenSize with
        | LeftSide ->
          if nextJoystick.ActiveId.IsNone then
            nextJoystick <- {
              Center = touch.Position
              Current = touch.Position
              ActiveId = ValueSome touch.Id
            }
        | RightSide ->
          if nextJumpId.IsNone then
            nextJumpId <- ValueSome touch.Id
            isNewJump <- true

    {
      Joystick = nextJoystick
      JumpTouchId = nextJumpId
      IsNewJump = isNewJump
      ScreenSize = screenSize
    }

  /// Map touch state to PlayerAction state and Analog Vector
  let getEffectiveInput(state: State) =
    let mutable held = Set.empty
    let mutable started = Set.empty
    let mutable analog = Vector2.Zero

    match state.Joystick.ActiveId with
    | ValueSome _ ->
      let diff = state.Joystick.Current - state.Joystick.Center
      let dist = diff.Length()
      let activationThreshold = 15.0f

      if dist > activationThreshold then
        let rawDir = Vector2.Normalize(diff)
        let absX = abs rawDir.X
        let absY = abs rawDir.Y
        let biasThreshold = 0.45f

        let biasedDir =
          if absX < biasThreshold then
            Vector2(0.0f, sign rawDir.Y |> float32)
          elif absY < biasThreshold then
            Vector2(sign rawDir.X |> float32, 0.0f)
          else
            rawDir

        analog <- biasedDir
    | ValueNone -> ()

    if state.IsNewJump then
      started <- started.Add Jump

    let baseInput: ActionState<PlayerAction> = ActionState.empty

    struct ({
              baseInput with
                  Held = held
                  Started = started
            },
            analog)

open Mibo.Elmish.Graphics3D

module TouchUI =
  open Mibo.Elmish.Graphics2D

  let private tileSize = 16

  // Helper to get source rect assuming 16 columns (256px width)
  let private getKeyRect (row: int) (col: int) =
    Rectangle(col * tileSize, row * tileSize, tileSize, tileSize)

  let private arrowUp = getKeyRect 7 2
  let private arrowLeft = getKeyRect 7 0
  let private arrowDown = getKeyRect 7 1
  let private arrowRight = getKeyRect 7 3
  let private analogStick = getKeyRect 1 4
  let private buttonA = getKeyRect 0 1

  let draw
    (env: #IModelStoreProvider)
    (screenSize: Vector2)
    (state: TouchLogic.State)
    (buffer: RenderBuffer<RenderCmd2D>)
    =
    env.ModelStore.GetTexture "gdb-switch-2"
    |> ValueOption.iter(fun texture ->
      // Draw Joystick
      match state.Joystick.ActiveId with
      | ValueSome _ ->
        let center = state.Joystick.Center
        let current = state.Joystick.Current
        let offset = 48.0f

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

      | ValueNone -> ()

      // Draw Jump Button Hint (Bottom Right)
      let jumpPos = screenSize - Vector2(96.0f, 96.0f)

      let jumpColor =
        if state.JumpTouchId.IsSome then Color.Gray else Color.White

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
