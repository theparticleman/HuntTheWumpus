namespace HuntTheWumpus.Domain.Entities;

public class Cave
{
    private readonly Room[] _rooms;
    private readonly Random _random = new Random();

    public Cave()
    {
        _rooms = new Room[20];
        InitializeRooms();
        PlaceHazards();
    }

    private void InitializeRooms()
    {
        // Create dodecahedron structure (each room connects to 3 others)
        var connections = new int[][]
        {
            new int[] { 2, 5, 8 },    // Room 1
            new int[] { 1, 3, 10 },   // Room 2
            new int[] { 2, 4, 12 },   // Room 3
            new int[] { 3, 5, 14 },   // Room 4
            new int[] { 1, 4, 6 },    // Room 5
            new int[] { 5, 7, 15 },   // Room 6
            new int[] { 6, 8, 17 },   // Room 7
            new int[] { 1, 7, 9 },    // Room 8
            new int[] { 8, 10, 18 },  // Room 9
            new int[] { 2, 9, 11 },   // Room 10
            new int[] { 10, 12, 19 }, // Room 11
            new int[] { 3, 11, 13 },  // Room 12
            new int[] { 12, 14, 20 }, // Room 13
            new int[] { 4, 13, 15 },  // Room 14
            new int[] { 6, 14, 16 },  // Room 15
            new int[] { 15, 17, 20 }, // Room 16
            new int[] { 7, 16, 18 },  // Room 17
            new int[] { 9, 17, 19 },  // Room 18
            new int[] { 11, 18, 20 }, // Room 19
            new int[] { 13, 16, 19 }  // Room 20
        };

        for (int i = 0; i < 20; i++)
        {
            _rooms[i] = new Room(i + 1, connections[i]);
        }
    }

    private void PlaceHazards()
    {
        var availableRooms = Enumerable.Range(0, 20).ToList();
        
        // Place Wumpus
        var wumpusRoomIndex = availableRooms[_random.Next(availableRooms.Count)];
        _rooms[wumpusRoomIndex].SetWumpus(true);
        availableRooms.Remove(wumpusRoomIndex);

        // Place 2 pits
        for (int i = 0; i < 2; i++)
        {
            var pitRoomIndex = availableRooms[_random.Next(availableRooms.Count)];
            _rooms[pitRoomIndex].SetPit(true);
            availableRooms.Remove(pitRoomIndex);
        }

        // Place 2 bat colonies
        for (int i = 0; i < 2; i++)
        {
            var batRoomIndex = availableRooms[_random.Next(availableRooms.Count)];
            _rooms[batRoomIndex].SetBats(true);
            availableRooms.Remove(batRoomIndex);
        }
    }

    public Room GetRoom(int roomNumber)
    {
        if (roomNumber < 1 || roomNumber > 20)
            throw new ArgumentOutOfRangeException(nameof(roomNumber), "Room number must be between 1 and 20");
        
        return _rooms[roomNumber - 1];
    }

    public Room GetRandomRoom()
    {
        return _rooms[_random.Next(20)];
    }

    public void MoveWumpus()
    {
        var currentWumpusRoom = _rooms.First(r => r.HasWumpus);
        currentWumpusRoom.SetWumpus(false);

        var connectedRooms = currentWumpusRoom.ConnectedRooms
            .Select(roomNum => _rooms[roomNum - 1])
            .ToArray();
        
        var newWumpusRoom = connectedRooms[_random.Next(connectedRooms.Length)];
        newWumpusRoom.SetWumpus(true);
    }
}
