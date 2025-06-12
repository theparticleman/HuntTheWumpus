CREATE DATABASE IF NOT EXISTS wumpus_db;
USE wumpus_db;

-- Players table
CREATE TABLE IF NOT EXISTS Players (
    PlayerId INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NULL,
    RegistrationDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLoginDate DATETIME NULL
);

-- Games table
CREATE TABLE IF NOT EXISTS Games (
    GameId INT AUTO_INCREMENT PRIMARY KEY,
    PlayerId INT NOT NULL,
    StartTime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    EndTime DATETIME NULL,
    Status ENUM('InProgress', 'Won', 'Lost') NOT NULL DEFAULT 'InProgress',
    Score INT NOT NULL DEFAULT 0,
    GridSize INT NOT NULL DEFAULT 10,
    FOREIGN KEY (PlayerId) REFERENCES Players(PlayerId)
);

-- Game states table
CREATE TABLE IF NOT EXISTS GameStates (
    GameStateId INT AUTO_INCREMENT PRIMARY KEY,
    GameId INT NOT NULL,
    StateData JSON NOT NULL,
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GameId) REFERENCES Games(GameId)
);

-- High scores table
CREATE TABLE IF NOT EXISTS HighScores (
    ScoreId INT AUTO_INCREMENT PRIMARY KEY,
    PlayerId INT NOT NULL,
    Score INT NOT NULL,
    GameDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PlayerId) REFERENCES Players(PlayerId)
);

-- Insert sample player
INSERT INTO Players (Username, Email) VALUES ('TestPlayer', 'test@example.com');
