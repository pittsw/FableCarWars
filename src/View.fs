/// View functions. Since this is a client-side framework, these views return new virtual DOMs, which are then applied.

module CarWars.View

open System

open Elmish
open Fable.Core.JsInterop
open Fable.Helpers.React.Props
open Fable.Import.Browser

open CarWars.Model

module R = Fable.Helpers.React
module P = Fable.Helpers.React.Props

module private Helpers =
    let h2 text = R.h2 [] [R.str text]
    let h3 text = R.h3 [] [R.str text]
    let separatorRow colSpan = R.tr [] [R.td [P.ColSpan colSpan] [R.hr []]]
    let thStr text = R.th [] [R.str text]
    let tdStr text = R.td [] [R.str text]

    let makeStringInput title updateFunc dispatch gameState =
        R.tr [] [
            tdStr title
            R.td [] [R.input [P.OnInput (fun e ->
                let content : string = unbox e.nativeEvent.srcElement?value
                updateFunc content gameState.ShotCalculator
                |> UpdateShotCalc
                |> dispatch)]]
        ]

    let makeArcSpinner title updateFunc dispatch gameState =
        R.tr [] [
            tdStr title
            R.td [] [
                R.select
                    [P.OnChange (fun e ->
                        let idx : int = unbox e.nativeEvent.srcElement?selectedIndex
                        let arc =
                            match idx with
                            | 0 -> Front
                            | 1 -> Back
                            | 2 -> Side
                            | i -> failwith <| sprintf "Could not understand arc index %d" i
                        updateFunc arc gameState.ShotCalculator
                        |> UpdateShotCalc
                        |> dispatch)]
                    [
                        R.option [] [R.str "Front arc"]
                        R.option [] [R.str "Back arc"]
                        R.option [] [R.str "Side arc"]
                    ]
            ]
        ]

    let makeCheckboxInput title updateFunc dispatch gameState =
        R.tr [] [
            tdStr title
            R.td [] [
                R.input [
                    P.Type "checkbox"
                    P.OnChange (fun e ->
                        let isChecked : bool = unbox e.nativeEvent.srcElement?``checked``
                        updateFunc isChecked gameState.ShotCalculator
                        |> UpdateShotCalc
                        |> dispatch)]
            ]
        ]

    let makeRadioInput<'T>
            (name : string)
            (values : 'T list)
            (valueFunc : 'T -> string)
            (updateFunc : 'T -> ShotCalculatorState -> ShotCalculatorState)
            dispatch
            gameState =
        R.tr [] [
            tdStr name
            R.td [] [
                for value in values do
                    yield R.input [
                        P.Type "radio"
                        P.Name name
                        P.Value <| Fable.Core.Case1(value.ToString())
                        P.OnChange (fun e ->
                            let isSelected : bool = unbox e.nativeEvent.srcElement?``checked``
                            updateFunc value gameState.ShotCalculator
                            |> UpdateShotCalc
                            |> dispatch)]
                    yield R.str <| valueFunc value
                    yield R.br []
            ]
        ]

open Helpers

let controlTableView =
    R.table [] [
        R.thead [] [
            R.tr [] [
                yield thStr "Speed"
                for hc = 7 downto -6 do
                    yield thStr <| string hc
                yield thStr "Modifier"
            ]
        ]
        R.tbody [] [
            for row in Data.controlTable ->
                R.tr [] [
                    yield R.td [P.Style [P.TextAlign "left"]] [R.str row.Speed]
                    for hc = 7 downto -6 do
                        yield R.td [P.Style [P.TextAlign "center"]] [R.str row.ControlRoll.[hc]]
                    yield R.td [P.Style [P.TextAlign "right"]] [R.str row.Modifier]
                ]
        ]
    ]

let setupView (players : SetupPlayer list) dispatch errorMsg =
    let newPlayerView =
        let newPlayerNameId = "newPlayerName"
        R.div [P.ClassName "block"; P.Style [P.Float "left"]] [
            h2 "New player"
            R.input [P.Id newPlayerNameId]
            R.button
                [P.OnClick (fun _ ->
                    let newPlayerBox = document.getElementById(newPlayerNameId)
                    let playerName = unbox <| newPlayerBox?value
                    if String.IsNullOrWhiteSpace(playerName) then
                        dispatch <| Replace (Setup (players, "Player name is empty."))
                    else if players |> List.exists (fun p -> p.Name = playerName) then
                        dispatch <| Replace (Setup (players, "Player name is already taken."))
                    else
                        newPlayerBox?value <- ""
                        dispatch <| AddPlayer playerName)]
                [R.str "Add new player"]
            R.br []
            R.button [P.OnClick (fun _ -> dispatch StartGame)] [R.str "Start the game!"]
            R.div [P.ClassName "error"] [R.str errorMsg]
        ]

    let playerViews =
        players
        |> Seq.mapi (fun idx player ->
            let inputName = sprintf "newCarName-%d" idx
            let hcName = sprintf "newCarHc-%d" idx
            R.span [P.ClassName "block"] [
                yield R.h2 [] [
                    R.str player.Name
                    R.button
                        [P.Style [P.Float "right"]; P.OnClick (fun _ -> dispatch <| RemovePlayer player.Name)]
                        [R.str <| sprintf "Remove %s" player.Name]]
                for car in player.Cars ->
                    R.div [] [
                        R.str (sprintf "%s (HC %d)" car.Name car.HandlingClass)
                        R.button
                            [P.Style [P.Float "right"]; P.OnClick (fun _ -> dispatch <| RemoveCar (car, player))]
                            [R.str "Remove"]
                        // Space things out or the removal button fucks with the input
                        R.div [P.Style [P.Height "0.5em"]] []
                    ]
                yield R.input [P.Id inputName]
                yield R.select [P.Id hcName] [
                    R.option [] [R.str "HC 1"]
                    R.option [] [R.str "HC 2"]
                    R.option [] [R.str "HC 3"]
                    R.option [] [R.str "HC 4"]
                ]
                yield R.button
                    [P.OnClick (fun _ ->
                        let carName = unbox <| document.getElementById(inputName)?value
                        if String.IsNullOrWhiteSpace(carName) then
                            let newPlayers =
                                players
                                |> List.map (fun p ->
                                    if p = player then
                                        { p with Error = "Car name is empty" }
                                    else p)
                            dispatch <| Replace (Setup (newPlayers, ""))
                        else
                            let hc = unbox <| document.getElementById(hcName)?selectedIndex
                            dispatch <| AddCar ({ Name = carName; HandlingClass = hc + 1 }, player))]
                    [R.str "Add new car"]
                yield R.div [P.ClassName "error"] [R.str player.Error]
            ])
        |> List.ofSeq
    R.div [] [
        newPlayerView
        R.div [P.Style [P.Display "flex"; P.FlexWrap "wrap"; P.AlignItems "flex-start"]] playerViews
    ]

let gameView gameState dispatch =
    let quantum = gameState.Current
    let shotCalculatorResult = ShotCalculatorState.GetValue gameState.ShotCalculator
    let finalToHit =
        shotCalculatorResult.Weapon.ToHit - (List.sumBy (fun (_, modifier) -> modifier) shotCalculatorResult.Modifiers)
    let leftBar =
        R.div [P.ClassName "block"; P.Style [P.Float "left"]] [
            R.div [P.Style [P.Display "flex"; P.FlexWrap "wrap"; P.AlignItems "flex-start"]] [
                R.button [P.OnClick (fun _ -> dispatch AdvancePhase)] [R.str "Next phase"]
                R.div [P.Style [P.FlexGrow 1.]] []
                R.button [P.OnClick (fun _ -> dispatch Undo)] [R.str "Undo"]
                R.button [P.OnClick (fun _ -> dispatch Redo)] [R.str "Redo"]
            ]
            R.table [] [
                let cols = [
                        yield R.col []
                        yield R.col []
                        yield R.col []
                        for i in 1..(quantum.Phase - 1) ->
                            R.col []
                        yield R.col [P.ClassName "currentPhase"]
                        for i in (quantum.Phase + 1)..5 ->
                            R.col []
                        yield R.col []
                    ]
                let numCols = List.length cols |> float

                yield R.colgroup [] cols
                yield R.tbody [] [
                    for player in quantum.Players do
                        yield R.tr [] [R.td [P.ColSpan numCols] [h2 player.Name]]
                        yield R.tr [] [
                            thStr "Car"
                            thStr "Speed"
                            thStr "HC"
                            thStr "Phase 1"
                            thStr "Phase 2"
                            thStr "Phase 3"
                            thStr "Phase 4"
                            thStr "Phase 5"
                            thStr "Speed Change"
                        ]
                        for car in player.ActiveCars do
                            yield R.tr [] [
                                tdStr car.Setup.Name
                                tdStr <| sprintf "%d" car.Speed
                                tdStr <| sprintf "%d (%d)" car.CurrentHandlingClass car.Setup.HandlingClass
                                tdStr Data.speedLookup.[car.Speed].Phase1
                                tdStr Data.speedLookup.[car.Speed].Phase2
                                tdStr Data.speedLookup.[car.Speed].Phase3
                                tdStr Data.speedLookup.[car.Speed].Phase4
                                tdStr Data.speedLookup.[car.Speed].Phase5
                                tdStr (car.Speed - car.StartingSpeed |> string)
                            ]
                            yield R.tr [] [
                                R.td [P.ColSpan numCols] [
                                    R.button
                                        [P.OnClick (fun _ ->
                                            dispatch <| Accelerate (player, car))]
                                        [R.str "Accelerate"]
                                    R.button
                                        [P.OnClick (fun _ ->
                                            dispatch <| Decelerate (player, car))]
                                        [R.str "Decelerate"]
                                    R.button
                                        [P.OnClick (fun _ ->
                                            dispatch <| LowerHc (player, car))]
                                        [R.str "Lower HC"]
                                ]
                            ]
                ]
            ]
            R.div [] [
                h2 "Shooting Calculator"
                R.table [] [
                    R.tbody [] [
                        yield R.tr [] [
                            tdStr "Weapon"
                            R.td [] [
                                R.select
                                    [P.OnChange (fun e ->
                                        let idx : int = unbox e.nativeEvent.srcElement?selectedIndex
                                        UpdateShotCalc { gameState.ShotCalculator with Weapon = Data.weapons.[idx] }
                                        |> dispatch)]
                                    [
                                        for weapon in Data.weapons ->
                                            R.option [] [R.str <| sprintf "%s (%s)" weapon.Name weapon.Abbreviation]
                                    ]
                            ]
                        ]
                        yield makeStringInput
                            "Distance (inches)"
                            (fun content state ->
                                match Double.TryParse(content.Trim().Trim([|'\"'|])) with
                                | (false, _) -> state
                                | (true, dist) -> { state with Distance = dist })
                            dispatch
                            gameState
                        yield makeArcSpinner
                            "Firer is in target's..."
                            (fun arc state -> { state with FirerInTargets = arc })
                            dispatch
                            gameState
                        yield makeArcSpinner
                            "Target is in firer's..."
                            (fun arc state -> { state with TargetInFirers = arc })
                            dispatch
                            gameState
                        yield makeStringInput
                            "Firer speed"
                            (fun content state ->
                                match Double.TryParse(content.Trim()) with
                                | (false, _) -> state
                                | (true, speed) -> { state with FirerSpeed = speed })
                            dispatch
                            gameState
                        yield makeStringInput
                            "Target speed"
                            (fun content state ->
                                match Double.TryParse(content.Trim()) with
                                | (false, _) -> state
                                | (true, speed) -> { state with TargetSpeed = speed })
                            dispatch
                            gameState
                        yield R.tr [] [
                            tdStr "Firing at..."
                            R.td [] [
                                R.select
                                    [P.OnChange (fun e ->
                                        let idx : int = unbox e.nativeEvent.srcElement?selectedIndex
                                        let target = Model.Target.AllTargets.[idx]
                                        UpdateShotCalc { gameState.ShotCalculator with TargetType = target }
                                        |> dispatch)]
                                    [
                                        for target in Model.Target.AllTargets ->
                                            R.option [] [R.str <| Model.Target.DisplayName target]
                                    ]
                            ]
                        ]
                        yield makeStringInput
                            "Firing through smoke (inches)"
                            (fun content state ->
                                match Double.TryParse(content.Trim()) with
                                | (false, _) -> state
                                | (true, dist) -> { state with SmokeDistance = dist })
                            dispatch
                            gameState
                        yield separatorRow 2.
                        yield makeRadioInput
                            "Rain"
                            [Model.NoRain; Model.Rain; Model.HeavyRain]
                            (fun value ->
                                match value with
                                | Model.NoRain -> "No rain"
                                | Model.Rain -> "Rain"
                                | Model.HeavyRain -> "Heavy rain/fog/night")
                            (fun value state -> { state with Rain = value })
                            dispatch
                            gameState
                        yield separatorRow 2.
                        yield makeRadioInput
                            "Targeting computer"
                            [Model.NoComputer; Model.Standard; Model.HiRes; Model.Cyberlink]
                            (fun value ->
                                match value with
                                | Model.NoComputer -> "No computer"
                                | Model.Standard -> "Targeting computer"
                                | Model.HiRes -> "Hi-Res targeting computer"
                                | Model.Cyberlink -> "Cyberlink")
                            (fun value state -> { state with Computer = value })
                            dispatch
                            gameState
                        yield separatorRow 2.
                        yield makeCheckboxInput
                            "Blinded by searchlight"
                            (fun isChecked state ->
                                { state with BlindedBySearchlight = isChecked })
                            dispatch
                            gameState
                        yield makeCheckboxInput
                            "Firing on oil/gravel/bad road"
                            (fun isChecked state ->
                                { state with FiringOnBadRoad = isChecked })
                            dispatch
                            gameState
                        yield separatorRow 2.
                        yield makeRadioInput
                            "Sustained fire"
                            [Model.FirstShot; Model.SecondShot; Model.ThirdShot]
                            (fun value ->
                                match value with
                                | Model.FirstShot -> "First shot"
                                | Model.SecondShot -> "Second consecutive shot"
                                | Model.ThirdShot -> "Third+ consecutive shot")
                            (fun value state -> { state with SustainedFire = value })
                            dispatch
                            gameState
                        yield separatorRow 2.
                        yield makeRadioInput
                            "Firer skidding"
                            [Model.NoSkid; Model.BadSkid; Model.WorseSkid]
                            (fun value ->
                                match value with
                                | Model.NoSkid -> "Not skidding"
                                | Model.BadSkid -> "Trivial Skid or Minor Fishtail"
                                | Model.WorseSkid -> "Minor or Moderate Skid or Major Fishtail")
                            (fun value state -> { state with Skidding = value })
                            dispatch
                            gameState
                        yield separatorRow 2.
                        yield makeCheckboxInput
                            "Firer not in arc of fire on target side"
                            (fun isChecked state ->
                                { state with FirerInWrongArc = isChecked })
                            dispatch
                            gameState

                        yield R.tr [] [
                            R.td [] [h3 "Total"]
                            R.td [] [h3 <| sprintf "%d" finalToHit]
                        ]
                        for (description, modifier) in shotCalculatorResult.Modifiers ->
                            R.tr [P.ClassName "shotBreakdown"] [
                                tdStr description
                                tdStr <| string modifier
                            ]
                    ]
                ]
            ]
            R.div [] [
                h2 "Notes"
                R.textarea [P.Style [P.Width "100%"]; P.Rows 10.] []
            ]
        ]
    let carsBySpeed =
        quantum.Players
        |> Seq.collect (fun p -> p.ActiveCars)
        |> Seq.groupBy (fun car -> car.Speed)
        |> Seq.map (fun (speed, cars) -> (speed, List.ofSeq cars))
        |> Map.ofSeq
    let rightBar =
        R.div [P.ClassName "block"; P.Style [P.Float "left"]] [
            h2 "Speed chart"
            R.table [] [
                R.colgroup [] [
                    yield R.col []
                    yield R.col []
                    for i in 1..(quantum.Phase - 1) ->
                        R.col []
                    yield R.col [P.ClassName "currentPhase"]
                    for i in (quantum.Phase + 1)..5 ->
                        R.col []
                    yield R.col []
                ]
                R.thead [] [
                    R.tr [] [
                        yield thStr "Car"
                        yield thStr "Speed"
                        for i in 1..5 ->
                            thStr <| sprintf "Phase %d" i
                        yield thStr "Ram"
                    ]
                ]
                R.tbody [] [
                    for speedRow in Data.speedChart do
                        let cars =
                            match Map.tryFind speedRow.Speed carsBySpeed with
                            | None -> []
                            | Some [] -> []
                            | Some [car] -> [R.str car.Setup.Name]
                            | Some (car :: cars) ->
                                [
                                    yield R.str car.Setup.Name
                                    for c in cars do
                                        yield R.br []
                                        yield R.str c.Setup.Name
                                ]
                        yield R.tr [] [
                            R.td [] cars
                            tdStr <| sprintf "%d" speedRow.Speed
                            tdStr speedRow.Phase1
                            tdStr speedRow.Phase2
                            tdStr speedRow.Phase3
                            tdStr speedRow.Phase4
                            tdStr speedRow.Phase5
                            tdStr speedRow.Ram
                        ]
                ]
            ]

            h2 "Control Table"
            controlTableView
        ]
    R.div [] [leftBar; rightBar]

let view model dispatch =
    let contents =
        match model with
        | Setup (players, error) -> setupView players dispatch error
        | ActiveGame gameState -> gameView gameState dispatch
    R.div [] [
        R.h1 [] [R.str "Car Wars"]
        contents
        R.footer [P.ClassName "footer"] [R.str "Contact wipi@robo.church for questions/bug reports"]
    ]
