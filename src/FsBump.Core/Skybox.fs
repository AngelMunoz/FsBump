namespace FsBump.Core

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Mibo.Rendering.Graphics3D

module Skybox =

  type State = { Time: float32 }

  let init() = { Time = 0.0f }

  let update dt state = { state with Time = state.Time + dt }

  let draw
    (modelStore: IModelStore)
    (cameraPosition: Vector3)
    (effect: Effect)
    (state: State)
    =
    fun (device: GraphicsDevice) (camera: Camera) ->
      modelStore.Get "cube"
      |> Option.iter(fun model ->
        // Ensure the model uses our custom skybox effect
        for m in model.Meshes do
          for part in m.MeshParts do
            if part.Effect <> effect then
              part.Effect <- effect

        // Huge scale to ensure it's outside the playable area
        // Centered on camera so it stays "at infinity" relative to the player
        let world =
          Matrix.CreateScale(-1000.0f) * Matrix.CreateTranslation(cameraPosition)

        // Skybox view matrix: strip translation so it's always centered
        let staticView =
          Matrix.CreateWorld(Vector3.Zero, camera.View.Forward, camera.View.Up)

        effect.Parameters.["World"].SetValue(world)
        effect.Parameters.["View"].SetValue(staticView)
        effect.Parameters.["Projection"].SetValue(camera.Projection)

        device.DepthStencilState <- DepthStencilState.None
        device.RasterizerState <- RasterizerState.CullNone

        for mesh in model.Meshes do
          mesh.Draw()

        device.DepthStencilState <- DepthStencilState.Default
        device.RasterizerState <- RasterizerState.CullCounterClockwise)
