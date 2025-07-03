using Xunit;
using HuntTheWumpus.Domain.Entities;

namespace HuntTheWumpus.UnitTests.Domain.Entities;

public class PlayerTests
{
    [Fact]
    public void Player_Constructor_ShouldInitializeCorrectly()
    {
        // Arrange
        var room = new Room(5, new int[] { 1, 2, 3 });

        // Act
        var player = new Player(room);

        // Assert
        Assert.Equal(room, player.CurrentRoom);
        Assert.Equal(5, player.ArrowCount);
    }

    [Fact]
    public void Player_MoveTo_ShouldUpdateCurrentRoom()
    {
        // Arrange
        var startingRoom = new Room(1, new int[] { 2, 3, 4 });
        var targetRoom = new Room(2, new int[] { 1, 3, 5 });
        var player = new Player(startingRoom);

        // Act
        player.MoveTo(targetRoom);

        // Assert
        Assert.Equal(targetRoom, player.CurrentRoom);
        Assert.Equal(2, player.CurrentRoom.Number);
    }

    [Fact]
    public void Player_UseArrow_ShouldDecrementArrowCount()
    {
        // Arrange
        var room = new Room(1, new int[] { 2, 3, 4 });
        var player = new Player(room);
        var initialArrowCount = player.ArrowCount;

        // Act
        player.UseArrow();

        // Assert
        Assert.Equal(initialArrowCount - 1, player.ArrowCount);
    }

    [Fact]
    public void Player_UseArrow_WhenNoArrowsLeft_ShouldNotGoNegative()
    {
        // Arrange
        var room = new Room(1, new int[] { 2, 3, 4 });
        var player = new Player(room);
        
        // Use all arrows
        for (int i = 0; i < 5; i++)
        {
            player.UseArrow();
        }

        // Act
        player.UseArrow(); // Try to use one more

        // Assert
        Assert.Equal(0, player.ArrowCount); // Should not go negative
    }
}
