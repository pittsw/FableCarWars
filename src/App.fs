/// The app's logic. Surprisingly short, compared to the view and model code. Maybe that's my OO background showing.

module CarWars.App

open System

open Elmish
open Elmish.React
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser

open CarWars.Model
open CarWars.View

importAll "../css/base.css"

let activatePlayer player =
    let activeCars =
        player.Cars
        |> List.map (fun c ->
            {
                Setup = c
                Speed = 0
                CurrentHandlingClass = c.HandlingClass
                StartingSpeed = 0
            })
    {
        Name = player.Name
        ActiveCars = activeCars
        DeadCars = []
    }

let initialModel () =
    // Setup ([{ Name = "Will"; Cars = [{ Name = "Will's Murdercycle"; HandlingClass = 3 }]; Error = "" }], "")
    Setup ([], "")

let updateSetup msg (players, error) =
    let updatePlayer player updateFunc =
        players
        |> List.map (fun p ->
            if p = player then updateFunc player else p)
        |> fun ps -> Setup (ps, "")

    match msg with
    | AddPlayer name ->
        Setup ({ Name = name; Cars = []; Error = "" } :: (List.rev players) |> List.rev, "")
    | RemovePlayer name ->
        Setup (players |> List.filter (fun p -> p.Name <> name), "")
    | AddCar (car, player) ->
        updatePlayer player (fun p -> { p with Cars = car :: (List.rev p.Cars) |> List.rev; Error = "" })
    | RemoveCar (car, player) ->
        updatePlayer player (fun p -> { p with Cars = p.Cars |> List.filter (fun c -> c <> car); Error = "" })
    | StartGame ->
        match players with
        | [] -> Setup (players, "No players added.")
        | ps ->
            if List.exists (fun p -> List.isEmpty p.Cars) ps then
                Setup (players, "There are players without cars.")
            else
                ActiveGame <| GameState.New { Players = (players |> List.map activatePlayer); Phase = 1 }
    | LoadGame ->
        match GameQuantum.LoadGame() with
        | None -> Setup (players, "Could not find saved game")
        | Some quantum -> quantum |> GameState.New |> ActiveGame
    | _ -> failwith <| sprintf "Could not understand %A" msg

let updateActive msg gameState =
    let quantum = gameState.Current

    let updateCar player car newCarFunc =
        let newPlayers =
            quantum.Players
            |> List.map (fun p ->
                if p = player then
                    let newCars =
                        p.ActiveCars
                        |> List.map (fun c ->
                            if c = car then
                                newCarFunc c
                            else c)
                    { p with ActiveCars = newCars }
                else p)
        ActiveGame <| GameState.Add { quantum with Players = newPlayers } gameState

    match msg with
    | Accelerate (player, car) ->
        updateCar player car (fun c ->
            { c with Speed = Math.Min(c.Speed + 5, Data.maxSpeed) })
    | Decelerate (player, car) ->
        updateCar player car (fun c ->
            { c with Speed = Math.Max(c.Speed - 5, Data.minSpeed) })
    | LowerHc (player, car) ->
        updateCar player car (fun c ->
            { c with CurrentHandlingClass = Math.Max(c.CurrentHandlingClass - 1, -6) })
    | AdvancePhase ->
        let newPhase = if quantum.Phase = 5 then 1 else quantum.Phase + 1
        if newPhase = 1 then
            let newPlayers =
                quantum.Players
                |> List.map (fun p ->
                    let newCars =
                        p.ActiveCars
                        |> List.map (fun c ->
                            let newHandling =
                                Math.Min(
                                    c.Setup.HandlingClass,
                                    c.CurrentHandlingClass + c.Setup.HandlingClass)
                            { c with
                                StartingSpeed = c.Speed
                                CurrentHandlingClass = newHandling })
                    { p with ActiveCars = newCars })
            ActiveGame <| GameState.Add { quantum with Players = newPlayers; Phase = newPhase } gameState
        else
            ActiveGame <| GameState.Add { quantum with Phase = newPhase } gameState
    | Undo ->
        ActiveGame <| GameState.Undo gameState
    | Redo ->
        ActiveGame <| GameState.Redo gameState
    | UpdateShotCalc newCalcState ->
        ActiveGame { gameState with ShotCalculator = newCalcState }
    | _ ->
        failwith <| sprintf "Could not understand %A" msg

let update (msg : Message) model =
    let newState =
        match (msg, model) with
        | (Replace newState, _) -> newState
        | (msg, Setup (players, error)) -> updateSetup msg (players, error)
        | (msg, ActiveGame gameState) -> updateActive msg gameState
    match newState with
    | ActiveGame gameState -> GameQuantum.SaveGame gameState.Current
    | _ -> ()
    newState

Program.mkSimple initialModel update view
|> Program.withConsoleTrace
|> Program.withReact "app"
|> Program.run