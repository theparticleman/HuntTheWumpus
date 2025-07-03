using Xunit;
using HuntTheWumpus.Domain.Entities;

namespace HuntTheWumpus.UnitTests.Domain.Entities;

public class GameTests
{
    [Fact]
    public void Game_Constructor_ShouldInitializeCorrectly()
    {
        // Arrange
        var playerId = "TestPlayer";

        // Act
        var game = new Game(playerId);

        // Assert
        Assert.Equal(playerId, game.PlayerId);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(0, game.Score);
        Assert.NotNull(game.Cave);
        Assert.NotNull(game.Player);
        Assert.True(game.CreatedAt <= DateTime.UtcNow);
        Assert.Null(game.CompletedAt);
    }

    [Fact]
    public void Game_MovePlayer_WhenGameNotInProgress_ShouldThrowException()
    {
        // Arrange
        var game = new Game("TestPlayer");
        // Force game to end state by reflection
        typeof(Game).GetProperty("Status")?.SetValue(game, GameStatus.Victory);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => game.MovePlayer(1));
    }

    [Fact]
    public void Game_MovePlayer_ToUnconnectedRoom_ShouldReturnFailure()
    {
        // Arrange
        var game = new Game("TestPlayer");
        var currentRoom = game.Player.CurrentRoom.Number;
        var connectedRooms = game.Player.CurrentRoom.ConnectedRooms;
        
        // Find a room that's not connected
        var unconnectedRoom = 1;
        while (connectedRooms.Contains(unconnectedRoom) || unconnectedRoom == currentRoom)
        {
            unconnectedRoom++;
            if (unconnectedRoom > 20) unconnectedRoom = 1;
        }

        // Act
        var result = game.MovePlayer(unconnectedRoom);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not connected", result.Message);
    }

    [Fact]
    public void Game_MovePlayer_ToConnectedRoom_ShouldSucceed()
    {
        // Arrange
        var game = new Game("TestPlayer");
        var connectedRoom = game.Player.CurrentRoom.ConnectedRooms.First();

        // Act
        var result = game.MovePlayer(connectedRoom);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(connectedRoom, game.Player.CurrentRoom.Number);
    }

    [Fact]
    public void Game_ShootArrow_WhenGameNotInProgress_ShouldThrowException()
    {
        // Arrange
        var game = new Game("TestPlayer");
        typeof(Game).GetProperty("Status")?.SetValue(game, GameStatus.Defeat);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => game.ShootArrow(1));
    }

    [Fact]
    public void Game_ShootArrow_WhenNoArrowsLeft_ShouldReturnFailure()
    {
        // Arrange
        var game = new Game("TestPlayer");
        // Use all arrows
        for (int i = 0; i < 5; i++)
        {
            game.Player.UseArrow();
        }

        // Act
        var result = game.ShootArrow(1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No arrows", result.Message);
    }

    [Fact]
    public void Game_GetGameState_ShouldReturnCorrectState()
    {
        // Arrange
        var game = new Game("TestPlayer");

        // Act
        var gameState = game.GetGameState();

        // Assert
        Assert.Equal(game.Player.CurrentRoom.Number, gameState.CurrentRoom);
        Assert.Equal(game.Player.ArrowCount, gameState.ArrowCount);
        Assert.Equal(game.Score, gameState.Score);
        Assert.Equal(game.Status, gameState.Status);
        Assert.NotNull(gameState.ConnectedRooms);
        Assert.NotNull(gameState.Warnings);
    }
}
