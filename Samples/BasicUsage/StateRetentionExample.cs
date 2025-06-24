using UnityEngine;
using Framework.Events;
using Framework.Events.Extensions;

namespace Samples.BasicUsage
{
    /// <summary>
    /// Demonstrates state retention feature where new subscribers automatically
    /// receive the last published state when repeat=true is used.
    /// </summary>
    public class StateRetentionExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Vector3 initialPosition = Vector3.zero;
        [SerializeField] private float moveSpeed = 5f;
        
        // Event producer with state retention enabled
        private BaseEventProducer<Vector3> _positionEvent;
        private BaseEventProducer<GameState> _gameStateEvent;
        
        private DisposeBag _disposeBag = new DisposeBag();
        
        // Custom enum for game state
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver
        }
        
        void Start()
        {
            // Create event producers with state retention (repeat: true)
            _positionEvent = new BaseEventProducer<Vector3>(repeat: true);
            _gameStateEvent = new BaseEventProducer<GameState>(repeat: true);
            
            // Publish initial states
            _positionEvent.Publish(this, initialPosition);
            _gameStateEvent.Publish(this, GameState.Playing);
            
            // Subscribe after initial states are published
            // These subscribers will immediately receive the current state
            _positionEvent.listener.Subscribe(OnPositionChanged).AddToDisposables(_disposeBag);
            _gameStateEvent.listener.Subscribe(OnGameStateChanged).AddToDisposables(_disposeBag);
            
            Debug.Log("State Retention Example started. Watch for immediate state notifications.");
        }
        
        void Update()
        {
            // Simulate movement and publish position updates
            if (_gameStateEvent.state.lastState == GameState.Playing)
            {
                Vector3 currentPos = _positionEvent.state.lastState;
                Vector3 movement = new Vector3(
                    Mathf.Sin(Time.time * moveSpeed) * 0.1f,
                    0,
                    Mathf.Cos(Time.time * moveSpeed) * 0.1f
                );
                
                Vector3 newPosition = currentPos + movement;
                _positionEvent.Publish(this, newPosition);
            }
            
            // Toggle game state with space key
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleGameState();
            }
        }
        
        private void OnPositionChanged(object sender, Vector3 position)
        {
            Debug.Log($"[StateRetention] Position updated to: {position}");
            transform.position = position;
        }
        
        private void OnGameStateChanged(object sender, GameState newState)
        {
            Debug.Log($"[StateRetention] Game state changed to: {newState}");
            
            // Demonstrate accessing the previous state through the state retainer
            // (In a real scenario, you might want to store previous state differently)
            switch (newState)
            {
                case GameState.Playing:
                    Debug.Log("Game resumed - movement enabled");
                    break;
                case GameState.Paused:
                    Debug.Log("Game paused - movement disabled");
                    break;
            }
        }
        
        private void ToggleGameState()
        {
            GameState currentState = _gameStateEvent.state.lastState;
            GameState newState = currentState == GameState.Playing ? GameState.Paused : GameState.Playing;
            _gameStateEvent.Publish(this, newState);
        }
        
        // Method to demonstrate late subscription (can be called from inspector or other scripts)
        [ContextMenu("Add Late Subscriber")]
        public void AddLateSubscriber()
        {
            Debug.Log("Adding late subscriber - it should immediately receive current state");
            
            _positionEvent.listener.Subscribe((sender, position) =>
            {
                Debug.Log($"[LateSubscriber] Received current position: {position}");
            }).AddToDisposables(_disposeBag);
            
            _gameStateEvent.listener.Subscribe((sender, state) =>
            {
                Debug.Log($"[LateSubscriber] Received current game state: {state}");
            }).AddToDisposables(_disposeBag);
        }
        
        void OnDestroy()
        {
            _disposeBag.Dispose();
        }
    }
}