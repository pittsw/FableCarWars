module CarWars.Model

open System

type SetupCar = {
    Name : string
    HandlingClass : int
}

type SetupPlayer = {
    Name : string
    Cars : SetupCar list
    Error : string
}

type GameCar = {
    Setup : SetupCar
    Speed : int
    CurrentHandlingClass : int
    StartingSpeed : int
}

type GamePlayer = {
    Name : string
    ActiveCars : GameCar list
    DeadCars : GameCar list
}

type GameQuantum = {
    Players : GamePlayer list
    Phase : int
}

type Arc =
    | Front
    | Back
    | Side

type Target =
    | CompactOrSubcompact
    | OtherCar
    | Motorcycle
    | LightTrike
    | MediumTrike
    | HeavyTrike
    | XHeavyTrike
    | Pedestrian
    | Tire
    | Turret
    | MotorcycleRider
with
    static member DisplayName target =
        match target with
        | CompactOrSubcompact -> "Compact/subcompact"
        | OtherCar -> "Other car"
        | Motorcycle -> "Motorcycle"
        | LightTrike -> "Light trike"
        | MediumTrike -> "Medium trike"
        | HeavyTrike -> "Heavy trike"
        | XHeavyTrike -> "X-heavy trike"
        | Pedestrian -> "Pedestrian"
        | Tire -> "Tire"
        | Turret -> "Turret"
        | MotorcycleRider -> "Motorcycle rider (side only)"

    static member AllTargets =
        [
            CompactOrSubcompact
            OtherCar
            Motorcycle
            LightTrike
            MediumTrike
            HeavyTrike
            XHeavyTrike
            Pedestrian
            Tire
            Turret
            MotorcycleRider
        ]

type RainStatus =
    | NoRain
    | Rain
    | HeavyRain

type TargetingComputer =
    | NoComputer
    | Standard
    | HiRes
    | Cyberlink

type SustainedFire =
    | FirstShot
    | SecondShot
    | ThirdShot

type SkidType =
    | NoSkid
    | BadSkid
    | WorseSkid

type ShotCalculatorValue = {
    Weapon : Data.Weapon
    Modifiers : (string * int) list
}

type ShotCalculatorState = {
    Weapon : Data.Weapon
    Distance : double
    FirerInTargets : Arc
    TargetInFirers : Arc
    FirerSpeed : double
    TargetSpeed : double
    TargetType : Target
    SmokeDistance : double
    Rain : RainStatus
    BlindedBySearchlight : bool
    Computer : TargetingComputer
    FiringOnBadRoad : bool
    SustainedFire : SustainedFire
    Skidding : SkidType
    FirerInWrongArc : bool
}
with
    static member Default =
        {
            Weapon = List.head Data.weapons
            Distance = 0.
            FirerInTargets = Front
            TargetInFirers = Front
            FirerSpeed = 0.
            TargetSpeed = 0.
            TargetType = CompactOrSubcompact
            SmokeDistance = 0.
            Rain = NoRain
            BlindedBySearchlight = false
            Computer = NoComputer
            FiringOnBadRoad = false
            SustainedFire = FirstShot
            Skidding = NoSkid
            FirerInWrongArc = false
        }

    static member GetValue shotCalc =
        let distanceMod =
            if shotCalc.Distance < 1. then ("Point blank", 4)
            else ("Long range", -1 * ((int shotCalc.Distance) / 4))
        let effectiveTargetSpeed =
            match (shotCalc.FirerInTargets, shotCalc.TargetInFirers) with
            | (Front, Front) -> shotCalc.TargetSpeed / 2.
            | (Front, Back) -> (shotCalc.TargetSpeed - shotCalc.FirerSpeed) / 2.
            | (Front, Side) -> shotCalc.TargetSpeed / 2.
            | (Back, Front) -> (shotCalc.TargetSpeed - shotCalc.FirerSpeed) / 2.
            | (Back, Back) -> shotCalc.TargetSpeed / 2.
            | (Back, Side) -> shotCalc.TargetSpeed / 2.
            | (Side, Front) -> shotCalc.TargetSpeed
            | (Side, Back) -> shotCalc.TargetSpeed
            | (Side, Side) -> shotCalc.TargetSpeed - shotCalc.FirerSpeed
            |> Math.Abs
        let firerSpeedMod = ("Firer is not moving", if shotCalc.FirerSpeed = 0. then 1 else 0)
        let targetSpeedMod =
            if shotCalc.TargetSpeed < 2.5 then ("Target is not moving", 1)
            else if effectiveTargetSpeed < 30. then ("Target is moving 5 to 27.5", 0)
            else if effectiveTargetSpeed < 37.5 then ("Target is moving 30 to 37.5", -1)
            else if effectiveTargetSpeed < 47.5 then ("Target is moving 40 to 47.5", -2)
            else if effectiveTargetSpeed < 57.5 then ("Target is moving 50 to 57.5", -3)
            else if effectiveTargetSpeed < 67.5 then ("Target is moving 60 to 67.5", -4)
            else if effectiveTargetSpeed < 77.5 then ("Target is moving 70 to 77.5", -5)
            else ("Target is moving 80 or faster", -6)
        let targetArcMod =
            // Heavy trikes are fucking weird, man...
            if shotCalc.TargetType = HeavyTrike then ("Target is a heavy trike", -1)
            else if shotCalc.FirerInTargets = Front
                    || shotCalc.FirerInTargets = Back then ("Firing at front/back of target", -1)
            else ("Firing at side of target", 0)
        let targetTypeMod =
            match shotCalc.TargetType with
            | CompactOrSubcompact -> -1
            | OtherCar -> 0
            | Motorcycle -> -2
            | LightTrike -> -2
            | MediumTrike -> -1
            | HeavyTrike -> 0
            | XHeavyTrike -> 0
            | Pedestrian -> -3
            | Tire -> -3
            | Turret -> -2
            | MotorcycleRider -> -3
            |> fun modifier -> (sprintf "Firing at %s" <| Target.DisplayName shotCalc.TargetType, modifier)
        let smokeMod = (sprintf "Firing through smoke", Math.Floor(shotCalc.SmokeDistance * -2.) |> int)
        let rainMod =
            match shotCalc.Rain with
            | NoRain -> ("No rain", 0)
            | Rain -> ("Rain", -2)
            | HeavyRain -> ("Heavy rain/fog/night", -3)
        let searchlightMod =
            if shotCalc.BlindedBySearchlight then ("Blinded by searchlight", -10)
            else ("Not blinded by searchlight", 0)
        let computerMod =
            match shotCalc.Computer with
            | NoComputer -> ("No computer", 0)
            | Standard -> ("Targeting computer", 1)
            | HiRes -> ("Hi-Res targeting computer", 2)
            | Cyberlink -> ("Cyberlink", 3)
        let badRoadMod = if shotCalc.FiringOnBadRoad then ("Firing on bad road", -1) else ("Firing on okay raod", 0)
        let sustainedFireMod =
            match shotCalc.SustainedFire with
            | FirstShot -> ("First consecutive shot", 0)
            | SecondShot -> ("Second consecutive shot", 1)
            | ThirdShot -> ("Third+ consecutive shot", 2)
        let skiddingMod =
            match shotCalc.Skidding with
            | NoSkid -> ("Not skidding", 0)
            | BadSkid -> ("Trivial skid/minor fishtail", -3)
            | WorseSkid -> ("Minor or moderate skid/major fishtail", -6)
        let wrongArcMod =
            if shotCalc.FirerInWrongArc then ("Firer not in arc of target side", -2) else ("No arc mod", 0)
        {
            Weapon = shotCalc.Weapon
            Modifiers =
                [
                    distanceMod
                    firerSpeedMod
                    targetSpeedMod
                    targetArcMod
                    targetTypeMod
                    smokeMod
                    rainMod
                    searchlightMod
                    computerMod
                    badRoadMod
                    sustainedFireMod
                    skiddingMod
                    wrongArcMod
                ]
                |> List.filter (fun (description, modifier) -> modifier <> 0)
        }

type GameState = {
    Current : GameQuantum
    Past : GameQuantum list
    Future : GameQuantum list
    ShotCalculator : ShotCalculatorState
}
with
    static member New state =
        {
            Current = state
            Past = []
            Future = []
            ShotCalculator = ShotCalculatorState.Default
        }

    static member Add state gameState =
        { gameState with
            Current = state
            Past = gameState.Current :: gameState.Past
            Future = []
        }

    static member Undo gameState =
        match gameState.Past with
        | [] -> gameState
        | lastState :: oldStates ->
            { gameState with
                Current = lastState
                Past = oldStates
                Future = gameState.Current :: gameState.Future
            }

    static member Redo gameState =
        match gameState.Future with
        | [] -> gameState
        | nextState :: nextStates ->
            { gameState with
                Current = nextState
                Past = gameState.Current :: gameState.Past
                Future = nextStates
            }

type Model =
    | Setup of (SetupPlayer list * string)
    | ActiveGame of GameState

type Message =
    // Universal.
    | Replace of Model

    // Setup only.
    | AddPlayer of string
    | RemovePlayer of string
    | AddCar of SetupCar * SetupPlayer
    | RemoveCar of SetupCar * SetupPlayer
    | StartGame

    // Active game only.
    | Accelerate of GamePlayer * GameCar
    | Decelerate of GamePlayer * GameCar
    | LowerHc of GamePlayer * GameCar
    | AdvancePhase
    | Undo
    | Redo
    | UpdateShotCalc of ShotCalculatorState