namespace FsBump.Core.Audio

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Media
open Mibo.Elmish
open FsBump.Core


module Audio =

  let create(ctx: GameContext) =
    let ambient =
      Assets.fromCustom
        "Audio/pizzadoggy/Floating Dream"
        (fun path -> ctx.Content.Load<Song>(path))
        ctx

    let jump = Assets.sound "Audio/Kenney/forceField_000" ctx
    let jumpInstance = jump.CreateInstance()


    { new IAudioProvider with
        member this.Play(arg1: AudioId) : unit =
          match arg1 with
          | AmbientMusic ->
            MediaPlayer.Play ambient
            MediaPlayer.IsRepeating <- true
          | JumpSound ->
            if jumpInstance.State = SoundState.Playing then
              jumpInstance.Stop()
              jumpInstance.Play()
            else
              jumpInstance.Play()
    }
