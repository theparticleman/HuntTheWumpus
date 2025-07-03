namespace HuntTheWumpus.Domain.Entities;

public class Room
{
    public int Number { get; private set; }
    public int[] ConnectedRooms { get; private set; }
    public bool HasWumpus { get; private set; }
    public bool HasPit { get; private set; }
    public bool HasBats { get; private set; }

    public Room(int number, int[] connectedRooms)
    {
        Number = number;
        ConnectedRooms = connectedRooms;
    }

    public bool IsConnectedTo(int roomNumber)
    {
        return ConnectedRooms.Contains(roomNumber);
    }

    public void SetWumpus(bool hasWumpus)
    {
        HasWumpus = hasWumpus;
    }

    public void SetPit(bool hasPit)
    {
        HasPit = hasPit;
    }

    public void SetBats(bool hasBats)
    {
        HasBats = hasBats;
    }
}
