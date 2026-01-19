namespace FsBump.Core.Audio

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Media
open Mibo.Elmish
open FsBump.Core


module Audio =

  let create(ctx: GameContext) =
    let ambient = Assets.sound "Audio/pizzadoggy/Floating Dream" ctx

    let jump = Assets.sound "Audio/Kenney/forceField_000" ctx
    let ambientinstance = ambient.CreateInstance()
    let jumpinstance = jump.CreateInstance()


    { new IAudioProvider with
        member this.Play(arg1: AudioId) : unit =
          match arg1 with
          | AmbientMusic ->
            if ambientinstance.State = SoundState.Playing then
              ()
            else
              ambientinstance.IsLooped <- true
              ambientinstance.Play()
          | JumpSound ->
            if jumpinstance.State = SoundState.Playing then
              jumpinstance.Stop()
              jumpinstance.Play()
            else
              jumpinstance.Play()
    }
