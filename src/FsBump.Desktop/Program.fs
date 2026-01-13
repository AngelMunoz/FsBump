module FsBump.Desktop.Program

open Mibo.Elmish
open ProceduralMap
open ProceduralMap.Program

[<EntryPoint>]
let main _ =
  // Configure window here if needed, or pass config to program in Core
  let program =
    Program.create()
    |> Program.withConfig(fun (game, graphics) ->
      game.IsMouseVisible <- true
      game.Window.Title <- "FsBump Desktop"
      graphics.PreferredBackBufferWidth <- 800
      graphics.PreferredBackBufferHeight <- 600)

  use game = new ElmishGame<Model, Msg>(program)
  game.Run()
  0
