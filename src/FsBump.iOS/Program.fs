module FsBump.iOS.Program

open System
open Foundation
open UIKit
open Mibo.Elmish

open ProceduralMap
open ProceduralMap.Program
open Microsoft.Xna.Framework

[<Register("AppDelegate")>]
type AppDelegate() =
  inherit UIApplicationDelegate()

  override this.FinishedLaunching(app, options) =
    let program =
      Program.create()
      |> Program.withConfig(fun (game, graphics) ->
        graphics.SupportedOrientations <-
          DisplayOrientation.LandscapeLeft
          ||| DisplayOrientation.LandscapeRight
          ||| DisplayOrientation.Portrait)

    let game = new ElmishGame<Model, Msg>(program)
    game.Run()
    true

[<EntryPoint>]
let main args =
  UIApplication.Main(args, null, typeof<AppDelegate>)
  0
