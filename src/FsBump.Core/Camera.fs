namespace FsBump.Core

open System
open Microsoft.Xna.Framework
open Mibo.Rendering.Graphics3D

module Camera =

  [<Struct>]
  type State = {
    Position: Vector3
    Target: Vector3
    Distance: float32
    Yaw: float32
    Pitch: float32
  }

  let init(target: Vector3) = {
    Position = target + Vector3(0.0f, 10.0f, 18.0f)
    Target = target
    Distance = 18.0f
    Yaw = 0.0f
    Pitch = MathHelper.ToRadians 25.0f // Looking down
  }

  /// Helper to interpolate angles correctly (handling the 360 wrap)
  let private lerpAngle (current: float32) (target: float32) (amount: float32) =
    let diff = target - current
    let diff = (diff + MathHelper.Pi) % (MathHelper.Pi * 2.0f) - MathHelper.Pi

    let diff =
      if diff < -MathHelper.Pi then
        diff + MathHelper.Pi * 2.0f
      else
        diff

    current + diff * amount

  /// Update camera to trail behind the target's movement
  let update
    (dt: float32)
    (targetPosition: Vector3)
    (targetVelocity: Vector3)
    (isMovingInput: bool)
    (state: State)
    : State =

    // Use horizontal speed for yaw updates to avoid rotation when falling straight down
    let horizontalVelocity = Vector2(targetVelocity.X, targetVelocity.Z)
    let horizontalSpeed = horizontalVelocity.Length()

    let nextYaw =
      // Only update rotation if the player is actively moving (input) AND moving fast enough.
      if isMovingInput && horizontalSpeed > 0.5f then
        let targetYaw = MathF.Atan2(-targetVelocity.X, -targetVelocity.Z)
        lerpAngle state.Yaw targetYaw (dt * 1.2f)
      else
        state.Yaw

    // We keep Pitch fixed
    let currentPitch = state.Pitch

    // Determine camera offset based on Yaw/Pitch
    let rotation = Matrix.CreateFromYawPitchRoll(nextYaw, -currentPitch, 0.0f)
    let offset = Vector3.Transform(Vector3.Backward * state.Distance, rotation)

    let desiredPosition = targetPosition + Vector3.Up * 2.0f + offset

    // Handle Snap vs Lerp (e.g. for Respawns)
    let dist = Vector3.Distance(state.Position, desiredPosition)
    let lerpFactor = if dist > 50.0f then 1.0f else (dt * 8.0f)
    let targetLerpFactor = if dist > 50.0f then 1.0f else (dt * 10.0f)

    {
      state with
          Position = Vector3.Lerp(state.Position, desiredPosition, lerpFactor)
          Target =
            Vector3.Lerp(
              state.Target,
              targetPosition + Vector3.Up * 1.5f,
              targetLerpFactor
            )
          Yaw = nextYaw
    }

  let perspective
    (position: Vector3)
    (target: Vector3)
    (up: Vector3)
    (fov: float32)
    (aspectRatio: float32)
    (nearPlane: float32)
    (farPlane: float32)
    =

    {
      View = Matrix.CreateLookAt(position, target, up)
      Projection =
        Matrix.CreatePerspectiveFieldOfView(
          fov,
          aspectRatio,
          nearPlane,
          farPlane
        )
      Position = position
      Target = target
      Up = up
      Fov = fov
      Aspect = aspectRatio
      Near = nearPlane
      Far = farPlane
    }
