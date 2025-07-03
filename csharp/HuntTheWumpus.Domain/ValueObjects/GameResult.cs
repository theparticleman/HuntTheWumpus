using HuntTheWumpus.Domain.Entities;

namespace HuntTheWumpus.Domain.ValueObjects;

public record GameState(
    int CurrentRoom,
    int[] ConnectedRooms,
    int ArrowCount,
    string[] Warnings,
    GameStatus Status,
    int Score
);

public record GameMoveResult(
    bool Success,
    string Message,
    GameState GameState,
    bool IsGameOver = false,
    bool IsVictory = false
);
