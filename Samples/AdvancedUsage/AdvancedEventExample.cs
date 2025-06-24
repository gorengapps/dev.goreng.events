using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Framework.Events;
using Framework.Events.Extensions;
using Extensions;

namespace Samples.AdvancedUsage
{
    /// <summary>
    /// Demonstrates advanced event patterns including event transformation,
    /// chaining, filtering, and async operations with cancellation tokens.
    /// </summary>
    public class AdvancedEventExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float processInterval = 1f;
        [SerializeField] private int maxProcessingItems = 5;
        
        // Source events
        private BaseEventProducer<RawInput> _rawInputEvent;
        private BaseEventProducer<float> _temperatureEvent;
        
        // Transformed events
        private BaseEventProducer<ProcessedInput> _processedInputEvent;
        private BaseEventProducer<TemperatureAlert> _temperatureAlertEvent;
        
        // Final aggregated event
        private BaseEventProducer<SystemStatus> _systemStatusEvent;
        
        private DisposeBag _disposeBag = new DisposeBag();
        
        // Custom data structures
        [System.Serializable]
        public struct RawInput
        {
            public KeyCode keyCode;
            public float timestamp;
            public Vector2 mousePosition;
        }
        
        [System.Serializable]
        public struct ProcessedInput
        {
            public InputType type;
            public float intensity;
            public Vector2 position;
        }
        
        public enum InputType
        {
            Movement,
            Action,
            Menu,
            Unknown
        }
        
        [System.Serializable]
        public struct TemperatureAlert
        {
            public AlertLevel level;
            public float temperature;
            public string message;
        }
        
        public enum AlertLevel
        {
            Normal,
            Warning,
            Critical
        }
        
        [System.Serializable]
        public struct SystemStatus
        {
            public bool isHealthy;
            public int activeAlerts;
            public float systemLoad;
            public string statusMessage;
        }
        
        void Start()
        {
            InitializeEvents();
            SetupEventPipeline();
            StartAsyncProcessing();
            
            Debug.Log("Advanced Event Example started. Press WASD keys to generate input events.");
        }
        
        private void InitializeEvents()
        {
            // Initialize all event producers
            _rawInputEvent = new BaseEventProducer<RawInput>();
            _temperatureEvent = new BaseEventProducer<float>(repeat: true);
            _processedInputEvent = new BaseEventProducer<ProcessedInput>();
            _temperatureAlertEvent = new BaseEventProducer<TemperatureAlert>();
            _systemStatusEvent = new BaseEventProducer<SystemStatus>(repeat: true);
            
            // Subscribe to final events
            _systemStatusEvent.listener.Subscribe(OnSystemStatusChanged).AddToDisposables(_disposeBag);
            
            // Publish initial temperature
            _temperatureEvent.Publish(this, 20f);
        }
        
        private void SetupEventPipeline()
        {
            // Transform raw input to processed input
            _rawInputEvent.listener.PipeTo(_processedInputEvent, TransformRawInput)
                .AddToDisposables(_disposeBag);
            
            // Transform temperature to alerts (with filtering)
            _temperatureEvent.listener.Subscribe((sender, temp) =>
            {
                var alert = TransformTemperatureToAlert(temp);
                if (alert.level != AlertLevel.Normal) // Filter out normal temperatures
                {
                    _temperatureAlertEvent.Publish(sender, alert);
                }
            }).AddToDisposables(_disposeBag);
            
            // Aggregate multiple events into system status
            _processedInputEvent.listener.Subscribe(OnProcessedInputForStatus).AddToDisposables(_disposeBag);
            _temperatureAlertEvent.listener.Subscribe(OnTemperatureAlertForStatus).AddToDisposables(_disposeBag);
        }
        
        void Update()
        {
            // Capture input and publish raw input events
            CaptureInputEvents();
            
            // Simulate temperature changes
            SimulateTemperatureChanges();
        }
        
        private void CaptureInputEvents()
        {
            // Check for various input types
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    var rawInput = new RawInput
                    {
                        keyCode = key,
                        timestamp = Time.time,
                        mousePosition = Input.mousePosition
                    };
                    
                    _rawInputEvent.Publish(this, rawInput);
                    break; // Only process one key per frame
                }
            }
        }
        
        private void SimulateTemperatureChanges()
        {
            // Simulate temperature fluctuations
            if (Random.Range(0f, 1f) < 0.01f) // 1% chance per frame
            {
                float currentTemp = _temperatureEvent.state.lastState;
                float change = Random.Range(-5f, 10f);
                float newTemp = Mathf.Clamp(currentTemp + change, -10f, 100f);
                
                _temperatureEvent.Publish(this, newTemp);
            }
        }
        
        private ProcessedInput TransformRawInput(RawInput raw)
        {
            InputType type = raw.keyCode switch
            {
                KeyCode.W or KeyCode.A or KeyCode.S or KeyCode.D => InputType.Movement,
                KeyCode.Space or KeyCode.Return => InputType.Action,
                KeyCode.Escape or KeyCode.Tab => InputType.Menu,
                _ => InputType.Unknown
            };
            
            float intensity = type == InputType.Movement ? 1f : 
                            type == InputType.Action ? 0.8f : 0.5f;
            
            return new ProcessedInput
            {
                type = type,
                intensity = intensity,
                position = raw.mousePosition
            };
        }
        
        private TemperatureAlert TransformTemperatureToAlert(float temperature)
        {
            AlertLevel level = temperature switch
            {
                > 80f => AlertLevel.Critical,
                > 60f => AlertLevel.Warning,
                _ => AlertLevel.Normal
            };
            
            string message = level switch
            {
                AlertLevel.Critical => "CRITICAL: System overheating!",
                AlertLevel.Warning => "WARNING: High temperature detected",
                _ => "Temperature normal"
            };
            
            return new TemperatureAlert
            {
                level = level,
                temperature = temperature,
                message = message
            };
        }
        
        private void OnProcessedInputForStatus(object sender, ProcessedInput input)
        {
            Debug.Log($"[AdvancedEvent] Processed input: {input.type} (intensity: {input.intensity:F2})");
            UpdateSystemStatus();
        }
        
        private void OnTemperatureAlertForStatus(object sender, TemperatureAlert alert)
        {
            Debug.Log($"[AdvancedEvent] Temperature alert: {alert.level} - {alert.message}");
            UpdateSystemStatus();
        }
        
        private void UpdateSystemStatus()
        {
            // Aggregate information from multiple sources
            bool hasTemperatureAlert = _temperatureAlertEvent.state.lastState.level != AlertLevel.Normal;
            float currentTemp = _temperatureEvent.state.lastState;
            
            var status = new SystemStatus
            {
                isHealthy = !hasTemperatureAlert && currentTemp < 70f,
                activeAlerts = hasTemperatureAlert ? 1 : 0,
                systemLoad = Mathf.Clamp01(currentTemp / 100f),
                statusMessage = hasTemperatureAlert ? "System monitoring active alerts" : "All systems normal"
            };
            
            _systemStatusEvent.Publish(this, status);
        }
        
        private void OnSystemStatusChanged(object sender, SystemStatus status)
        {
            Debug.Log($"[AdvancedEvent] System Status: {(status.isHealthy ? "HEALTHY" : "UNHEALTHY")} " +
                     $"| Load: {status.systemLoad:P0} | Alerts: {status.activeAlerts} | {status.statusMessage}");
        }
        
        private async void StartAsyncProcessing()
        {
            // Start background processing with cancellation support
            await ProcessBackgroundTasks(_disposeBag.token.Token);
        }
        
        private async Task ProcessBackgroundTasks(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Simulate background work
                    await Task.Delay((int)(processInterval * 1000), cancellationToken);
                    
                    // Perform periodic system checks
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Debug.Log("[AdvancedEvent] Background system check completed");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[AdvancedEvent] Background processing cancelled");
            }
        }
        
        void OnDestroy()
        {
            _disposeBag.Dispose();
        }
    }
}