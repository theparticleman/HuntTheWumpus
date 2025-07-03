using Xunit;
using HuntTheWumpus.Domain.Entities;

namespace HuntTheWumpus.UnitTests.Domain.Entities;

public class CaveTests
{
    [Fact]
    public void Cave_Constructor_ShouldCreateCorrectNumberOfRooms()
    {
        // Arrange & Act
        var cave = new Cave();

        // Assert
        for (int i = 1; i <= 20; i++)
        {
            var room = cave.GetRoom(i);
            Assert.NotNull(room);
            Assert.Equal(i, room.Number);
        }
    }

    [Fact]
    public void Cave_GetRoom_WithInvalidRoomNumber_ShouldThrowException()
    {
        // Arrange
        var cave = new Cave();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => cave.GetRoom(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => cave.GetRoom(21));
        Assert.Throws<ArgumentOutOfRangeException>(() => cave.GetRoom(-1));
    }

    [Fact]
    public void Cave_EachRoom_ShouldHaveThreeConnections()
    {
        // Arrange
        var cave = new Cave();

        // Act & Assert
        for (int i = 1; i <= 20; i++)
        {
            var room = cave.GetRoom(i);
            Assert.Equal(3, room.ConnectedRooms.Length);
        }
    }

    [Fact]
    public void Cave_RoomConnections_ShouldBeBidirectional()
    {
        // Arrange
        var cave = new Cave();

        // Act & Assert
        for (int i = 1; i <= 20; i++)
        {
            var room = cave.GetRoom(i);
            foreach (var connectedRoomNumber in room.ConnectedRooms)
            {
                var connectedRoom = cave.GetRoom(connectedRoomNumber);
                Assert.Contains(i, connectedRoom.ConnectedRooms);
            }
        }
    }

    [Fact]
    public void Cave_ShouldHaveExactlyOneWumpus()
    {
        // Arrange
        var cave = new Cave();

        // Act
        var wumpusCount = 0;
        for (int i = 1; i <= 20; i++)
        {
            var room = cave.GetRoom(i);
            if (room.HasWumpus) wumpusCount++;
        }

        // Assert
        Assert.Equal(1, wumpusCount);
    }

    [Fact]
    public void Cave_ShouldHaveExactlyTwoPits()
    {
        // Arrange
        var cave = new Cave();

        // Act
        var pitCount = 0;
        for (int i = 1; i <= 20; i++)
        {
            var room = cave.GetRoom(i);
            if (room.HasPit) pitCount++;
        }

        // Assert
        Assert.Equal(2, pitCount);
    }

    [Fact]
    public void Cave_ShouldHaveExactlyTwoBatColonies()
    {
        // Arrange
        var cave = new Cave();

        // Act
        var batCount = 0;
        for (int i = 1; i <= 20; i++)
        {
            var room = cave.GetRoom(i);
            if (room.HasBats) batCount++;
        }

        // Assert
        Assert.Equal(2, batCount);
    }

    [Fact]
    public void Cave_GetRandomRoom_ShouldReturnValidRoom()
    {
        // Arrange
        var cave = new Cave();

        // Act
        var randomRoom = cave.GetRandomRoom();

        // Assert
        Assert.NotNull(randomRoom);
        Assert.InRange(randomRoom.Number, 1, 20);
    }
}
