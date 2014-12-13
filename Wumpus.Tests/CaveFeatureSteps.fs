module MapFeatureSteps

open System
open Xunit
open TickSpec
open FsUnit.Xunit
open Wumpus.Core.Model

let mutable Number : int = 0

let parseExits (exits : string) = exits.Split(',') |> Array.map (fun n -> Int32.Parse(n)) |> Array.toList 

let [<Given>] ``the standard Wumpus cave system`` () = ()

let [<When>] ``I am in room (.*)`` (number : int) =
    Number <- number

let [<Then>] ``I should see exits (.*)`` (exits : string) =
    let expected = parseExits exits
    Cave.Rooms.[Number].Exits |> should equal expected

let [<Then>] ``I should be able to enter and return from all exits`` () =
    for exit in Cave.Rooms.[Number].Exits do
        Cave.Rooms.[exit].Exits |> should contain Number