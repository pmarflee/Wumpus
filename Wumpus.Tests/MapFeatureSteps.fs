module MapFeatureSteps

open System
open Xunit
open TickSpec
open FsUnit.Xunit
open Wumpus.Core

let mutable Number : int = 0

let [<Given>] ``the standard Wumpus map`` () = ()

let [<When>] ``I am in room (.*)`` (number : int) =
    Number <- number

let [<Then>] ``I should see exits (.*)`` (exits : string) =
    let expected =exits.Split(',') |> Array.map (fun n -> Int32.Parse(n)) |> Array.toList 
    Model.Rooms.GetRoom(Number).Exits |> should equal expected