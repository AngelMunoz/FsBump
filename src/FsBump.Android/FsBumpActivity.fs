namespace FsBump.Android

open Android.App
open Android.Content.PM
open Android.OS
open Android.Views
open Microsoft.Xna.Framework
open Mibo.Elmish

open FsBump.Core
open FsBump.Core.Program
open Microsoft.Xna.Framework.Graphics

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
        graphics.PreferredDepthStencilFormat <- DepthFormat.Depth24

        graphics.SupportedOrientations <-
          DisplayOrientation.LandscapeRight
          ||| DisplayOrientation.LandscapeRight)

    let game = new ElmishGame<_, _>(program)
    let view = game.Services.GetService(typeof<View>) :?> View
    this.SetContentView(view)
    game.Run()
