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
    JumpTouchId: int option
    IsNewJump: bool
    ScreenSize: Vector2
  }

  let init(screenSize: Vector2) = {
    Joystick = {
      Center = Vector2.Zero
      Current = Vector2.Zero
      ActiveId = None
    }
    JumpTouchId = None
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
    | Some id ->
      match touches |> Seq.tryFind (fun t -> t.Id = id) with
      | Some t when t.State <> TouchLocationState.Released ->
          let currentPos = t.Position
          let diff = currentPos - nextJoystick.Center
          let dist = diff.Length()
          
          // Floating Joystick: Center follows if we move too far
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
          nextJoystick <- { Center = Vector2.Zero; Current = Vector2.Zero; ActiveId = None }
    | None -> ()

    // 2. Update Jump
    match nextJumpId with
    | Some id ->
       if not (touches |> Seq.exists (fun t -> t.Id = id && t.State <> TouchLocationState.Released)) then
         nextJumpId <- None
    | None -> ()

    // 3. Process New/Missed Touches
    for touch in touches do
      if touch.State <> TouchLocationState.Released then
          match getZone touch.Position screenSize with
          | LeftSide ->
              if nextJoystick.ActiveId.IsNone then
                  nextJoystick <- {
                    Center = touch.Position
                    Current = touch.Position
                    ActiveId = Some touch.Id
                  }
          | RightSide ->
              if nextJumpId.IsNone then
                  nextJumpId <- Some touch.Id
                  isNewJump <- true

    {
      Joystick = nextJoystick
      JumpTouchId = nextJumpId
      IsNewJump = isNewJump
      ScreenSize = screenSize
    }

  /// Map touch state to PlayerAction state
  let toActionState(state: State) =
    let mutable held = Set.empty
    let mutable started = Set.empty

    match state.Joystick.ActiveId with
    | Some _ ->
      let diff = state.Joystick.Current - state.Joystick.Center
      
      // Activation threshold (how far to drag before moving starts)
      let activationThreshold = 15.0f 
      
      if diff.Length() > activationThreshold then
        let absX = abs diff.X
        let absY = abs diff.Y
        
        // Horizontal/Vertical bias: If one axis is significantly stronger, 
        // snap to that axis for better cardinal control.
        let biasRatio = 0.4f
        let moveUp = diff.Y < -activationThreshold
        let moveDown = diff.Y > activationThreshold
        let moveLeft = diff.X < -activationThreshold
        let moveRight = diff.X > activationThreshold

        if absY > absX * biasRatio then
            if moveUp then held <- held.Add MoveForward
            if moveDown then held <- held.Add MoveBackward

        if absX > absY * biasRatio then
            if moveLeft then held <- held.Add MoveLeft
            if moveRight then held <- held.Add MoveRight
    | None -> ()

    if state.IsNewJump then
      started <- started.Add Jump

    let baseInput: ActionState<PlayerAction> = ActionState.empty
    { baseInput with Held = held; Started = started }

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
    |> Option.iter(fun texture ->
      // Draw Joystick
      match state.Joystick.ActiveId with
      | Some _ ->
        let center = state.Joystick.Center
        let current = state.Joystick.Current
        let offset = 48.0f

        let destRect (pos: Vector2) (scale: float32) =
          let size = float32 tileSize * scale
          Rectangle(int(pos.X - size * 0.5f), int(pos.Y - size * 0.5f), int size, int size)

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
      let jumpColor = if state.JumpTouchId.IsSome then Color.Gray else Color.White
      let jumpSize = float32 tileSize * 3.0f

      let jumpRect =
        Rectangle(int(jumpPos.X - jumpSize * 0.5f), int(jumpPos.Y - jumpSize * 0.5f), int jumpSize, int jumpSize)

      Draw2D.sprite texture jumpRect
      |> Draw2D.withSource buttonA
      |> Draw2D.withColor jumpColor
      |> Draw2D.submit buffer)
