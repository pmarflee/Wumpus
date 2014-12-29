namespace Wumpus.Core

open System
open System.Collections.Generic

module public Model = 

    type Hazard = Wumpus | Bat | Pit

    type Room (number : int, exits : int list) =
        let hazards = new List<Hazard>()

        member this.Number = number
        member this.Exits = exits
        member this.Hazards with get() = hazards
        member this.ContainsHazard(hazard) = hazards.Contains(hazard)

    type Cave () =
        let roomData = [| [1;4;7];[0;2;9];[1;3;11];
                          [2;4;13];[0;3;5];[4;6;14];
                          [5;7;16];[0;6;8];[7;9;17];
                          [1;8;10];[9;11;18];[2;10;12];
                          [11;13;19];[3;12;14];[5;13;15];
                          [14;16;19];[6;15;17];[8;16;18];
                          [10;17;19];[12;15;18] |] 
        let rooms = roomData |> Array.mapi (fun i exits -> new Room(i, exits)) 

        member this.Rooms with get() = rooms
        member this.AddHazard roomNumber hazard = this.Rooms.[roomNumber].Hazards.Add(hazard)
        member this.MoveBat roomNumberFrom roomNumberTo =
            match this.Rooms.[roomNumberFrom].Hazards.Remove(Hazard.Bat) with
            | true -> this.Rooms.[roomNumberTo].Hazards.Add(Hazard.Bat)
            | false -> failwith "No bat present in room"
        member this.MoveWumpus roomNumberFrom =
           let roomFrom = this.Rooms.[roomNumberFrom] 
           match roomFrom.Hazards.Remove(Hazard.Wumpus) with 
           | true ->
               let roomTo = this.Rooms.[roomFrom.Exits.[(new Random()).Next(0, 2)]]
               roomTo.Hazards.Add(Hazard.Wumpus)
           | false -> failwith "No wumpus present in room"

    type MoveResult = Success | Failure

    type Player (room : Room) =

        let mutable currentRoom : Room = room
        member this.Room = currentRoom

        member this.Move(roomNumber : int, cave : Cave) =
            if currentRoom.Exits |> List.exists ((=) roomNumber) then
                currentRoom <- cave.Rooms.[roomNumber]
                MoveResult.Success
            else
                MoveResult.Failure

        member this.Senses (cave : Cave) =
            currentRoom.Exits |> List.collect (fun exit -> cave.Rooms.[exit].Hazards |> List.ofSeq)

    type GameResult = Won | Lost

    type GameState = 
        | InProgress 
        | Over of GameResult

    let init = fun () ->
        let cave = new Cave()
        let rnd = new Random()
        let getRandomRoom = fun () -> cave.Rooms.[rnd.Next(0, cave.Rooms.GetUpperBound(0))]
        let player = new Player(getRandomRoom())
        let addHazard hazard times = 
            for i = 1 to times do
                let mutable room = getRandomRoom()
                while player.Room = room do
                    room <- getRandomRoom()
                cave.AddHazard room.Number hazard
        addHazard Hazard.Wumpus 1
        addHazard Hazard.Bat 2
        addHazard Hazard.Pit 2
        cave, player

    let batRoomMoveCalculator (cave : Cave) = 
        (new Random()).Next(0, cave.Rooms.GetUpperBound(0))

    let wumpusEatCalculator = fun () ->
        if (new Random()).Next(0, 1) = 1 then
            true
        else
            false

    type Game (init : unit -> Cave * Player, batRoomMoveCalculator : Cave -> int, wumpusEatCalculator : unit -> bool) =
        let cave, player = init()
        let mutable state = GameState.InProgress

        new() = Game(init, batRoomMoveCalculator, wumpusEatCalculator)

        member this.Player = player

        member this.Cave = cave

        member this.State = state

        member this.MovePlayer roomNumber = 
            if state <> GameState.InProgress then ()
            else
                player.Move(roomNumber, cave) |> ignore
                let room = player.Room
                if room.ContainsHazard(Hazard.Pit) then
                    this.EndGame(GameResult.Lost)
                    ()
                if room.ContainsHazard(Hazard.Wumpus) then
                    let wumpusEatsPlayer = wumpusEatCalculator()
                    if wumpusEatsPlayer then
                        this.EndGame(GameResult.Lost)
                        ()
                    else
                        cave.MoveWumpus room.Number
                if room.ContainsHazard(Hazard.Bat) then
                    let newRoomNumber = batRoomMoveCalculator(cave)
                    let oldRoomNumber = room.Number
                    this.MovePlayer(newRoomNumber)
                    cave.MoveBat oldRoomNumber newRoomNumber

        member private this.EndGame(result : GameResult) =
            state <- GameState.Over(result)