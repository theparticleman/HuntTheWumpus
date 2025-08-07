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
        
        // Reset grid styles and use absolute positioning for dodecahedron layout
        viz.style.display = 'block';
        viz.style.position = 'relative';
        viz.style.width = '600px';
        viz.style.height = '600px';
        viz.style.margin = '0 auto';
        
        const centerX = 300;
        const centerY = 300;
        
        // Create 3 concentric pentagons based on Cave.cs connectivity
        // Inner pentagon (inverted): rooms 1-5
        // Middle pentagon: rooms 6-15 (arranged as two rings of 5)
        // Outer pentagon: rooms 16-20
        
        const roomPositions = {};
        
        // Inner pentagon (1-5) - inverted (pointing down)
        const innerRadius = 60;
        const innerStartAngle = Math.PI / 2; // Start from bottom
        for (let i = 0; i < 5; i++) {
            const angle = innerStartAngle + (i * 2 * Math.PI / 5);
            const roomNum = i + 1;
            roomPositions[roomNum] = {
                x: centerX + innerRadius * Math.cos(angle),
                y: centerY + innerRadius * Math.sin(angle)
            };
        }
        
        // Middle pentagon ring 1 (14, 6, 8, 10, 12) - normal orientation
        const middleRoomOrientation1 = [14, 6, 8, 10, 12];
        const middleRadius1 = 140;
        const middleStartAngle1 = -Math.PI / 2 + Math.PI / 5; // Start from top
        for (let i = 0; i < 5; i++) {
            const angle = middleStartAngle1 + (i * 2 * Math.PI / 5);
            const roomNum = middleRoomOrientation1[i];
            roomPositions[roomNum] = {
                x: centerX + middleRadius1 * Math.cos(angle),
                y: centerY + middleRadius1 * Math.sin(angle)
            };
        }

        // Middle pentagon ring 2 (7, 9, 11, 13, 15) - offset orientation
        const middleRoomOrientation2 = [13, 15, 7, 9, 11];
        const middleRadius2 = 180;
        const middleStartAngle2 = -Math.PI / 2; // Offset by 36 degrees
        for (let i = 0; i < 5; i++) {
            const angle = middleStartAngle2 + (i * 2 * Math.PI / 5);
            const roomNum = middleRoomOrientation2[i];
            roomPositions[roomNum] = {
                x: centerX + middleRadius2 * Math.cos(angle),
                y: centerY + middleRadius2 * Math.sin(angle)
            };
        }
        
        // Outer pentagon (16-20) - normal orientation
        const outerRoomOrientation = [20, 16, 17, 18, 19];
        const outerRadius = 260;
        const outerStartAngle = -Math.PI / 2; // Start from top
        for (let i = 0; i < 5; i++) {
            const angle = outerStartAngle + (i * 2 * Math.PI / 5);
            const roomNum = outerRoomOrientation[i];
            roomPositions[roomNum] = {
                x: centerX + outerRadius * Math.cos(angle),
                y: centerY + outerRadius * Math.sin(angle)
            };
        }

        // Room connections from Cave.cs
        const connections = {
            1: [2, 5, 8], 2: [1, 3, 10], 3: [2, 4, 12], 4: [3, 5, 14], 5: [1, 4, 6],
            6: [5, 7, 15], 7: [6, 8, 17], 8: [1, 7, 9], 9: [8, 10, 18], 10: [2, 9, 11],
            11: [10, 12, 19], 12: [3, 11, 13], 13: [12, 14, 20], 14: [4, 13, 15], 15: [6, 14, 16],
            16: [15, 17, 20], 17: [7, 16, 18], 18: [9, 17, 19], 19: [11, 18, 20], 20: [13, 16, 19]
        };

        // Create SVG for connection lines
        const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        svg.style.position = 'absolute';
        svg.style.width = '600px';
        svg.style.height = '600px';
        svg.style.top = '0';
        svg.style.left = '0';
        svg.style.pointerEvents = 'none';
        svg.style.zIndex = '1';
        svg.setAttribute('viewBox', '0 0 600 600');
        
        // Draw connection lines
        const drawnConnections = new Set();
        for (let roomNum = 1; roomNum <= 20; roomNum++) {
            const pos1 = roomPositions[roomNum];
            connections[roomNum].forEach(connectedRoom => {
                const connectionKey = `${Math.min(roomNum, connectedRoom)}-${Math.max(roomNum, connectedRoom)}`;
                if (!drawnConnections.has(connectionKey)) {
                    const pos2 = roomPositions[connectedRoom];
                    const line = document.createElementNS('http://www.w3.org/2000/svg', 'line');
                    line.setAttribute('x1', pos1.x);
                    line.setAttribute('y1', pos1.y);
                    line.setAttribute('x2', pos2.x);
                    line.setAttribute('y2', pos2.y);
                    line.setAttribute('stroke', '#ccc');
                    line.setAttribute('stroke-width', '2');
                    svg.appendChild(line);
                    drawnConnections.add(connectionKey);
                }
            });
        }
        viz.appendChild(svg);
        
        // Create room circles
        for (let i = 1; i <= 20; i++) {
            const roomDiv = document.createElement('div');
            roomDiv.className = 'room dodecahedral';
            roomDiv.textContent = i;
            
            // Position the room absolutely
            const pos = roomPositions[i];
            roomDiv.style.position = 'absolute';
            roomDiv.style.left = (pos.x - 20) + 'px';
            roomDiv.style.top = (pos.y - 20) + 'px';
            roomDiv.style.width = '40px';
            roomDiv.style.height = '40px';
            roomDiv.style.borderRadius = '50%';
            roomDiv.style.display = 'flex';
            roomDiv.style.alignItems = 'center';
            roomDiv.style.justifyContent = 'center';
            roomDiv.style.fontSize = '14px';
            roomDiv.style.fontWeight = 'bold';
            roomDiv.style.border = '2px solid #333';
            roomDiv.style.backgroundColor = '#f0f0f0';
            roomDiv.style.zIndex = '2';
            
            if (this.gameState && i === this.gameState.currentRoom) {
                roomDiv.classList.add('current');
                roomDiv.style.backgroundColor = '#ff6b6b';
                roomDiv.style.color = 'white';
                roomDiv.style.border = '3px solid #ff0000';
                roomDiv.style.boxShadow = '0 0 10px rgba(255, 0, 0, 0.5)';
            } else if (this.gameState && this.gameState.connectedRooms.includes(i)) {
                roomDiv.classList.add('connected');
                roomDiv.style.backgroundColor = '#4ecdc4';
                roomDiv.style.color = 'white';
                roomDiv.style.border = '2px solid #26a69a';
                roomDiv.style.cursor = 'pointer';
                
                // Add click handler for connected rooms
                roomDiv.addEventListener('click', () => {
                    document.getElementById('moveRoom').value = i;
                    this.movePlayer();
                });
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
