namespace HuntTheWumpus.Domain.Entities;

public enum CellType
{
    Empty,
    Wumpus,
    Pit,
    Bat,
    Player,
    Gold
}

public enum Direction
{
    North,
    East,
    South,
    West
}

public class Player
{
    public int PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public class Game
{
    public int GameId { get; set; }
    public int PlayerId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public GameStatus Status { get; set; }
    public int Score { get; set; }
    public int GridSize { get; set; }
    
    // Navigation property
    public Player? Player { get; set; }
}

public enum GameStatus
{
    InProgress,
    Won,
    Lost
}

public class GameState
{
    public int GameStateId { get; set; }
    public int GameId { get; set; }
    public string StateData { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    
    // Navigation property
    public Game? Game { get; set; }
}

public class HighScore
{
    public int ScoreId { get; set; }
    public int PlayerId { get; set; }
    public int Score { get; set; }
    public DateTime GameDate { get; set; }
    
    // Navigation property
    public Player? Player { get; set; }
}
