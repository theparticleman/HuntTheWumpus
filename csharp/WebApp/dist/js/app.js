// API Base URL - uses relative path which works both in development and production
// This will use the current origin (domain) and append /api to it
const API_BASE_URL = '/api';

// Direction enum mapping to match backend enum values
const Direction = {
    North: 0,
    East: 1,
    South: 2,
    West: 3
};

// Game state
let currentPlayer = {
    id: 1,  // Default player ID
    username: 'Player'
};
let currentGame = null;

// DOM Elements
const screens = {
    menu: document.getElementById('menu'),
    game: document.getElementById('game'),
    highScores: document.getElementById('high-scores'),
    instructions: document.getElementById('instructions'),
    gameOver: document.getElementById('game-over')
};

const gameElements = {
    grid: document.getElementById('game-grid'),
    status: document.getElementById('game-status'),
    score: document.getElementById('game-score'),
    arrowStatus: document.getElementById('arrow-status'),
    goldStatus: document.getElementById('gold-status'),
    hintsList: document.getElementById('hints-list'),
    gameOverTitle: document.getElementById('game-over-title'),
    gameOverMessage: document.getElementById('game-over-message'),
    finalScore: document.getElementById('final-score')
};

// Button click event listeners
document.getElementById('new-game-btn').addEventListener('click', startNewGame);
document.getElementById('high-scores-btn').addEventListener('click', showHighScores);
document.getElementById('instructions-btn').addEventListener('click', showInstructions);
document.getElementById('back-to-menu-btn').addEventListener('click', showMenu);
document.getElementById('back-from-scores-btn').addEventListener('click', showMenu);
document.getElementById('back-from-instructions-btn').addEventListener('click', showMenu);
document.getElementById('play-again-btn').addEventListener('click', startNewGame);
document.getElementById('game-over-to-menu-btn').addEventListener('click', showMenu);

// Direction buttons
document.getElementById('north-btn').addEventListener('click', () => movePlayer('North'));
document.getElementById('east-btn').addEventListener('click', () => movePlayer('East'));
document.getElementById('south-btn').addEventListener('click', () => movePlayer('South'));
document.getElementById('west-btn').addEventListener('click', () => movePlayer('West'));

// Action buttons
document.getElementById('shoot-btn').addEventListener('click', () => {
    // When clicking shoot, prepare for direction input
    const shootDirections = ['North', 'East', 'South', 'West'];
    const directionBtns = ['north-btn', 'east-btn', 'south-btn', 'west-btn'];
    
    // Update button text to indicate shooting direction
    directionBtns.forEach((btnId, index) => {
        const btn = document.getElementById(btnId);
        const origText = btn.textContent;
        btn.textContent = `Shoot ${shootDirections[index]}`;
        
        // Create a one-time event listener for shooting
        const shootHandler = () => {
            shootArrow(shootDirections[index]);
            btn.textContent = origText;
            directionBtns.forEach((id, i) => {
                document.getElementById(id).removeEventListener('click', shootHandlers[i]);
            });
        };
        
        return shootHandler;
    });
    
    // Store the handlers to remove them later
    const shootHandlers = directionBtns.map((btnId, index) => {
        const handler = () => {
            shootArrow(shootDirections[index]);
            
            // Restore original button text
            directionBtns.forEach((id, i) => {
                document.getElementById(id).textContent = shootDirections[i];
                document.getElementById(id).removeEventListener('click', shootHandlers[i]);
            });
        };
        
        document.getElementById(btnId).addEventListener('click', handler);
        return handler;
    });
});

document.getElementById('pickup-btn').addEventListener('click', pickUpGold);

// Helper functions
function showScreen(screenId) {
    // Hide all screens
    Object.values(screens).forEach(screen => {
        screen.classList.add('hidden');
    });
    
    // Show the requested screen
    document.getElementById(screenId).classList.remove('hidden');
}

function showMenu() {
    showScreen('menu');
}

function showInstructions() {
    showScreen('instructions');
}

async function startNewGame() {
    try {
        // Create a new game via API
        const response = await fetch(`${API_BASE_URL}/games`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                playerId: currentPlayer.id,
                gridSize: 10
            })
        });
        
        if (!response.ok) {
            throw new Error('Failed to create a new game');
        }
        
        const data = await response.json();
        currentGame = {
            id: data.gameId
        };
        
        // Get initial game state
        await refreshGameState();
        
        // Show game screen
        showScreen('game');
        
    } catch (error) {
        console.error('Error starting new game:', error);
        alert('Failed to start a new game. Please try again.');
    }
}

async function refreshGameState() {
    if (!currentGame || !currentGame.id) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/games/${currentGame.id}`);
        
        if (!response.ok) {
            throw new Error('Failed to get game state');
        }
        
        const gameState = await response.json();
        updateGameDisplay(gameState);
        
    } catch (error) {
        console.error('Error refreshing game state:', error);
    }
}

// Game status enum mapping to match backend enum values
const GameStatus = {
    InProgress: 0,
    Won: 1,
    Lost: 2
};

function updateGameDisplay(gameState) {
    // Update status and score
    // Convert numeric status to readable text
    const statusText = gameState.status === GameStatus.InProgress 
        ? 'In Progress' 
        : gameState.status === GameStatus.Won 
            ? 'Won' 
            : 'Lost';
    gameElements.status.textContent = statusText;
    gameElements.score.textContent = gameState.score;
    
    // Update inventory
    gameElements.arrowStatus.textContent = gameState.hasArrow ? 'Yes' : 'No';
    gameElements.goldStatus.textContent = gameState.hasGold ? 'Yes' : 'No';
    
    // Update hints
    gameElements.hintsList.innerHTML = '';
    gameState.hints.forEach(hint => {
        const li = document.createElement('li');
        li.textContent = hint;
        gameElements.hintsList.appendChild(li);
    });
    
    // Check if game is over
    if (gameState.status !== GameStatus.InProgress) {
        handleGameEnd(gameState);
    }
}

function handleGameEnd(gameState) {
    gameElements.finalScore.textContent = gameState.score;
    
    if (gameState.status === GameStatus.Won) {
        gameElements.gameOverTitle.textContent = 'Victory!';
        gameElements.gameOverMessage.textContent = 'You escaped with the gold!';
        gameElements.gameOverTitle.style.color = 'var(--success-color)';
    } else {
        gameElements.gameOverTitle.textContent = 'Game Over';
        gameElements.gameOverMessage.textContent = 'You died!';
        gameElements.gameOverTitle.style.color = 'var(--danger-color)';
    }
    
    showScreen('game-over');
}

async function movePlayer(direction) {
    if (!currentGame || !currentGame.id) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/games/${currentGame.id}/move`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                direction: Direction[direction]
            })
        });
        
        if (!response.ok) {
            throw new Error('Failed to move player');
        }
        
        const result = await response.json();
        
        // Update game display based on result
        updateGameBasedOnActionResult(result);
        
    } catch (error) {
        console.error('Error moving player:', error);
    }
}

async function shootArrow(direction) {
    if (!currentGame || !currentGame.id) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/games/${currentGame.id}/shoot`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                direction: Direction[direction]
            })
        });
        
        if (!response.ok) {
            throw new Error('Failed to shoot arrow');
        }
        
        const result = await response.json();
        
        // Update game display based on result
        updateGameBasedOnActionResult(result);
        
    } catch (error) {
        console.error('Error shooting arrow:', error);
    }
}

async function pickUpGold() {
    if (!currentGame || !currentGame.id) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/games/${currentGame.id}/pickup`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to pick up gold');
        }
        
        const result = await response.json();
        
        // Update game display based on result
        updateGameBasedOnActionResult(result);
        
    } catch (error) {
        console.error('Error picking up gold:', error);
    }
}

function updateGameBasedOnActionResult(result) {
    // Update inventory status
    gameElements.arrowStatus.textContent = result.hasArrow ? 'Yes' : 'No';
    gameElements.goldStatus.textContent = result.hasGold ? 'Yes' : 'No';
    
    // Update hints
    if (result.hints && result.hints.length > 0) {
        gameElements.hintsList.innerHTML = '';
        result.hints.forEach(hint => {
            const li = document.createElement('li');
            li.textContent = hint;
            gameElements.hintsList.appendChild(li);
        });
    }
    
    // Show message
    if (result.message) {
        alert(result.message);
    }
    
    // Check if game ended
    if (result.gameStatus !== GameStatus.InProgress) {
        // Get final game state to display score
        refreshGameState();
    }
}

async function showHighScores() {
    try {
        const response = await fetch(`${API_BASE_URL}/players/highscores?count=10`);
        
        if (!response.ok) {
            throw new Error('Failed to fetch high scores');
        }
        
        const highScores = await response.json();
        
        // Update the high scores table
        const tableBody = document.getElementById('high-scores-body');
        tableBody.innerHTML = '';
        
        highScores.forEach((score, index) => {
            const row = document.createElement('tr');
            
            const rankCell = document.createElement('td');
            rankCell.textContent = index + 1;
            
            const playerCell = document.createElement('td');
            playerCell.textContent = score.playerName;
            
            const scoreCell = document.createElement('td');
            scoreCell.textContent = score.score;
            
            const dateCell = document.createElement('td');
            dateCell.textContent = new Date(score.date).toLocaleDateString();
            
            row.appendChild(rankCell);
            row.appendChild(playerCell);
            row.appendChild(scoreCell);
            row.appendChild(dateCell);
            
            tableBody.appendChild(row);
        });
        
        showScreen('high-scores');
        
    } catch (error) {
        console.error('Error fetching high scores:', error);
        alert('Failed to load high scores. Please try again.');
    }
}

// Initialize the app
function init() {
    showMenu();
}

// Start the app
init();
