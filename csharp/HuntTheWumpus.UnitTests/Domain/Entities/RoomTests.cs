using Xunit;
using HuntTheWumpus.Domain.Entities;

namespace HuntTheWumpus.UnitTests.Domain.Entities;

public class RoomTests
{
    [Fact]
    public void Room_Constructor_ShouldInitializeCorrectly()
    {
        // Arrange
        var roomNumber = 5;
        var connectedRooms = new int[] { 1, 2, 3 };

        // Act
        var room = new Room(roomNumber, connectedRooms);

        // Assert
        Assert.Equal(roomNumber, room.Number);
        Assert.Equal(connectedRooms, room.ConnectedRooms);
        Assert.False(room.HasWumpus);
        Assert.False(room.HasPit);
        Assert.False(room.HasBats);
    }

    [Fact]
    public void Room_IsConnectedTo_WhenRoomIsConnected_ShouldReturnTrue()
    {
        // Arrange
        var room = new Room(1, new int[] { 2, 3, 4 });

        // Act & Assert
        Assert.True(room.IsConnectedTo(2));
        Assert.True(room.IsConnectedTo(3));
        Assert.True(room.IsConnectedTo(4));
    }

    [Fact]
    public void Room_IsConnectedTo_WhenRoomIsNotConnected_ShouldReturnFalse()
    {
        // Arrange
        var room = new Room(1, new int[] { 2, 3, 4 });

        // Act & Assert
        Assert.False(room.IsConnectedTo(5));
        Assert.False(room.IsConnectedTo(1)); // Not connected to itself
        Assert.False(room.IsConnectedTo(10));
    }

    [Fact]
    public void Room_SetWumpus_ShouldUpdateHasWumpusProperty()
    {
        // Arrange
        var room = new Room(1, new int[] { 2, 3, 4 });

        // Act
        room.SetWumpus(true);

        // Assert
        Assert.True(room.HasWumpus);

        // Act
        room.SetWumpus(false);

        // Assert
        Assert.False(room.HasWumpus);
    }

    [Fact]
    public void Room_SetPit_ShouldUpdateHasPitProperty()
    {
        // Arrange
        var room = new Room(1, new int[] { 2, 3, 4 });

        // Act
        room.SetPit(true);

        // Assert
        Assert.True(room.HasPit);

        // Act
        room.SetPit(false);

        // Assert
        Assert.False(room.HasPit);
    }

    [Fact]
    public void Room_SetBats_ShouldUpdateHasBatsProperty()
    {
        // Arrange
        var room = new Room(1, new int[] { 2, 3, 4 });

        // Act
        room.SetBats(true);

        // Assert
        Assert.True(room.HasBats);

        // Act
        room.SetBats(false);

        // Assert
        Assert.False(room.HasBats);
    }
}
