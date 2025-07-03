class HuntTheWumpusGame {
    constructor() {
        this.gameId = null;
        this.playerName = '';
        this.gameState = null;
        
        this.initializeEventListeners();
        this.loadLeaderboard();
    }

    initializeEventListeners() {
        // New game button
        document.getElementById('newGameBtn').addEventListener('click', () => {
            this.startNewGame();
        });

        // Move button
        document.getElementById('moveBtn').addEventListener('click', () => {
            this.movePlayer();
        });

        // Shoot button
        document.getElementById('shootBtn').addEventListener('click', () => {
            this.shootArrow();
        });

        // Enter key handlers
        document.getElementById('playerName').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.startNewGame();
        });

        document.getElementById('shootRoom').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.shootArrow();
        });

        // Modal buttons
        document.getElementById('playAgainBtn').addEventListener('click', () => {
            this.hideModal();
            this.startNewGame();
        });

        document.getElementById('closeModalBtn').addEventListener('click', () => {
            this.hideModal();
        });
    }

    async startNewGame() {
        const playerNameInput = document.getElementById('playerName');
        this.playerName = playerNameInput.value.trim();
        
        if (!this.playerName) {
            alert('Please enter your name first!');
            playerNameInput.focus();
            return;
        }

        try {
            const response = await fetch('/api/game/new', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    playerId: this.playerName
                })
            });

            const result = await response.json();
            
            if (result.success) {
                this.gameId = result.gameId;
                this.gameState = result.gameState;
                this.showGameArea();
                this.updateGameDisplay();
                this.showMessage(result.message);
                this.loadPlayerHistory();
            } else {
                alert(result.message);
            }
        } catch (error) {
            console.error('Error starting new game:', error);
            alert('Failed to start new game. Please try again.');
        }
    }

    async movePlayer() {
        const moveRoom = parseInt(document.getElementById('moveRoom').value);
        
        if (!moveRoom) {
            alert('Please select a room to move to!');
            return;
        }

        try {
            const response = await fetch(`/api/game/${this.gameId}/move`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    targetRoom: moveRoom
                })
            });

            const result = await response.json();
            this.handleGameResult(result);
        } catch (error) {
            console.error('Error moving player:', error);
            alert('Failed to move. Please try again.');
        }
    }

    async shootArrow() {
        const shootRoom = parseInt(document.getElementById('shootRoom').value);
        
        if (!shootRoom || shootRoom < 1 || shootRoom > 20) {
            alert('Please enter a valid room number (1-20)!');
            return;
        }

        try {
            const response = await fetch(`/api/game/${this.gameId}/shoot`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    targetRoom: shootRoom
                })
            });

            const result = await response.json();
            this.handleGameResult(result);
            
            // Clear the shoot input
            document.getElementById('shootRoom').value = '';
        } catch (error) {
            console.error('Error shooting arrow:', error);
            alert('Failed to shoot arrow. Please try again.');
        }
    }

    handleGameResult(result) {
        if (result.success) {
            this.gameState = result.gameState;
            this.updateGameDisplay();
            this.showMessage(result.message);
            
            if (result.isGameOver) {
                this.showGameOverModal(result.isVictory, result.message);
                this.loadLeaderboard();
                this.loadPlayerHistory();
            }
        } else {
            alert(result.message);
        }
    }

    updateGameDisplay() {
        if (!this.gameState) return;

        // Update stats
        document.getElementById('currentRoom').textContent = this.gameState.currentRoom;
        document.getElementById('arrowCount').textContent = this.gameState.arrowCount;
        document.getElementById('score').textContent = this.gameState.score;

        // Update warnings
        const warningsDiv = document.getElementById('warnings');
        if (this.gameState.warnings && this.gameState.warnings.length > 0) {
            warningsDiv.innerHTML = this.gameState.warnings.map(warning => 
                `<div>⚠️ ${warning}</div>`
            ).join('');
        } else {
            warningsDiv.innerHTML = '<div>🟢 All clear nearby...</div>';
        }

        // Update connected rooms
        this.updateConnectedRooms();
        this.updateCaveVisualization();
    }

    updateConnectedRooms() {
        const connectedRoomsDiv = document.getElementById('connectedRooms');
        const moveSelect = document.getElementById('moveRoom');
        
        // Clear previous options
        moveSelect.innerHTML = '<option value="">Select room</option>';
        connectedRoomsDiv.innerHTML = '';

        if (this.gameState && this.gameState.connectedRooms) {
            this.gameState.connectedRooms.forEach(roomNumber => {
                // Add to select dropdown
                const option = document.createElement('option');
                option.value = roomNumber;
                option.textContent = `Room ${roomNumber}`;
                moveSelect.appendChild(option);

                // Add as button
                const button = document.createElement('button');
                button.className = 'room-button';
                button.textContent = roomNumber;
                button.addEventListener('click', () => {
                    moveSelect.value = roomNumber;
                    this.movePlayer();
                });
                connectedRoomsDiv.appendChild(button);
            });
        }
    }

    updateCaveVisualization() {
        const viz = document.getElementById('caveVisualization');
        viz.innerHTML = '';
        
        // Create a simple grid visualization of rooms 1-20
        for (let i = 1; i <= 20; i++) {
            const roomDiv = document.createElement('div');
            roomDiv.className = 'room';
            roomDiv.textContent = i;
            
            if (this.gameState && i === this.gameState.currentRoom) {
                roomDiv.classList.add('current');
            } else if (this.gameState && this.gameState.connectedRooms.includes(i)) {
                roomDiv.classList.add('connected');
            }
            
            viz.appendChild(roomDiv);
        }
    }

    showMessage(message) {
        const messagesDiv = document.getElementById('messages');
        const messageElement = document.createElement('div');
        messageElement.textContent = message;
        messagesDiv.innerHTML = '';
        messagesDiv.appendChild(messageElement);
    }

    showGameArea() {
        document.getElementById('gameArea').classList.remove('hidden');
    }

    showGameOverModal(isVictory, message) {
        const modal = document.getElementById('gameOverModal');
        const title = document.getElementById('gameOverTitle');
        const messageElement = document.getElementById('gameOverMessage');
        
        title.textContent = isVictory ? '🎉 Victory!' : '💀 Game Over';
        messageElement.textContent = message;
        
        modal.classList.remove('hidden');
    }

    hideModal() {
        document.getElementById('gameOverModal').classList.add('hidden');
    }

    async loadLeaderboard() {
        try {
            const response = await fetch('/api/game/leaderboard?count=10');
            const leaderboard = await response.json();
            
            const leaderboardDiv = document.getElementById('leaderboardList');
            
            if (leaderboard.length === 0) {
                leaderboardDiv.innerHTML = '<div>No scores yet!</div>';
                return;
            }
            
            leaderboardDiv.innerHTML = leaderboard.map((entry, index) => 
                `<div class="leaderboard-item">
                    <span>${index + 1}. ${entry.playerId}</span>
                    <span>${entry.score}</span>
                 </div>`
            ).join('');
        } catch (error) {
            console.error('Error loading leaderboard:', error);
            document.getElementById('leaderboardList').innerHTML = '<div>Failed to load</div>';
        }
    }

    async loadPlayerHistory() {
        if (!this.playerName) return;
        
        try {
            const response = await fetch(`/api/game/player/${encodeURIComponent(this.playerName)}/games`);
            const games = await response.json();
            
            const historyDiv = document.getElementById('gameHistory');
            
            if (games.length === 0) {
                historyDiv.innerHTML = '<div>No games played yet</div>';
                return;
            }
            
            historyDiv.innerHTML = games.slice(0, 5).map(game => 
                `<div class="history-item">
                    <span>${game.status}</span>
                    <span>${game.score}</span>
                 </div>`
            ).join('');
        } catch (error) {
            console.error('Error loading player history:', error);
            document.getElementById('gameHistory').innerHTML = '<div>Failed to load</div>';
        }
    }
}

// Initialize the game when the page loads
document.addEventListener('DOMContentLoaded', () => {
    new HuntTheWumpusGame();
});
