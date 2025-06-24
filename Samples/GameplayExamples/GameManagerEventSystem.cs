using UnityEngine;
using Framework.Events;
using Framework.Events.Extensions;
using Extensions;

namespace Samples.GameplayExamples
{
    /// <summary>
    /// Demonstrates a game manager that coordinates multiple game systems
    /// using events. Shows how to create a decoupled architecture where
    /// systems communicate through events without direct dependencies.
    /// </summary>
    public class GameManagerEventSystem : MonoBehaviour
    {
        [Header("Game Configuration")]
        [SerializeField] private float gameSessionDuration = 300f; // 5 minutes
        [SerializeField] private int scoreTarget = 1000;
        [SerializeField] private int livesCount = 3;
        
        // Game state events
        private BaseEventProducer<GameState> _gameStateEvent;
        private BaseEventProducer<ScoreData> _scoreEvent;
        private BaseEventProducer<LivesData> _livesEvent;
        private BaseEventProducer<TimeData> _timeEvent;
        private BaseEventProducer<GameResult> _gameResultEvent;
        
        // Cross-system events
        private BaseEventProducer<PlayerAction> _playerActionEvent;
        private BaseEventProducer<EnemyAction> _enemyActionEvent;
        private BaseEventProducer<GameEvent> _gameEventProducer;
        
        private DisposeBag _disposeBag = new DisposeBag();
        private float _gameStartTime;
        private int _currentScore;
        private int _currentLives;
        
        // Game data structures
        public enum GameState
        {
            MainMenu,
            Loading,
            Playing,
            Paused,
            GameOver,
            Victory
        }
        
        [System.Serializable]
        public struct ScoreData
        {
            public int currentScore;
            public int targetScore;
            public int lastScoreIncrease;
            public float progressPercent;
        }
        
        [System.Serializable]
        public struct LivesData
        {
            public int currentLives;
            public int maxLives;
            public bool gameOverOnNextDeath;
        }
        
        [System.Serializable]
        public struct TimeData
        {
            public float elapsedTime;
            public float totalTime;
            public float remainingTime;
            public float timePercent;
        }
        
        [System.Serializable]
        public struct GameResult
        {
            public bool isVictory;
            public int finalScore;
            public float timeTaken;
            public string resultMessage;
        }
        
        [System.Serializable]
        public struct PlayerAction
        {
            public PlayerActionType type;
            public Vector3 position;
            public int value;
            public string details;
        }
        
        public enum PlayerActionType
        {
            Move,
            Jump,
            Attack,
            Collect,
            UseItem,
            Die
        }
        
        [System.Serializable]
        public struct EnemyAction
        {
            public EnemyActionType type;
            public Vector3 position;
            public int damage;
            public string enemyId;
        }
        
        public enum EnemyActionType
        {
            Spawn,
            Attack,
            Move,
            Die,
            SpecialAbility
        }
        
        [System.Serializable]
        public struct GameEvent
        {
            public GameEventType type;
            public string message;
            public int value;
            public float timestamp;
        }
        
        public enum GameEventType
        {
            LevelStart,
            LevelComplete,
            PowerUpCollected,
            BossSpawned,
            BossDefeated,
            CheckpointReached,
            SecretFound
        }
        
        void Start()
        {
            InitializeGameEvents();
            SetupEventPipelines();
            StartGame();
            
            Debug.Log("Game Manager Event System initialized. Press G(start), P(pause), R(reset), K(kill player), S(add score), E(spawn enemy)");
        }
        
        private void InitializeGameEvents()
        {
            // Initialize game state events with state retention
            _gameStateEvent = new BaseEventProducer<GameState>(repeat: true);
            _scoreEvent = new BaseEventProducer<ScoreData>(repeat: true);
            _livesEvent = new BaseEventProducer<LivesData>(repeat: true);
            _timeEvent = new BaseEventProducer<TimeData>(repeat: true);
            _gameResultEvent = new BaseEventProducer<GameResult>();
            
            // Initialize action events
            _playerActionEvent = new BaseEventProducer<PlayerAction>();
            _enemyActionEvent = new BaseEventProducer<EnemyAction>();
            _gameEventProducer = new BaseEventProducer<GameEvent>();
            
            // Subscribe to events for game logic
            _gameStateEvent.listener.Subscribe(OnGameStateChanged).AddToDisposables(_disposeBag);
            _scoreEvent.listener.Subscribe(OnScoreChanged).AddToDisposables(_disposeBag);
            _livesEvent.listener.Subscribe(OnLivesChanged).AddToDisposables(_disposeBag);
            _timeEvent.listener.Subscribe(OnTimeChanged).AddToDisposables(_disposeBag);
            _playerActionEvent.listener.Subscribe(OnPlayerAction).AddToDisposables(_disposeBag);
            _enemyActionEvent.listener.Subscribe(OnEnemyAction).AddToDisposables(_disposeBag);
            _gameEventProducer.listener.Subscribe(OnGameEvent).AddToDisposables(_disposeBag);
        }
        
        private void SetupEventPipelines()
        {
            // Pipeline player actions to game events for certain actions
            _playerActionEvent.listener.Subscribe((sender, action) =>
            {
                if (action.type == PlayerActionType.Collect)
                {
                    var gameEvent = new GameEvent
                    {
                        type = GameEventType.PowerUpCollected,
                        message = $"Player collected item worth {action.value} points",
                        value = action.value,
                        timestamp = Time.time
                    };
                    _gameEventProducer.Publish(sender, gameEvent);
                }
            }).AddToDisposables(_disposeBag);
            
            // Pipeline enemy actions to score changes
            _enemyActionEvent.listener.Subscribe((sender, action) =>
            {
                if (action.type == EnemyActionType.Die)
                {
                    AddScore(50); // Base score for defeating enemy
                }
            }).AddToDisposables(_disposeBag);
        }
        
        private void StartGame()
        {
            _currentScore = 0;
            _currentLives = livesCount;
            _gameStartTime = Time.time;
            
            PublishGameState(GameState.Playing);
            PublishScoreUpdate(0);
            PublishLivesUpdate();
            
            // Publish start event
            var startEvent = new GameEvent
            {
                type = GameEventType.LevelStart,
                message = "Game session started",
                value = 0,
                timestamp = Time.time
            };
            _gameEventProducer.Publish(this, startEvent);
        }
        
        void Update()
        {
            HandleTestInput();
            UpdateGameTime();
            CheckWinConditions();
        }
        
        private void HandleTestInput()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (_gameStateEvent.state.lastState != GameState.Playing)
                {
                    StartGame();
                }
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ResetGame();
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                SimulatePlayerDeath();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                AddScore(Random.Range(10, 100));
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                SimulateEnemySpawn();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                SimulatePlayerCollectItem();
            }
        }
        
        private void UpdateGameTime()
        {
            if (_gameStateEvent.state.lastState == GameState.Playing)
            {
                float elapsedTime = Time.time - _gameStartTime;
                float remainingTime = Mathf.Max(0, gameSessionDuration - elapsedTime);
                
                var timeData = new TimeData
                {
                    elapsedTime = elapsedTime,
                    totalTime = gameSessionDuration,
                    remainingTime = remainingTime,
                    timePercent = elapsedTime / gameSessionDuration
                };
                
                _timeEvent.Publish(this, timeData);
                
                // Check for time up
                if (remainingTime <= 0)
                {
                    EndGame(false, "Time's up!");
                }
            }
        }
        
        private void CheckWinConditions()
        {
            if (_gameStateEvent.state.lastState == GameState.Playing)
            {
                // Check if player reached score target
                if (_currentScore >= scoreTarget)
                {
                    EndGame(true, "Target score reached!");
                }
            }
        }
        
        public void AddScore(int points)
        {
            if (_gameStateEvent.state.lastState != GameState.Playing) return;
            
            _currentScore += points;
            PublishScoreUpdate(points);
        }
        
        public void LoseLife()
        {
            if (_gameStateEvent.state.lastState != GameState.Playing) return;
            
            _currentLives = Mathf.Max(0, _currentLives - 1);
            PublishLivesUpdate();
            
            if (_currentLives <= 0)
            {
                EndGame(false, "No lives remaining!");
            }
        }
        
        private void TogglePause()
        {
            var currentState = _gameStateEvent.state.lastState;
            if (currentState == GameState.Playing)
            {
                PublishGameState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                PublishGameState(GameState.Playing);
            }
        }
        
        private void ResetGame()
        {
            PublishGameState(GameState.MainMenu);
            Debug.Log("[GameManager] Game reset to main menu");
        }
        
        private void EndGame(bool isVictory, string reason)
        {
            var finalState = isVictory ? GameState.Victory : GameState.GameOver;
            PublishGameState(finalState);
            
            var timeData = _timeEvent.state.lastState;
            var result = new GameResult
            {
                isVictory = isVictory,
                finalScore = _currentScore,
                timeTaken = timeData.elapsedTime,
                resultMessage = reason
            };
            
            _gameResultEvent.Publish(this, result);
            
            // Publish game end event
            var endEvent = new GameEvent
            {
                type = isVictory ? GameEventType.LevelComplete : GameEventType.LevelStart,
                message = reason,
                value = _currentScore,
                timestamp = Time.time
            };
            _gameEventProducer.Publish(this, endEvent);
        }
        
        private void SimulatePlayerDeath()
        {
            var deathAction = new PlayerAction
            {
                type = PlayerActionType.Die,
                position = Vector3.zero,
                value = 0,
                details = "Player died from enemy attack"
            };
            _playerActionEvent.Publish(this, deathAction);
        }
        
        private void SimulatePlayerCollectItem()
        {
            int itemValue = Random.Range(10, 50);
            var collectAction = new PlayerAction
            {
                type = PlayerActionType.Collect,
                position = Vector3.zero,
                value = itemValue,
                details = $"Collected power-up worth {itemValue} points"
            };
            _playerActionEvent.Publish(this, collectAction);
        }
        
        private void SimulateEnemySpawn()
        {
            var spawnAction = new EnemyAction
            {
                type = EnemyActionType.Spawn,
                position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)),
                damage = 0,
                enemyId = $"Enemy_{Random.Range(1000, 9999)}"
            };
            _enemyActionEvent.Publish(this, spawnAction);
            
            // Simulate enemy death after a short time for testing
            Invoke(nameof(SimulateEnemyDeath), Random.Range(2f, 5f));
        }
        
        private void SimulateEnemyDeath()
        {
            var deathAction = new EnemyAction
            {
                type = EnemyActionType.Die,
                position = Vector3.zero,
                damage = 0,
                enemyId = "TestEnemy"
            };
            _enemyActionEvent.Publish(this, deathAction);
        }
        
        private void PublishGameState(GameState newState)
        {
            _gameStateEvent.Publish(this, newState);
        }
        
        private void PublishScoreUpdate(int lastIncrease)
        {
            var scoreData = new ScoreData
            {
                currentScore = _currentScore,
                targetScore = scoreTarget,
                lastScoreIncrease = lastIncrease,
                progressPercent = (float)_currentScore / scoreTarget
            };
            _scoreEvent.Publish(this, scoreData);
        }
        
        private void PublishLivesUpdate()
        {
            var livesData = new LivesData
            {
                currentLives = _currentLives,
                maxLives = livesCount,
                gameOverOnNextDeath = _currentLives == 1
            };
            _livesEvent.Publish(this, livesData);
        }
        
        // Event Handlers
        private void OnGameStateChanged(object sender, GameState state)
        {
            Debug.Log($"[GameManager] Game state changed to: {state}");
        }
        
        private void OnScoreChanged(object sender, ScoreData score)
        {
            Debug.Log($"[GameManager] Score: {score.currentScore}/{score.targetScore} " +
                     $"({score.progressPercent:P0}) +{score.lastScoreIncrease}");
        }
        
        private void OnLivesChanged(object sender, LivesData lives)
        {
            Debug.Log($"[GameManager] Lives: {lives.currentLives}/{lives.maxLives} " +
                     $"{(lives.gameOverOnNextDeath ? "(CRITICAL)" : "")}");
        }
        
        private void OnTimeChanged(object sender, TimeData time)
        {
            // Only log time updates occasionally to avoid spam
            if (Mathf.RoundToInt(time.elapsedTime) % 10 == 0 && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[GameManager] Time: {time.remainingTime:F0}s remaining " +
                         $"({time.timePercent:P0} elapsed)");
            }
        }
        
        private void OnPlayerAction(object sender, PlayerAction action)
        {
            Debug.Log($"[GameManager] Player Action: {action.type} - {action.details}");
            
            if (action.type == PlayerActionType.Die)
            {
                LoseLife();
            }
            else if (action.type == PlayerActionType.Collect)
            {
                AddScore(action.value);
            }
        }
        
        private void OnEnemyAction(object sender, EnemyAction action)
        {
            Debug.Log($"[GameManager] Enemy Action: {action.type} by {action.enemyId}");
        }
        
        private void OnGameEvent(object sender, GameEvent gameEvent)
        {
            Debug.Log($"[GameManager] Game Event: {gameEvent.type} - {gameEvent.message}");
        }
        
        void OnDestroy()
        {
            _disposeBag.Dispose();
        }
    }
}