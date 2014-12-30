﻿namespace Wumpus.Core

open System
open System.Collections.Generic

module public Model = 

    type Hazard = Wumpus | Bat | Pit

    type Room (number : int, exits : int list) =
        let hazards = new List<Hazard>()

        member this.Number = number
        member this.Exits = exits
        member this.HasExit roomNumber = exits |> List.exists ((=) roomNumber)
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
        member this.WumpusRoom = rooms |> Array.find (fun room -> room.ContainsHazard(Hazard.Wumpus))
        member this.MoveWumpus roomNumberTo =
           this.WumpusRoom.Hazards.Remove(Hazard.Wumpus) |> ignore
           this.Rooms.[roomNumberTo].Hazards.Add(Hazard.Wumpus)

    type Player (cave : Cave, room : Room) =

        let mutable currentRoom : Room = room
        member this.Room = currentRoom

        member this.CanMove(roomNumber : int) = currentRoom.Exits |> List.exists ((=) roomNumber)

        member this.Move(roomNumber : int) = currentRoom <- cave.Rooms.[roomNumber]

        member this.Senses = currentRoom.Exits |> List.collect (fun exit -> cave.Rooms.[exit].Hazards |> List.ofSeq)

    type GameResult = Won | Lost

    type GameState = 
        | InProgress 
        | Over of GameResult

    let init = fun () ->
        let cave = new Cave()
        let rnd = new Random()
        let getRandomRoom = fun () -> cave.Rooms.[rnd.Next(0, cave.Rooms.GetUpperBound(0))]
        let player = new Player(cave, getRandomRoom())
        let addHazard hazard times = 
            for i = 1 to times do
                let mutable room = getRandomRoom()
                while player.Room = room || room.Hazards.Count > 0 do
                    room <- getRandomRoom()
                cave.AddHazard room.Number hazard
        addHazard Hazard.Wumpus 1
        addHazard Hazard.Bat 2
        addHazard Hazard.Pit 2
        cave, player

    let calculateBatRoomMove (cave : Cave) (room : Room) = 
        let rooms = cave.Rooms.Length
        cave.Rooms.[(room.Number + (new Random()).Next(1, rooms - 1)) % rooms]

    type WumpusAction =
        | Stay
        | Move of int

    let calculateWumpusAction (cave : Cave) =
        let choice = (new Random()).Next(0, 3)
        if choice = 0 then WumpusAction.Stay
        else WumpusAction.Move(cave.WumpusRoom.Exits.[choice - 1])

    type Game (init : unit -> Cave * Player, calculateBatRoomMove : Cave -> Room -> Room, calculateWumpusAction : Cave -> WumpusAction) =
        let cave, player = init()
        let mutable state = GameState.InProgress

        new() = Game(init, calculateBatRoomMove, calculateWumpusAction)

        member this.Player = player

        member this.Cave = cave

        member this.State = state

        member this.MovePlayer roomNumber = 
            if not <| player.CanMove(roomNumber) then
                invalidArg "roomNumber" (sprintf "Can only move to rooms %s" (String.Join(",", player.Room.Exits)))

            let rec movePlayer roomNumber = 
                if state = GameState.InProgress then
                    player.Move(roomNumber)
                    let room = player.Room
                    if room.ContainsHazard(Hazard.Pit) then
                        this.EndGame(GameResult.Lost)
                    elif room.ContainsHazard(Hazard.Wumpus) then
                        match calculateWumpusAction cave with
                        | Move(newRoomNumber) -> cave.MoveWumpus newRoomNumber
                        | Stay -> this.EndGame(GameResult.Lost)
                    if state = GameState.InProgress && room.ContainsHazard(Hazard.Bat) then
                        let newRoom = calculateBatRoomMove cave room
                        movePlayer newRoom.Number
                        cave.MoveBat room.Number newRoom.Number

            movePlayer roomNumber

        member this.ShootArrow roomNumber =
            if not <| player.Room.HasExit roomNumber then
                invalidArg "roomNumber" (sprintf "Can only shoot arrow into rooms %s" (String.Join(",", player.Room.Exits))) 
            if cave.Rooms.[roomNumber].ContainsHazard(Hazard.Wumpus) then
                this.EndGame(GameResult.Won)
            else
               match calculateWumpusAction cave with
                   | Move(newRoomNumber) -> 
                        cave.MoveWumpus newRoomNumber
                        if player.Room = cave.WumpusRoom then 
                            this.EndGame(GameResult.Lost)
                   | Stay -> ()

        member private this.EndGame(result : GameResult) =
            state <- GameState.Over(result)