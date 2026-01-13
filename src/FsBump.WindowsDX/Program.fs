module FsBump.WindowsDX.Program

open System
open Mibo.Elmish

open ProceduralMap
open ProceduralMap.Program

[<EntryPoint; STAThread>]
let main _ =
  let program =
    Program.create()
    |> Program.withConfig(fun (game, graphics) ->
      game.IsMouseVisible <- true
      game.Window.Title <- "FsBump Windows"
      graphics.PreferredBackBufferWidth <- 800
      graphics.PreferredBackBufferHeight <- 600)

  use game = new ElmishGame<Model, Msg>(program)
  game.Run()
  0
