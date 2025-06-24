using UnityEngine;
using UnityEngine.UI;
using Framework.Events;
using Framework.Events.Extensions;

namespace Samples.GameplayExamples
{
    /// <summary>
    /// Demonstrates UI integration with the event system, showing how to
    /// create reactive UI that responds to game events without tight coupling.
    /// This example shows a complete UI system driven entirely by events.
    /// </summary>
    public class UIEventIntegration : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Text healthText;
        [SerializeField] private Slider experienceBar;
        [SerializeField] private Text levelText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text gameStateText;
        [SerializeField] private Transform statusEffectsContainer;
        [SerializeField] private GameObject statusEffectPrefab;
        
        [Header("UI Configuration")]
        [SerializeField] private Color healthColorNormal = Color.green;
        [SerializeField] private Color healthColorWarning = Color.yellow;
        [SerializeField] private Color healthColorCritical = Color.red;
        [SerializeField] private float uiUpdateSmoothTime = 0.2f;
        
        // Event references (these would typically be injected or found via a service locator)
        private BaseEventProducer<HealthData> _healthEvent;
        private BaseEventProducer<ExperienceData> _experienceEvent;
        private BaseEventProducer<ScoreData> _scoreEvent;
        private BaseEventProducer<GameState> _gameStateEvent;
        private BaseEventProducer<StatusEffectData> _statusEffectEvent;
        
        private DisposeBag _disposeBag = new DisposeBag();
        
        // UI State for smooth transitions
        private float _targetHealthPercent;
        private float _currentHealthPercent;
        private float _targetXpPercent;
        private float _currentXpPercent;
        
        // Data structures (matching the PlayerEventSystem)
        [System.Serializable]
        public struct HealthData
        {
            public int currentHealth;
            public int maxHealth;
            public float healthPercentage;
            public bool isAlive;
        }
        
        [System.Serializable]
        public struct ExperienceData
        {
            public int currentLevel;
            public int currentXP;
            public int xpToNextLevel;
            public float xpProgressPercent;
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
        public struct StatusEffectData
        {
            public StatusEffect effect;
            public bool wasAdded;
        }
        
        [System.Serializable]
        public class StatusEffect
        {
            public string name;
            public StatusEffectType type;
            public float duration;
            public float remainingTime;
            public float intensity;
        }
        
        public enum StatusEffectType
        {
            Poison,
            Regeneration,
            SpeedBoost,
            StrengthBoost,
            Paralysis,
            Invincibility
        }
        
        public enum GameState
        {
            MainMenu,
            Loading,
            Playing,
            Paused,
            GameOver,
            Victory
        }
        
        void Start()
        {
            InitializeUIEvents();
            SetupEventSubscriptions();
            ValidateUIReferences();
            
            Debug.Log("UI Event Integration initialized. UI will respond to game events automatically.");
        }
        
        private void InitializeUIEvents()
        {
            // In a real implementation, these would be injected or retrieved from a service locator
            // For this example, we create them here to demonstrate the pattern
            _healthEvent = new BaseEventProducer<HealthData>(repeat: true);
            _experienceEvent = new BaseEventProducer<ExperienceData>(repeat: true);
            _scoreEvent = new BaseEventProducer<ScoreData>(repeat: true);
            _gameStateEvent = new BaseEventProducer<GameState>(repeat: true);
            _statusEffectEvent = new BaseEventProducer<StatusEffectData>();
            
            // Publish initial test data
            PublishTestData();
        }
        
        private void SetupEventSubscriptions()
        {
            // Subscribe to all relevant game events
            _healthEvent.listener.Subscribe(OnHealthChanged).AddToDisposables(_disposeBag);
            _experienceEvent.listener.Subscribe(OnExperienceChanged).AddToDisposables(_disposeBag);
            _scoreEvent.listener.Subscribe(OnScoreChanged).AddToDisposables(_disposeBag);
            _gameStateEvent.listener.Subscribe(OnGameStateChanged).AddToDisposables(_disposeBag);
            _statusEffectEvent.listener.Subscribe(OnStatusEffectChanged).AddToDisposables(_disposeBag);
        }
        
        private void ValidateUIReferences()
        {
            // Validate that all UI elements are assigned
            if (healthBar == null) Debug.LogWarning("[UIEventIntegration] Health bar not assigned");
            if (healthText == null) Debug.LogWarning("[UIEventIntegration] Health text not assigned");
            if (experienceBar == null) Debug.LogWarning("[UIEventIntegration] Experience bar not assigned");
            if (levelText == null) Debug.LogWarning("[UIEventIntegration] Level text not assigned");
            if (scoreText == null) Debug.LogWarning("[UIEventIntegration] Score text not assigned");
            if (gameStateText == null) Debug.LogWarning("[UIEventIntegration] Game state text not assigned");
            if (statusEffectsContainer == null) Debug.LogWarning("[UIEventIntegration] Status effects container not assigned");
        }
        
        void Update()
        {
            // Smooth UI transitions
            UpdateUIAnimations();
            
            // Test input for demonstration
            HandleTestInput();
        }
        
        private void UpdateUIAnimations()
        {
            // Smooth health bar animation
            if (Mathf.Abs(_currentHealthPercent - _targetHealthPercent) > 0.01f)
            {
                _currentHealthPercent = Mathf.Lerp(_currentHealthPercent, _targetHealthPercent, 
                    Time.deltaTime / uiUpdateSmoothTime);
                
                if (healthBar != null)
                {
                    healthBar.value = _currentHealthPercent;
                }
            }
            
            // Smooth experience bar animation
            if (Mathf.Abs(_currentXpPercent - _targetXpPercent) > 0.01f)
            {
                _currentXpPercent = Mathf.Lerp(_currentXpPercent, _targetXpPercent,
                    Time.deltaTime / uiUpdateSmoothTime);
                
                if (experienceBar != null)
                {
                    experienceBar.value = _currentXpPercent;
                }
            }
        }
        
        private void HandleTestInput()
        {
            // Test events for demonstration
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // Simulate health change
                var health = _healthEvent.state.lastState;
                var newHealth = new HealthData
                {
                    currentHealth = Mathf.Max(0, health.currentHealth - 20),
                    maxHealth = health.maxHealth,
                    healthPercentage = 0, // Will be calculated
                    isAlive = health.currentHealth > 20
                };
                newHealth.healthPercentage = (float)newHealth.currentHealth / newHealth.maxHealth;
                _healthEvent.Publish(this, newHealth);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // Simulate experience gain
                var exp = _experienceEvent.state.lastState;
                var newExp = new ExperienceData
                {
                    currentLevel = exp.currentLevel,
                    currentXP = exp.currentXP + 50,
                    xpToNextLevel = exp.xpToNextLevel,
                    xpProgressPercent = 0 // Will be calculated
                };
                newExp.xpProgressPercent = (float)newExp.currentXP / newExp.xpToNextLevel;
                _experienceEvent.Publish(this, newExp);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                // Simulate score increase
                var score = _scoreEvent.state.lastState;
                var newScore = new ScoreData
                {
                    currentScore = score.currentScore + 100,
                    targetScore = score.targetScore,
                    lastScoreIncrease = 100,
                    progressPercent = 0 // Will be calculated
                };
                newScore.progressPercent = (float)newScore.currentScore / newScore.targetScore;
                _scoreEvent.Publish(this, newScore);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                // Simulate game state change
                var currentState = _gameStateEvent.state.lastState;
                var newState = currentState == GameState.Playing ? GameState.Paused : GameState.Playing;
                _gameStateEvent.Publish(this, newState);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                // Simulate status effect
                var effect = new StatusEffect
                {
                    name = "Test Effect",
                    type = StatusEffectType.SpeedBoost,
                    duration = 10f,
                    remainingTime = 10f,
                    intensity = 1.5f
                };
                var statusData = new StatusEffectData { effect = effect, wasAdded = true };
                _statusEffectEvent.Publish(this, statusData);
            }
        }
        
        private void PublishTestData()
        {
            // Publish initial test data to demonstrate UI
            var initialHealth = new HealthData
            {
                currentHealth = 80,
                maxHealth = 100,
                healthPercentage = 0.8f,
                isAlive = true
            };
            _healthEvent.Publish(this, initialHealth);
            
            var initialExp = new ExperienceData
            {
                currentLevel = 5,
                currentXP = 150,
                xpToNextLevel = 500,
                xpProgressPercent = 0.3f
            };
            _experienceEvent.Publish(this, initialExp);
            
            var initialScore = new ScoreData
            {
                currentScore = 1250,
                targetScore = 5000,
                lastScoreIncrease = 0,
                progressPercent = 0.25f
            };
            _scoreEvent.Publish(this, initialScore);
            
            _gameStateEvent.Publish(this, GameState.Playing);
        }
        
        // Event Handlers
        private void OnHealthChanged(object sender, HealthData health)
        {
            Debug.Log($"[UIEventIntegration] Health UI update: {health.currentHealth}/{health.maxHealth}");
            
            // Update health bar target
            _targetHealthPercent = health.healthPercentage;
            
            // Update health text
            if (healthText != null)
            {
                healthText.text = $"{health.currentHealth}/{health.maxHealth}";
            }
            
            // Update health bar color based on health percentage
            if (healthBar != null)
            {
                var fillImage = healthBar.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (health.healthPercentage > 0.6f)
                        fillImage.color = healthColorNormal;
                    else if (health.healthPercentage > 0.3f)
                        fillImage.color = healthColorWarning;
                    else
                        fillImage.color = healthColorCritical;
                }
            }
        }
        
        private void OnExperienceChanged(object sender, ExperienceData experience)
        {
            Debug.Log($"[UIEventIntegration] Experience UI update: Level {experience.currentLevel}, XP {experience.currentXP}/{experience.xpToNextLevel}");
            
            // Update experience bar target
            _targetXpPercent = experience.xpProgressPercent;
            
            // Update level text
            if (levelText != null)
            {
                levelText.text = $"Level {experience.currentLevel}";
            }
        }
        
        private void OnScoreChanged(object sender, ScoreData score)
        {
            Debug.Log($"[UIEventIntegration] Score UI update: {score.currentScore} (+{score.lastScoreIncrease})");
            
            // Update score text with animation for score increase
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score.currentScore:N0}";
                
                // Simple score increase feedback
                if (score.lastScoreIncrease > 0)
                {
                    StartCoroutine(AnimateScoreIncrease(score.lastScoreIncrease));
                }
            }
        }
        
        private void OnGameStateChanged(object sender, GameState state)
        {
            Debug.Log($"[UIEventIntegration] Game state UI update: {state}");
            
            // Update game state text
            if (gameStateText != null)
            {
                gameStateText.text = state.ToString();
                
                // Color code game states
                gameStateText.color = state switch
                {
                    GameState.Playing => Color.green,
                    GameState.Paused => Color.yellow,
                    GameState.GameOver => Color.red,
                    GameState.Victory => Color.cyan,
                    _ => Color.white
                };
            }
        }
        
        private void OnStatusEffectChanged(object sender, StatusEffectData statusEffect)
        {
            Debug.Log($"[UIEventIntegration] Status effect UI update: {statusEffect.effect.name} ({(statusEffect.wasAdded ? "Added" : "Removed")})");
            
            // Update status effects display (simplified for example)
            if (statusEffectsContainer != null && statusEffectPrefab != null)
            {
                if (statusEffect.wasAdded)
                {
                    var effectUI = Instantiate(statusEffectPrefab, statusEffectsContainer);
                    var text = effectUI.GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        text.text = statusEffect.effect.name;
                    }
                    
                    // Remove the UI element after the effect duration (simplified)
                    Destroy(effectUI, statusEffect.effect.duration);
                }
            }
        }
        
        private System.Collections.IEnumerator AnimateScoreIncrease(int increase)
        {
            // Simple score increase animation
            if (scoreText != null)
            {
                var originalScale = scoreText.transform.localScale;
                var targetScale = originalScale * 1.2f;
                
                // Scale up
                float timer = 0f;
                while (timer < 0.1f)
                {
                    timer += Time.deltaTime;
                    float progress = timer / 0.1f;
                    scoreText.transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
                    yield return null;
                }
                
                // Scale back down
                timer = 0f;
                while (timer < 0.1f)
                {
                    timer += Time.deltaTime;
                    float progress = timer / 0.1f;
                    scoreText.transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
                    yield return null;
                }
                
                scoreText.transform.localScale = originalScale;
            }
        }
        
        void OnDestroy()
        {
            _disposeBag.Dispose();
        }
    }
}