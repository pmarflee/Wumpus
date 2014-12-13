module CaveTests

open System
open Xunit
open FsUnit.Xunit
open Wumpus.Core.Model

[<Fact>]
let ``All room exits should be bidirectional`` () =
    for room in Cave.Rooms do
        for exit in room.Exits do
            Cave.Rooms.[exit].Exits |> should contain room.Number

[<Fact>]
let ``All rooms should have 3 exits`` () =
    for room in Cave.Rooms do
        room.Exits.Length |> should equal 3
