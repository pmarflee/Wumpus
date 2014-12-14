module PlayerTests

open System
open Xunit
open FsUnit.Xunit
open Wumpus.Core.Model

[<Fact>]
let ``Player should be able to move to a different room if a valid exit is selected`` () =
    let player = new Player(Cave)
    let result = player.Move(1)
    result |> should equal MoveResult.Success
    player.Room.Number |> should equal 1

[<Fact>]
let ``Player should not be able to move to a different room if an invalid exit is selected`` () =
    let player = new Player(Cave)
    let result = player.Move(2)
    result |> should equal MoveResult.Failure
    player.Room.Number |> should equal 0