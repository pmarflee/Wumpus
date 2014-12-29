﻿module GameTests

open System
open Xunit
open FsUnit.Xunit
open Wumpus.Core.Model

type InitTestFixture () =

    let cave, player = Wumpus.Core.Model.init()

    let getCountOfHazardsByType t = 
        cave.Rooms 
        |> Array.collect (fun room -> room.Hazards |> Array.ofSeq)
        |> Array.filter ((=) t)
        |> Array.length

    [<Fact>]
    let ``Player should be placed in a room when the game begins`` () =
        player.Room.Number |> should be (greaterThanOrEqualTo 0)
        player.Room.Number |> should be (lessThanOrEqualTo <| cave.Rooms.GetUpperBound(0))

    [<Fact>]
    let ``A single Wumpus should be placed in the cave when the game begins`` () =
        getCountOfHazardsByType Hazard.Wumpus |> should equal 1

    [<Fact>]
    let ``Bats should be placed in two rooms of the cave when the game begins`` () =
        getCountOfHazardsByType Hazard.Bat |> should equal 2

    [<Fact>]
    let ``Pits should be placed in two rooms of the cave when the game begins`` () =
        getCountOfHazardsByType Hazard.Pit |> should equal 2

    [<Fact>]
    let ``Player should not be placed in the same room as a hazard when the game begins`` () =
        player.Room.Hazards.Count |> should equal 0

type GameTestFixture () =

    [<Fact>]
    let ``Player should lose game if they enter a room containing a pit`` () =
        let init = fun () ->
            let cave = new Cave()
            cave.AddHazard 1 Hazard.Pit
            let player = new Player(cave.Rooms.[0])
            cave, player
        let game = new Game(init)
        game.MovePlayer(1) |> ignore
        game.State |> should equal (GameState.Over(GameResult.Lost))