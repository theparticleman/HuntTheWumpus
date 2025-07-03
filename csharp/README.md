# Hunt the Wumpus - Web Version

A modern web implementation of the classic "Hunt the Wumpus" game, built with C# .NET 8 backend and vanilla HTML/CSS/JavaScript frontend.

## Game Overview

Hunt the Wumpus is a classic adventure game where the player navigates through a cave system of connected rooms, hunting a monster called the Wumpus. The player must avoid hazards like bottomless pits and super bats while using limited arrows to kill the Wumpus.

## Architecture

This implementation follows Domain-Driven Design (DDD) principles:

- **HuntTheWumpus.Domain**: Contains all the core game logic and business rules
- **HuntTheWumpus.API**: ASP.NET Core Web API that hosts the frontend and provides game endpoints
- **HuntTheWumpus.UnitTests**: Comprehensive unit tests for the domain layer

## Technology Stack

### Backend
- **C# .NET 8**: Modern cross-platform runtime
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: ORM for database operations
- **MySQL**: Database for game persistence
- **Pomelo.EntityFrameworkCore.MySql**: MySQL provider for EF Core

### Frontend
- **HTML5**: Semantic markup
- **CSS3**: Modern styling with grid and flexbox
- **Vanilla JavaScript**: Game interaction logic
- **Responsive Design**: Works on desktop and mobile

### Infrastructure
- **Docker Compose**: MySQL database container
- **Domain-Driven Design**: Clean architecture patterns

## Prerequisites

- .NET 8 SDK
- Docker (for MySQL database)

## Getting Started

### 1. Start the Database

```bash
cd csharp
docker-compose up -d
```

This will start MySQL on port 3308 with the following credentials:
- **Host**: localhost:3308
- **Database**: huntthewumpus
- **Username**: wumpus_user
- **Password**: wumpus_pass

### 2. Run the Application

```bash
cd csharp
dotnet run --project HuntTheWumpus.API
```

The application will start on `https://localhost:5001` (or the port shown in the console).

### 3. Play the Game

1. Open your browser and navigate to the application URL
2. Enter your name in the player name field
3. Click "New Game" to start
4. Use the interface to move through rooms and shoot arrows
5. Try to kill the Wumpus while avoiding hazards!

## Game Rules

- **Objective**: Find and kill the Wumpus with your arrows
- **Movement**: You can only move to connected rooms
- **Arrows**: You have 5 arrows - use them wisely!
- **Hazards**:
  - **Wumpus**: Kills you if you enter its room
  - **Pits**: You fall and die if you enter
  - **Super Bats**: Transport you to a random room
- **Warnings**: You'll receive warnings about nearby hazards
- **Victory**: Kill the Wumpus with an arrow
- **Defeat**: Run out of arrows, fall in a pit, or encounter the Wumpus

## Development

### Running Tests

```bash
cd csharp
dotnet test
```

### Building the Solution

```bash
cd csharp
dotnet build
```

### Project Structure

```
csharp/
├── HuntTheWumpus.Domain/          # Core domain logic
│   ├── Entities/                  # Game entities (Game, Cave, Room, Player)
│   ├── ValueObjects/              # Value objects (GameState, GameMoveResult)
│   ├── Interfaces/                # Repository interfaces
│   └── Services/                  # Domain services
├── HuntTheWumpus.API/             # Web API and frontend hosting
│   ├── Controllers/               # API controllers
│   ├── Data/                      # Entity Framework DbContext
│   ├── Infrastructure/            # Repository implementations
│   └── wwwroot/                   # Frontend static files
├── HuntTheWumpus.UnitTests/       # Unit tests
└── docker-compose.yml             # MySQL database container
```

## API Endpoints

- `POST /api/game/new` - Create a new game
- `POST /api/game/{id}/move` - Move player to a room
- `POST /api/game/{id}/shoot` - Shoot an arrow at a room
- `GET /api/game/{id}/state` - Get current game state
- `GET /api/game/player/{playerId}/games` - Get player's game history
- `GET /api/game/leaderboard` - Get top scores

## Database Schema

The game state is persisted in MySQL with the following structure:
- **Games table**: Stores game instances with JSON columns for Cave and Player state
- **Indexes**: Optimized for player lookups and leaderboard queries

## Features

- **Persistent Game State**: Games are saved to the database
- **Player History**: Track your game statistics
- **Leaderboard**: See top scores from all players
- **Real-time Feedback**: Immediate response to player actions
- **Responsive Design**: Works on desktop and mobile devices
- **Modern UI**: Clean, dark theme with smooth animations

## Future Enhancements

- Multiplayer support
- Different cave layouts
- Sound effects
- Achievements system
- Game replay functionality

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is open source and available under the MIT License.
