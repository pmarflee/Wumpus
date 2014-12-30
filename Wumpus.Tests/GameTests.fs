module GameTests

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

    let initWithHazard roomNumber hazard = fun () ->
        let cave = new Cave()
        cave.AddHazard roomNumber hazard
        let player = new Player(cave, cave.Rooms.[0])
        cave, player

    let initWithNoHazards = fun () ->
        let cave = new Cave()
        cave, new Player(cave, cave.Rooms.[0])

    [<Fact>]
    let ``Player should be able to move to a different room if a valid exit is selected`` () =
        let game = new Game(initWithNoHazards, batRoomMoveCalculator, wumpusEatCalculator)
        game.MovePlayer(1) |> should not' (throw typeof<ArgumentException>)
        game.Player.Room.Number |> should equal 1

    [<Fact>]
    let ``Player should not be able to move to a different room if an invalid exit is selected`` () =
        let game = new Game(initWithNoHazards, batRoomMoveCalculator, wumpusEatCalculator)
        Assert.Throws<ArgumentException>(fun () -> game.MovePlayer(2)) |> ignore
        game.Player.Room.Number |> should equal 0
 
    [<Fact>]
    let ``Player should lose game if they enter a room containing a pit`` () =
        let game = new Game(initWithHazard 1 Hazard.Pit, batRoomMoveCalculator, wumpusEatCalculator)
        game.MovePlayer(1)
        game.State |> should equal (GameState.Over(GameResult.Lost))

    [<Fact>]
    let ``Player and bat should be carried into another room if they enter a room containing a bat`` () =
        let batRoomMoveCalculator = fun (cave : Cave) room -> cave.Rooms.[2]
        let game = new Game(initWithHazard 1 Hazard.Bat, batRoomMoveCalculator, wumpusEatCalculator)
        game.MovePlayer(1)
        game.Player.Room.Number |> should equal 2
        game.Player.Room.Hazards |> should contain (Hazard.Bat)

    [<Fact>]
    let ``Player should lose game if they enter a room containing a wumpus who eats them`` () =
        let game = new Game(initWithHazard 1 Hazard.Wumpus, batRoomMoveCalculator, fun () -> true)
        game.MovePlayer(1)
        game.State |> should equal (GameState.Over(GameResult.Lost))

    [<Fact>]
    let ``Player should not lose game if they enter a room containing a wumpus who doesn't eat them`` () =
        let game = new Game(initWithHazard 1 Hazard.Wumpus, batRoomMoveCalculator, fun () -> false)
        game.MovePlayer(1)
        game.State |> should equal (GameState.InProgress)

    [<Fact>]
    let ``Wumpus should move to an adjoining room if the player enters a room containing a wumpus who doesn't eat them`` () =
        let game = new Game(initWithHazard 1 Hazard.Wumpus, batRoomMoveCalculator, fun () -> false)
        game.MovePlayer(1)
        game.Player.Senses |> should contain (Hazard.Wumpus)
