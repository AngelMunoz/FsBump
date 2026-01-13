namespace FsBump.Android

open Android.App
open Android.Content.PM
open Android.OS
open Android.Views
open Microsoft.Xna.Framework
open Mibo.Elmish

open FsBump.Core
open FsBump.Core.Program

[<Activity(Label = "FsBump",
           MainLauncher = true,
           Icon = "@drawable/icon",
           AlwaysRetainTaskState = true,
           LaunchMode = LaunchMode.SingleInstance,
           ScreenOrientation = ScreenOrientation.FullUser,
           ConfigurationChanges =
             (ConfigChanges.Orientation
              ||| ConfigChanges.Keyboard
              ||| ConfigChanges.KeyboardHidden
              ||| ConfigChanges.ScreenSize))>]
type FsBumpActivity() =
  inherit AndroidGameActivity()

  override this.OnCreate(bundle: Bundle) =
    base.OnCreate(bundle)

    let program =
      Program.create()
      |> Program.withConfig(fun (game, graphics) ->
        graphics.SupportedOrientations <-
          DisplayOrientation.LandscapeLeft
          ||| DisplayOrientation.LandscapeRight
          ||| DisplayOrientation.Portrait)

    let game = new ElmishGame<Model, Msg>(program)
    let view = game.Services.GetService(typeof<View>) :?> View
    this.SetContentView(view)
    game.Run()
