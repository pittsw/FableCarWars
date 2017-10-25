module CarWars.MainApp

open Elmish
open Elmish.React
open Fable.Core.JsInterop

open CarWars
open CarWars.GameTracker.Model

type Model = {
    GameTrackerState : CarWars.GameTracker.Model.Model
}

importAll "../css/base.css"

Program.mkSimple GameTracker.App.initialModel GameTracker.App.update GameTracker.View.view
|> Program.withConsoleTrace
|> Program.withReact "app"
|> Program.run