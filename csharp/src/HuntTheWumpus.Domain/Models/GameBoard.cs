namespace HuntTheWumpus.Domain.Models;

using HuntTheWumpus.Domain.Entities;

public class Location
{
    public int Row { get; set; }
    public int Column { get; set; }

    public Location(int row, int column)
    {
        Row = row;
        Column = column;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is Location other)
        {
            return Row == other.Row && Column == other.Column;
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Column);
    }
}

public class GameBoard
{
    public int Size { get; }
    public CellType[,] Grid { get; }
    public Location PlayerLocation { get; private set; }
    public Location WumpusLocation { get; private set; }
    public List<Location> PitLocations { get; }
    public List<Location> BatLocations { get; }
    public Location GoldLocation { get; private set; }
    public bool HasArrow { get; set; } = true;
    public bool HasGold { get; set; } = false;
    
    private readonly Random _random = new Random();
    
    public GameBoard(int size)
    {
        Size = size;
        Grid = new CellType[size, size];
        PitLocations = new List<Location>();
        BatLocations = new List<Location>();
        
        // Initialize the board with all empty cells
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Grid[i, j] = CellType.Empty;
            }
        }
        
        // Place elements on the board
        PlaceWumpus();
        PlacePits();
        PlaceBats();
        PlaceGold();
        PlacePlayer();
    }
    
    private void PlaceWumpus()
    {
        WumpusLocation = GetRandomEmptyLocation();
        Grid[WumpusLocation.Row, WumpusLocation.Column] = CellType.Wumpus;
    }
    
    private void PlacePits()
    {
        int numPits = Size / 3; // Approximately 1/3 of the board size
        for (int i = 0; i < numPits; i++)
        {
            var location = GetRandomEmptyLocation();
            Grid[location.Row, location.Column] = CellType.Pit;
            PitLocations.Add(location);
        }
    }
    
    private void PlaceBats()
    {
        int numBats = Size / 4; // Approximately 1/4 of the board size
        for (int i = 0; i < numBats; i++)
        {
            var location = GetRandomEmptyLocation();
            Grid[location.Row, location.Column] = CellType.Bat;
            BatLocations.Add(location);
        }
    }
    
    private void PlaceGold()
    {
        GoldLocation = GetRandomEmptyLocation();
        Grid[GoldLocation.Row, GoldLocation.Column] = CellType.Gold;
    }
    
    private void PlacePlayer()
    {
        // Place player away from the Wumpus
        Location location;
        do
        {
            location = GetRandomEmptyLocation();
        } while (IsAdjacent(location, WumpusLocation));
        
        PlayerLocation = location;
        Grid[PlayerLocation.Row, PlayerLocation.Column] = CellType.Player;
    }
    
    private Location GetRandomEmptyLocation()
    {
        while (true)
        {
            int row = _random.Next(Size);
            int col = _random.Next(Size);
            
            if (Grid[row, col] == CellType.Empty)
            {
                return new Location(row, col);
            }
        }
    }
    
    public bool MovePlayer(Direction direction)
    {
        var newLocation = CalculateNewPosition(PlayerLocation, direction);
        
        if (!IsValidLocation(newLocation))
        {
            return false; // Can't move outside the board
        }
        
        // Update the grid
        Grid[PlayerLocation.Row, PlayerLocation.Column] = CellType.Empty;
        PlayerLocation = newLocation;
        
        // Check what's in the new location
        var cellType = Grid[newLocation.Row, newLocation.Column];
        
        if (cellType == CellType.Gold)
        {
            // We don't automatically pick up gold, just mark the player's location
            Grid[newLocation.Row, newLocation.Column] = CellType.Player;
            return true;
        }
        
        Grid[newLocation.Row, newLocation.Column] = CellType.Player;
        
        return cellType switch
        {
            CellType.Wumpus => false, // Player dies
            CellType.Pit => false,   // Player dies
            CellType.Bat => HandleBatEncounter(),
            _ => true
        };
    }
    
    private bool HandleBatEncounter()
    {
        // Bat carries the player to a random location
        var newLocation = GetRandomEmptyLocation();
        Grid[newLocation.Row, newLocation.Column] = CellType.Player;
        PlayerLocation = newLocation;
        
        return true;
    }
    
    public bool ShootArrow(Direction direction)
    {
        if (!HasArrow)
        {
            return false;
        }
        
        HasArrow = false;
        
        var arrowLocation = PlayerLocation;
        
        // Arrow travels 3 rooms or until it hits something
        for (int i = 0; i < 3; i++)
        {
            arrowLocation = CalculateNewPosition(arrowLocation, direction);
            
            if (!IsValidLocation(arrowLocation))
            {
                return false; // Arrow hits a wall
            }
            
            if (arrowLocation.Equals(WumpusLocation))
            {
                // Wumpus is killed
                Grid[WumpusLocation.Row, WumpusLocation.Column] = CellType.Empty;
                return true;
            }
        }
        
        return false; // Arrow missed the Wumpus
    }
    
    public List<string> GetHints()
    {
        var hints = new List<string>();
        
        // Check adjacent cells for Wumpus, Pits, and Bats
        var adjacentCells = GetAdjacentCells(PlayerLocation);
        
        foreach (var cell in adjacentCells)
        {
            if (!IsValidLocation(cell))
            {
                continue;
            }
            
            switch (Grid[cell.Row, cell.Column])
            {
                case CellType.Wumpus:
                    hints.Add("You smell a terrible stench.");
                    break;
                case CellType.Pit:
                    hints.Add("You feel a breeze.");
                    break;
                case CellType.Bat:
                    hints.Add("You hear flapping.");
                    break;
                case CellType.Gold:
                    hints.Add("You see a glimmer.");
                    break;
            }
        }
        
        return hints;
    }
    
    private Location CalculateNewPosition(Location current, Direction direction)
    {
        return direction switch
        {
            Direction.North => new Location(current.Row - 1, current.Column),
            Direction.East => new Location(current.Row, current.Column + 1),
            Direction.South => new Location(current.Row + 1, current.Column),
            Direction.West => new Location(current.Row, current.Column - 1),
            _ => current
        };
    }
    
    private bool IsValidLocation(Location location)
    {
        return location.Row >= 0 && location.Row < Size && 
               location.Column >= 0 && location.Column < Size;
    }
    
    private bool IsAdjacent(Location a, Location b)
    {
        return Math.Abs(a.Row - b.Row) <= 1 && Math.Abs(a.Column - b.Column) <= 1;
    }
    
    private List<Location> GetAdjacentCells(Location location)
    {
        return new List<Location>
        {
            new Location(location.Row - 1, location.Column),     // North
            new Location(location.Row, location.Column + 1),     // East
            new Location(location.Row + 1, location.Column),     // South
            new Location(location.Row, location.Column - 1),     // West
        };
    }
    
    public string SerializeState()
    {
        // Implement serialization logic (for persistence)
        // This would typically use JSON serialization
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            Size,
            PlayerLocation,
            WumpusLocation,
            PitLocations,
            BatLocations,
            GoldLocation,
            HasArrow,
            HasGold
        });
    }
    
    public static GameBoard? DeserializeState(string state)
    {
        try
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<GameBoardState>(state);
            if (data == null) return null;
            
            var board = new GameBoard(data.Size);
            
            // Reset the grid (clear automatically placed items)
            for (int i = 0; i < data.Size; i++)
            {
                for (int j = 0; j < data.Size; j++)
                {
                    board.Grid[i, j] = CellType.Empty;
                }
            }
            
            // Set locations
            board.PlayerLocation = data.PlayerLocation;
            board.WumpusLocation = data.WumpusLocation;
            board.GoldLocation = data.GoldLocation;
            board.PitLocations.Clear();
            board.PitLocations.AddRange(data.PitLocations);
            board.BatLocations.Clear();
            board.BatLocations.AddRange(data.BatLocations);
            
            // Set status
            board.HasArrow = data.HasArrow;
            board.HasGold = data.HasGold;
            
            // Update grid - Place items in a specific order to ensure player position takes precedence
            
            // First place other elements
            board.Grid[board.WumpusLocation.Row, board.WumpusLocation.Column] = CellType.Wumpus;
            
            // Only place gold if it hasn't been found or if player isn't on it
            if (!board.HasGold || !board.PlayerLocation.Equals(board.GoldLocation))
            {
                board.Grid[board.GoldLocation.Row, board.GoldLocation.Column] = CellType.Gold;
            }
            
            foreach (var pit in board.PitLocations)
            {
                board.Grid[pit.Row, pit.Column] = CellType.Pit;
            }
            
            foreach (var bat in board.BatLocations)
            {
                board.Grid[bat.Row, bat.Column] = CellType.Bat;
            }
            
            // Place player last to ensure they always take precedence
            board.Grid[board.PlayerLocation.Row, board.PlayerLocation.Column] = CellType.Player;
            
            return board;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    private class GameBoardState
    {
        public int Size { get; set; }
        public Location PlayerLocation { get; set; }
        public Location WumpusLocation { get; set; }
        public Location GoldLocation { get; set; }
        public List<Location> PitLocations { get; set; } = new();
        public List<Location> BatLocations { get; set; } = new();
        public bool HasArrow { get; set; }
        public bool HasGold { get; set; }
    }
}
