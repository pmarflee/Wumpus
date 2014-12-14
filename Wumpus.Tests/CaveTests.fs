module CaveTests

open System
open Xunit
open FsUnit.Xunit
open Wumpus.Core.Model

let cave = new Cave()

[<Fact>]
let ``All room exits should be bidirectional`` () =
    for room in cave.Rooms do
        for exit in room.Exits do
            cave.Rooms.[exit].Exits |> should contain room.Number

[<Fact>]
let ``All rooms should have 3 exits`` () =
    for room in cave.Rooms do
        room.Exits.Length |> should equal 3
