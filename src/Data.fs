/// This file contains all of the static data in use by this app. In other projects, this would be a couple of JSON
/// files.

module CarWars.Data

type SpeedRow = {
    Speed : int
    Phase1 : string
    Phase2 : string
    Phase3 : string
    Phase4 : string
    Phase5 : string
    Ram : string
}
with
    static member New(speed, p1, p2, p3, p4, p5, ram) =
        {
            Speed = speed
            Phase1 = p1
            Phase2 = p2
            Phase3 = p3
            Phase4 = p4
            Phase5 = p5
            Ram = ram
        }

let speedChart =
    [
        (0, "0", "0", "0", "0", "0", "0")
        (5, "1/2", "0", "0", "0", "0", "1d – 4")
        (10, "1", "0", "0", "0", "0", "1d – 2")
        (15, "1", "0", "1/2", "0", "0", "1d – 1")
        (20, "1", "0", "1", "0", "0", "1")
        (25, "1", "0", "1", "0", "1/2", "1")
        (30, "1", "0", "1", "0", "1", "1")
        (35, "1", "1/2", "1", "0", "1", "2")
        (40, "1", "1", "1", "0", "1", "3")
        (45, "1", "1", "1", "1/2", "1", "4")
        (50, "1", "1", "1", "1", "1", "5")
        (55, "1 1/2", "1", "1", "1", "1", "6")
        (60, "2", "1", "1", "1", "1", "7")
        (65, "2", "1", "1 1/2", "1", "1", "8")
        (70, "2", "1", "2", "1", "1", "9")
        (75, "2", "1", "2", "1", "1 1/2", "10")
        (80, "2", "1", "2", "1", "2", "11")
        (85, "2", "1 1/2", "2", "1", "2", "12")
        (90, "2", "2", "2", "1", "2", "13")
        (95, "2", "2", "2", "1 1/2", "2", "14")
        (100, "2", "2", "2", "2", "2", "15")
        (105, "2 1/2", "2", "2", "2", "2", "16")
        (110, "3", "2", "2", "2", "2", "17")
        (115, "3", "2", "2 1/2", "2", "2", "18")
        (120, "3", "2", "3", "2", "2", "19")
        (125, "3", "2", "3", "2", "2 1/2", "20")
        (130, "3", "2", "3", "2", "3", "21")
        (135, "3", "2 1/2", "3", "2", "3", "22")
        (140, "3", "3", "3", "2", "3", "23")
        (145, "3", "3", "3", "2 1/2", "3", "24")
        (150, "3", "3", "3", "3", "3", "25")
        (155, "3 1/2", "3", "3", "3", "3", "26")
        (160, "4", "3", "3", "3", "3", "27")
        (165, "4", "3", "3 1/2", "3", "3", "28")
        (170, "4", "3", "4", "3", "3", "29")
        (175, "4", "3", "4", "3", "3 1/2", "30")
        (180, "4", "3", "4", "3", "4", "31")
        (185, "4", "3 1/2", "4", "3", "4", "32")
        (190, "4", "4", "4", "3", "4", "33")
        (195, "4", "4", "4", "3 1/2", "4", "34")
        (200, "4", "4", "4", "4", "4", "35")
    ]
    |> List.map SpeedRow.New

let minSpeed = (List.head speedChart).Speed
let maxSpeed =
    let rec getLast lst =
        match lst with
        | [] -> failwith "Don't call me with an empty list."
        | [last] -> last
        | _ :: tail -> getLast tail
    (getLast speedChart).Speed
let speedLookup = speedChart |> Seq.map (fun sr -> (sr.Speed, sr)) |> dict

type Weapon = {
    Name : string
    Abbreviation : string
    ToHit : int
}
with
    static member New(name, abbreviation, toHit) =
        { Name = name; Abbreviation = abbreviation; ToHit = toHit }

let weapons =
    [
        ("Autocannon", "AC", 6)
        ("Machine Gun", "MG", 7)
        ("Recoilless Rifle", "RR", 7)
        ("Vulcan Machine Gun", "VMG", 6)
        ("Anti-Tank Gun", "ATG", 8)
        ("Grenade Launcher", "GL", 7)
        ("Spike Gun", "SG", 7)
        ("Heavy Rocket", "HR", 9)
        ("Light Rocket", "LtR", 9)
        ("Medium Rocket", "MR", 9)
        ("Micromissile Launcher", "MML", 8)
        ("Mini Rocket", "MNR", 9)
        ("Multi-Fire Rocket Pod", "MFR", 9)
        ("Rocket Launcher", "RL", 8)
        ("Heavy Laser", "HL", 6)
        ("Laser", "L", 6)
        ("Light Laser", "LL", 6)
        ("Medium Laser", "ML", 6)
        ("Flamethrower", "FT", 6)
    ]
    |> List.map Weapon.New

type ControlRow = {
    Speed : string
    ControlRoll : Map<int, string>
    Modifier : string
}
with
    static member New(speed, hc7, hc6, hc5, hc4, hc3, hc2, hc1, hc0, hcn1, hcn2, hcn3, hcn4, hcn5, hcn6, modifier) =
        let controlRoll =
            [
                (7, hc7)
                (6, hc6)
                (5, hc5)
                (4, hc4)
                (3, hc3)
                (2, hc2)
                (1, hc1)
                (0, hc0)
                (-1, hcn1)
                (-2, hcn2)
                (-3, hcn3)
                (-4, hcn4)
                (-5, hcn5)
                (-6, hcn6)
            ]
            |> Map.ofList
        {
            Speed = speed
            ControlRoll = controlRoll
            Modifier = modifier
        }

let controlTable =
    [
        ("5-10", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "−3")
        ("15-20", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "3", "−2")
        ("25-30", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "4", "−1")
        ("35-40", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "3", "4", "0")
        ("45-50", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "3", "4", "5", "+1")
        ("55-60", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "3", "4", "4", "5", "+1")
        ("65-70", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "3", "4", "5", "6", "+2")
        ("75-80", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "3", "4", "5", "5", "6", "+2")
        ("85-90", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "3", "5", "5", "6", "XX", "+2")
        ("95-100", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "4", "5", "6", "6", "XX", "+3")
        ("105-110", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "3", "4", "6", "6", "XX", "XX", "+3")
        ("115-120", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "3", "5", "6", "XX", "XX", "XX", "+3")
        ("125-130", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "2", "4", "5", "6", "XX", "XX", "XX", "+4")
        ("135-140", "safe", "safe", "safe", "safe", "safe", "safe", "safe", "3", "4", "6", "XX", "XX", "XX", "XX", "+4")
        ("145-150", "safe", "safe", "safe", "safe", "safe", "safe", "2", "3", "5", "6", "XX", "XX", "XX", "XX", "+4")
        ("155-160", "safe", "safe", "safe", "safe", "safe", "safe", "2", "4", "5", "6", "XX", "XX", "XX", "XX", "+5")
        ("165-170", "safe", "safe", "safe", "safe", "safe", "safe", "3", "4", "6", "XX", "XX", "XX", "XX", "XX", "+5")
        ("175-180", "safe", "safe", "safe", "safe", "safe", "2", "3", "5", "6", "XX", "XX", "XX", "XX", "XX", "+5")
        ("185-190", "safe", "safe", "safe", "safe", "safe", "2", "4", "5", "6", "XX", "XX", "XX", "XX", "XX", "+6")
        ("195-200", "safe", "safe", "safe", "safe", "safe", "3", "4", "6", "XX", "XX", "XX", "XX", "XX", "XX", "+6")
    ]
    |> List.map ControlRow.New