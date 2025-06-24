using UnityEngine;
using Framework.Events;
using Framework.Events.Extensions;

namespace Samples.BasicUsage
{
    /// <summary>
    /// Demonstrates basic event usage with subscription and publishing.
    /// This example shows how to create events, subscribe to them, and publish data.
    /// </summary>
    public class BasicEventExample : MonoBehaviour
    {
        [Header("Event Configuration")]
        [SerializeField] private int initialHealth = 100;
        [SerializeField] private float damageInterval = 2f;
        
        // Event producers for different game events
        private BaseEventProducer<int> _healthEvent;
        private BaseEventProducer<string> _messageEvent;
        
        // DisposeBag for managing subscriptions
        private DisposeBag _disposeBag = new DisposeBag();
        
        void Start()
        {
            // Initialize event producers
            _healthEvent = new BaseEventProducer<int>();
            _messageEvent = new BaseEventProducer<string>();
            
            // Subscribe to events using DisposeBag for automatic cleanup
            _healthEvent.listener.Subscribe(OnHealthChanged).AddToDisposables(_disposeBag);
            _messageEvent.listener.Subscribe(OnMessageReceived).AddToDisposables(_disposeBag);
            
            // Publish initial values
            _healthEvent.Publish(this, initialHealth);
            _messageEvent.Publish(this, "Game started!");
            
            // Start simulating damage over time
            InvokeRepeating(nameof(SimulateDamage), damageInterval, damageInterval);
        }
        
        private void OnHealthChanged(object sender, int newHealth)
        {
            Debug.Log($"[BasicEventExample] Health changed to: {newHealth}");
            
            // Publish status messages based on health
            if (newHealth <= 0)
            {
                _messageEvent.Publish(this, "Player defeated!");
                CancelInvoke(nameof(SimulateDamage));
            }
            else if (newHealth <= 30)
            {
                _messageEvent.Publish(this, "Health critical!");
            }
            else if (newHealth <= 50)
            {
                _messageEvent.Publish(this, "Health low!");
            }
        }
        
        private void OnMessageReceived(object sender, string message)
        {
            Debug.Log($"[BasicEventExample] Message: {message}");
        }
        
        private void SimulateDamage()
        {
            // Get current health and reduce it
            int currentHealth = _healthEvent.state.lastState;
            int newHealth = Mathf.Max(0, currentHealth - Random.Range(5, 20));
            
            _healthEvent.Publish(this, newHealth);
        }
        
        void OnDestroy()
        {
            // Clean up all subscriptions
            _disposeBag.Dispose();
        }
    }
}