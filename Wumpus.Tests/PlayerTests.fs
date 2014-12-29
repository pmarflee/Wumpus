﻿module PlayerTests

open System
open Xunit
open FsUnit.Xunit
open Wumpus.Core.Model

type PlayerTestFixture () =
    let cave = new Cave()
    let player = new Player(cave.Rooms.[0])

    [<Fact>]
    let ``Player should be able to move to a different room if a valid exit is selected`` () =
        let result = player.Move(1, cave)
        result |> should equal MoveResult.Success
        player.Room.Number |> should equal 1

    [<Fact>]
    let ``Player should not be able to move to a different room if an invalid exit is selected`` () =
        let result = player.Move(2, cave)
        result |> should equal MoveResult.Failure
        player.Room.Number |> should equal 0

    [<Fact>]
    let ``Player should be able to sense the Wumpus if it is in an adjoining room`` () =
        let room = cave.Rooms.[player.Room.Exits.[0]]
        room.Hazards.Add(Hazard.Wumpus)
        player.Senses(cave) |> should contain Hazard.Wumpus

    [<Fact>]
    let ``Player should not be able to sense the Wumpus if it is not in an adjoining room`` () =
        let room = cave.Rooms.[2]
        room.Hazards.Add(Hazard.Wumpus)
        player.Senses(cave) |> should not' (contain Hazard.Wumpus)

    [<Fact>]
    let ``Player should be able to sense a Bat if it is in an adjoining room`` () =
        let room = cave.Rooms.[player.Room.Exits.[0]]
        room.Hazards.Add(Hazard.Bat)
        player.Senses(cave) |> should contain Hazard.Bat

    [<Fact>]
    let ``Player should not be able to sense a Bat if it is not in an adjoining room`` () =
        let room = cave.Rooms.[2]
        room.Hazards.Add(Hazard.Bat)
        player.Senses(cave) |> should not' (contain Hazard.Bat)


    [<Fact>]
    let ``Player should be able to sense a Pit if it is in an adjoining room`` () =
        let room = cave.Rooms.[player.Room.Exits.[0]]
        room.Hazards.Add(Hazard.Pit)
        player.Senses(cave) |> should contain Hazard.Pit

    [<Fact>]
    let ``Player should not be able to sense a Pit if it is not in an adjoining room`` () =
        let room = cave.Rooms.[2]
        room.Hazards.Add(Hazard.Pit)
        player.Senses(cave) |> should not' (contain Hazard.Pit)