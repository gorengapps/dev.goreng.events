using UnityEngine;
using System.Collections.Generic;
using Framework.Events;
using Framework.Events.Extensions;

namespace Samples.GameplayExamples
{
    /// <summary>
    /// Demonstrates a complete player system using events for health, inventory,
    /// experience, and status effects. Shows how different game systems can
    /// communicate through events without tight coupling.
    /// </summary>
    public class PlayerEventSystem : MonoBehaviour
    {
        [Header("Player Configuration")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private int maxInventorySlots = 10;
        
        // Player event producers
        private BaseEventProducer<HealthData> _healthEvent;
        private BaseEventProducer<ExperienceData> _experienceEvent;
        private BaseEventProducer<InventoryData> _inventoryEvent;
        private BaseEventProducer<StatusEffectData> _statusEffectEvent;
        private BaseEventProducer<PlayerStateData> _playerStateEvent;
        
        private DisposeBag _disposeBag = new DisposeBag();
        private List<StatusEffect> _activeStatusEffects = new List<StatusEffect>();
        private List<InventoryItem> _inventory = new List<InventoryItem>();
        
        // Player data structures
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
        public struct InventoryData
        {
            public int usedSlots;
            public int maxSlots;
            public bool isFull;
            public InventoryItem lastAddedItem;
            public InventoryItem lastRemovedItem;
        }
        
        [System.Serializable]
        public struct StatusEffectData
        {
            public StatusEffect effect;
            public bool wasAdded; // true for added, false for removed
        }
        
        [System.Serializable]
        public struct PlayerStateData
        {
            public bool canMove;
            public bool canAttack;
            public bool canUseItems;
            public float moveSpeedMultiplier;
            public float damageMultiplier;
        }
        
        [System.Serializable]
        public class StatusEffect
        {
            public string name;
            public StatusEffectType type;
            public float duration;
            public float remainingTime;
            public float intensity;
            
            public StatusEffect(string name, StatusEffectType type, float duration, float intensity = 1f)
            {
                this.name = name;
                this.type = type;
                this.duration = duration;
                this.remainingTime = duration;
                this.intensity = intensity;
            }
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
        
        [System.Serializable]
        public class InventoryItem
        {
            public string name;
            public ItemType type;
            public int quantity;
            public string description;
            
            public InventoryItem(string name, ItemType type, int quantity = 1, string description = "")
            {
                this.name = name;
                this.type = type;
                this.quantity = quantity;
                this.description = description;
            }
        }
        
        public enum ItemType
        {
            Weapon,
            Armor,
            Consumable,
            QuestItem,
            Material
        }
        
        void Start()
        {
            InitializePlayerEvents();
            SetupEventSubscriptions();
            InitializePlayerState();
            
            Debug.Log("Player Event System initialized. Press keys to test: H(damage), R(heal), X(gain XP), I(add item), P(poison), S(speed boost)");
        }
        
        private void InitializePlayerEvents()
        {
            // Initialize all player event producers with state retention
            _healthEvent = new BaseEventProducer<HealthData>(repeat: true);
            _experienceEvent = new BaseEventProducer<ExperienceData>(repeat: true);
            _inventoryEvent = new BaseEventProducer<InventoryData>(repeat: true);
            _statusEffectEvent = new BaseEventProducer<StatusEffectData>();
            _playerStateEvent = new BaseEventProducer<PlayerStateData>(repeat: true);
        }
        
        private void SetupEventSubscriptions()
        {
            // Subscribe to all player events for logging and cross-system reactions
            _healthEvent.listener.Subscribe(OnHealthChanged).AddToDisposables(_disposeBag);
            _experienceEvent.listener.Subscribe(OnExperienceChanged).AddToDisposables(_disposeBag);
            _inventoryEvent.listener.Subscribe(OnInventoryChanged).AddToDisposables(_disposeBag);
            _statusEffectEvent.listener.Subscribe(OnStatusEffectChanged).AddToDisposables(_disposeBag);
            _playerStateEvent.listener.Subscribe(OnPlayerStateChanged).AddToDisposables(_disposeBag);
        }
        
        private void InitializePlayerState()
        {
            // Publish initial player state
            PublishHealthUpdate(maxHealth);
            PublishExperienceUpdate(startingLevel, 0);
            PublishInventoryUpdate();
            UpdatePlayerState();
        }
        
        void Update()
        {
            // Handle input for testing different events
            HandleTestInput();
            
            // Update status effects
            UpdateStatusEffects();
        }
        
        private void HandleTestInput()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                TakeDamage(Random.Range(10, 25));
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                Heal(Random.Range(15, 30));
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                GainExperience(Random.Range(50, 150));
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                AddRandomItem();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                AddStatusEffect(new StatusEffect("Poison", StatusEffectType.Poison, 10f, 0.5f));
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                AddStatusEffect(new StatusEffect("Speed Boost", StatusEffectType.SpeedBoost, 15f, 1.5f));
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                ClearAllStatusEffects();
            }
        }
        
        public void TakeDamage(int damage)
        {
            var currentHealth = _healthEvent.state.lastState;
            if (!currentHealth.isAlive) return;
            
            // Apply damage multipliers from status effects
            float damageMultiplier = GetDamageMultiplier();
            int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);
            
            int newHealth = Mathf.Max(0, currentHealth.currentHealth - finalDamage);
            PublishHealthUpdate(newHealth);
            
            Debug.Log($"[PlayerSystem] Took {finalDamage} damage (base: {damage}, multiplier: {damageMultiplier:F2})");
        }
        
        public void Heal(int amount)
        {
            var currentHealth = _healthEvent.state.lastState;
            if (!currentHealth.isAlive) return;
            
            int newHealth = Mathf.Min(currentHealth.maxHealth, currentHealth.currentHealth + amount);
            PublishHealthUpdate(newHealth);
            
            Debug.Log($"[PlayerSystem] Healed for {amount} points");
        }
        
        public void GainExperience(int xp)
        {
            var currentXP = _experienceEvent.state.lastState;
            int newXP = currentXP.currentXP + xp;
            
            // Check for level up
            int newLevel = currentXP.currentLevel;
            while (newXP >= GetXPRequiredForLevel(newLevel + 1))
            {
                newXP -= GetXPRequiredForLevel(newLevel + 1);
                newLevel++;
                Debug.Log($"[PlayerSystem] LEVEL UP! Now level {newLevel}");
            }
            
            PublishExperienceUpdate(newLevel, newXP);
        }
        
        public void AddRandomItem()
        {
            var items = new[]
            {
                new InventoryItem("Health Potion", ItemType.Consumable, 1, "Restores 50 HP"),
                new InventoryItem("Iron Sword", ItemType.Weapon, 1, "A sturdy iron sword"),
                new InventoryItem("Leather Armor", ItemType.Armor, 1, "Basic protection"),
                new InventoryItem("Magic Gem", ItemType.Material, Random.Range(1, 5), "Glowing magical crystal")
            };
            
            var randomItem = items[Random.Range(0, items.Length)];
            AddItem(randomItem);
        }
        
        public void AddItem(InventoryItem item)
        {
            if (_inventory.Count >= maxInventorySlots)
            {
                Debug.Log($"[PlayerSystem] Inventory full! Cannot add {item.name}");
                return;
            }
            
            _inventory.Add(item);
            PublishInventoryUpdate(item, null);
            Debug.Log($"[PlayerSystem] Added {item.quantity}x {item.name} to inventory");
        }
        
        public void AddStatusEffect(StatusEffect effect)
        {
            // Remove existing effect of the same type
            _activeStatusEffects.RemoveAll(e => e.type == effect.type);
            
            _activeStatusEffects.Add(effect);
            _statusEffectEvent.Publish(this, new StatusEffectData { effect = effect, wasAdded = true });
            
            UpdatePlayerState();
            Debug.Log($"[PlayerSystem] Added status effect: {effect.name} for {effect.duration:F1} seconds");
        }
        
        private void UpdateStatusEffects()
        {
            for (int i = _activeStatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeStatusEffects[i];
                effect.remainingTime -= Time.deltaTime;
                
                // Apply effect (for continuous effects like poison/regen)
                ApplyStatusEffect(effect);
                
                if (effect.remainingTime <= 0)
                {
                    _activeStatusEffects.RemoveAt(i);
                    _statusEffectEvent.Publish(this, new StatusEffectData { effect = effect, wasAdded = false });
                    Debug.Log($"[PlayerSystem] Status effect expired: {effect.name}");
                    UpdatePlayerState();
                }
            }
        }
        
        private void ApplyStatusEffect(StatusEffect effect)
        {
            switch (effect.type)
            {
                case StatusEffectType.Poison:
                    if (Time.frameCount % 60 == 0) // Every second
                    {
                        TakeDamage(Mathf.RoundToInt(5 * effect.intensity));
                    }
                    break;
                case StatusEffectType.Regeneration:
                    if (Time.frameCount % 120 == 0) // Every 2 seconds
                    {
                        Heal(Mathf.RoundToInt(3 * effect.intensity));
                    }
                    break;
            }
        }
        
        private void ClearAllStatusEffects()
        {
            foreach (var effect in _activeStatusEffects)
            {
                _statusEffectEvent.Publish(this, new StatusEffectData { effect = effect, wasAdded = false });
            }
            _activeStatusEffects.Clear();
            UpdatePlayerState();
            Debug.Log("[PlayerSystem] Cleared all status effects");
        }
        
        private void PublishHealthUpdate(int newHealth)
        {
            var healthData = new HealthData
            {
                currentHealth = newHealth,
                maxHealth = maxHealth,
                healthPercentage = (float)newHealth / maxHealth,
                isAlive = newHealth > 0
            };
            
            _healthEvent.Publish(this, healthData);
        }
        
        private void PublishExperienceUpdate(int level, int xp)
        {
            int xpRequired = GetXPRequiredForLevel(level + 1);
            var expData = new ExperienceData
            {
                currentLevel = level,
                currentXP = xp,
                xpToNextLevel = xpRequired,
                xpProgressPercent = (float)xp / xpRequired
            };
            
            _experienceEvent.Publish(this, expData);
        }
        
        private void PublishInventoryUpdate(InventoryItem addedItem = null, InventoryItem removedItem = null)
        {
            var inventoryData = new InventoryData
            {
                usedSlots = _inventory.Count,
                maxSlots = maxInventorySlots,
                isFull = _inventory.Count >= maxInventorySlots,
                lastAddedItem = addedItem,
                lastRemovedItem = removedItem
            };
            
            _inventoryEvent.Publish(this, inventoryData);
        }
        
        private void UpdatePlayerState()
        {
            bool hasParalysis = _activeStatusEffects.Exists(e => e.type == StatusEffectType.Paralysis);
            bool hasInvincibility = _activeStatusEffects.Exists(e => e.type == StatusEffectType.Invincibility);
            
            float speedMultiplier = 1f;
            float damageMultiplier = 1f;
            
            foreach (var effect in _activeStatusEffects)
            {
                switch (effect.type)
                {
                    case StatusEffectType.SpeedBoost:
                        speedMultiplier *= effect.intensity;
                        break;
                    case StatusEffectType.StrengthBoost:
                        damageMultiplier *= effect.intensity;
                        break;
                }
            }
            
            var playerState = new PlayerStateData
            {
                canMove = !hasParalysis,
                canAttack = !hasParalysis,
                canUseItems = !hasParalysis,
                moveSpeedMultiplier = hasParalysis ? 0f : speedMultiplier,
                damageMultiplier = damageMultiplier
            };
            
            _playerStateEvent.Publish(this, playerState);
        }
        
        private float GetDamageMultiplier()
        {
            if (_activeStatusEffects.Exists(e => e.type == StatusEffectType.Invincibility))
                return 0f;
            
            return 1f;
        }
        
        private int GetXPRequiredForLevel(int level)
        {
            return level * 100; // Simple XP curve
        }
        
        // Event handlers
        private void OnHealthChanged(object sender, HealthData health)
        {
            Debug.Log($"[PlayerSystem] Health: {health.currentHealth}/{health.maxHealth} ({health.healthPercentage:P0}) - " +
                     $"{(health.isAlive ? "Alive" : "Dead")}");
        }
        
        private void OnExperienceChanged(object sender, ExperienceData experience)
        {
            Debug.Log($"[PlayerSystem] Level {experience.currentLevel} - XP: {experience.currentXP}/{experience.xpToNextLevel} " +
                     $"({experience.xpProgressPercent:P0})");
        }
        
        private void OnInventoryChanged(object sender, InventoryData inventory)
        {
            Debug.Log($"[PlayerSystem] Inventory: {inventory.usedSlots}/{inventory.maxSlots} slots " +
                     $"{(inventory.isFull ? "(FULL)" : "")}");
        }
        
        private void OnStatusEffectChanged(object sender, StatusEffectData statusEffect)
        {
            string action = statusEffect.wasAdded ? "Added" : "Removed";
            Debug.Log($"[PlayerSystem] {action} status effect: {statusEffect.effect.name}");
        }
        
        private void OnPlayerStateChanged(object sender, PlayerStateData state)
        {
            Debug.Log($"[PlayerSystem] Player State - Move: {state.canMove}, Attack: {state.canAttack}, " +
                     $"Speed: {state.moveSpeedMultiplier:F2}x, Damage: {state.damageMultiplier:F2}x");
        }
        
        void OnDestroy()
        {
            _disposeBag.Dispose();
        }
    }
}