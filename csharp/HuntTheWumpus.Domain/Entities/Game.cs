using HuntTheWumpus.Domain.ValueObjects;

namespace HuntTheWumpus.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string PlayerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public GameStatus Status { get; private set; }
    public int Score { get; private set; }
    public Cave Cave { get; private set; }
    public Player Player { get; private set; }

    public Game(string playerId)
    {
        Id = Guid.NewGuid();
        PlayerId = playerId;
        CreatedAt = DateTime.UtcNow;
        Status = GameStatus.InProgress;
        Score = 0;
        Cave = new Cave();
        Player = new Player(Cave.GetRandomRoom());
    }

    // For Entity Framework
    private Game() 
    { 
        PlayerId = string.Empty;
        Cave = new Cave();
        Player = new Player(Cave.GetRandomRoom());
    }

    public GameMoveResult MovePlayer(int targetRoomNumber)
    {
        if (Status != GameStatus.InProgress)
            throw new InvalidOperationException("Game is not in progress");

        var currentRoom = Player.CurrentRoom;
        if (!currentRoom.IsConnectedTo(targetRoomNumber))
            return new GameMoveResult(false, "Room is not connected to current location", GetGameState());

        var targetRoom = Cave.GetRoom(targetRoomNumber);
        Player.MoveTo(targetRoom);
        
        var result = CheckRoomHazards(targetRoom);
        if (result.IsGameOver)
        {
            Status = result.IsVictory ? GameStatus.Victory : GameStatus.Defeat;
            CompletedAt = DateTime.UtcNow;
        }

        return new GameMoveResult(true, result.Message, GetGameState(), result.IsGameOver, result.IsVictory);
    }

    public GameMoveResult ShootArrow(int targetRoomNumber)
    {
        if (Status != GameStatus.InProgress)
            throw new InvalidOperationException("Game is not in progress");

        if (Player.ArrowCount <= 0)
            return new GameMoveResult(false, "No arrows remaining", GetGameState());

        Player.UseArrow();

        var targetRoom = Cave.GetRoom(targetRoomNumber);
        if (targetRoom.HasWumpus)
        {
            Score += 1000;
            Status = GameStatus.Victory;
            CompletedAt = DateTime.UtcNow;
            return new GameMoveResult(true, "Congratulations! You killed the Wumpus!", GetGameState(), true, true);
        }

        // Arrow missed, Wumpus might move
        if (new Random().Next(0, 4) == 0) // 25% chance
        {
            Cave.MoveWumpus();
        }

        if (Player.ArrowCount <= 0)
        {
            Status = GameStatus.Defeat;
            CompletedAt = DateTime.UtcNow;
            return new GameMoveResult(true, "You're out of arrows! Game Over.", GetGameState(), true, false);
        }

        return new GameMoveResult(true, "Arrow missed the Wumpus!", GetGameState());
    }

    private HazardCheckResult CheckRoomHazards(Room room)
    {
        if (room.HasWumpus)
        {
            return new HazardCheckResult("You encountered the Wumpus! Game Over.", true, false);
        }

        if (room.HasPit)
        {
            return new HazardCheckResult("You fell into a pit! Game Over.", true, false);
        }

        if (room.HasBats)
        {
            var newRoom = Cave.GetRandomRoom();
            Player.MoveTo(newRoom);
            return new HazardCheckResult($"Super bats grabbed you and dropped you in room {newRoom.Number}!", false, false);
        }

        return new HazardCheckResult("You moved safely.", false, false);
    }

    public GameState GetGameState()
    {
        var currentRoom = Player.CurrentRoom;
        var nearbyWarnings = new List<string>();

        foreach (var connectedRoom in currentRoom.ConnectedRooms.Select(Cave.GetRoom))
        {
            if (connectedRoom.HasWumpus)
                nearbyWarnings.Add("You smell something terrible nearby...");
            if (connectedRoom.HasPit)
                nearbyWarnings.Add("You feel a cold draft...");
            if (connectedRoom.HasBats)
                nearbyWarnings.Add("You hear rustling nearby...");
        }

        return new GameState(
            Player.CurrentRoom.Number,
            Player.CurrentRoom.ConnectedRooms.ToArray(),
            Player.ArrowCount,
            nearbyWarnings.Distinct().ToArray(),
            Status,
            Score
        );
    }

    private class HazardCheckResult
    {
        public string Message { get; }
        public bool IsGameOver { get; }
        public bool IsVictory { get; }

        public HazardCheckResult(string message, bool isGameOver, bool isVictory)
        {
            Message = message;
            IsGameOver = isGameOver;
            IsVictory = isVictory;
        }
    }
}

public enum GameStatus
{
    InProgress,
    Victory,
    Defeat
}
