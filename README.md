# dev.goreng.events

A lightweight, type-safe event framework for Unity that provides decoupled communication between game objects and systems with built-in state management and resource cleanup.

## Features

- **Type-safe Events**: Strongly typed event system with compile-time safety
- **State Retention**: Automatic state management with optional replay for new subscribers
- **Resource Management**: Built-in DisposeBag for automatic cleanup and cancellation tokens
- **Event Transformation**: Pipeline events with transformation capabilities
- **Unity Integration**: Seamless integration with Unity's lifecycle and debugging
- **Memory Efficient**: Automatic subscription cleanup and garbage collection friendly

## Installation

### Unity Package Manager

Add the package to your Unity project using the Package Manager:

1. Open Package Manager (`Window > Package Manager`)
2. Click the `+` button and select `Add package from git URL`
3. Enter: `https://github.com/gorengapps/dev.goreng.events.git`

### Manual Installation

Clone or download this repository and place it in your Unity project's `Packages` folder.

## Quick Start

### Basic Event Usage

```csharp
using Framework.Events;

// Create an event producer for player health changes
var healthEvent = new BaseEventProducer<int>();

// Subscribe to health changes
var subscription = healthEvent.listener.Subscribe((sender, health) => 
{
    Debug.Log($"Player health changed to: {health}");
});

// Publish health changes
healthEvent.Publish(this, 100);
healthEvent.Publish(this, 85);

// Clean up subscription
subscription.Dispose();
```

### State Retention

```csharp
// Create an event producer with state retention
var playerPositionEvent = new BaseEventProducer<Vector3>(repeat: true);

// Publish initial position
playerPositionEvent.Publish(this, new Vector3(0, 0, 0));

// New subscribers automatically receive the last published state
var subscription = playerPositionEvent.listener.Subscribe((sender, position) => 
{
    Debug.Log($"Player position: {position}"); // Immediately receives (0,0,0)
});

// Access the last state directly
Vector3 lastPosition = playerPositionEvent.state.lastState;
```

### Resource Management with DisposeBag

```csharp
using Framework.Events;
using Framework.Events.Extensions;

public class PlayerController : MonoBehaviour
{
    private DisposeBag _disposeBag = new DisposeBag();
    
    void Start()
    {
        // All subscriptions are automatically cleaned up when DisposeBag is disposed
        healthEvent.listener.Subscribe(OnHealthChanged).AddToDisposables(_disposeBag);
        scoreEvent.listener.Subscribe(OnScoreChanged).AddToDisposables(_disposeBag);
        
        // Use cancellation token for async operations
        StartCoroutine(PlayerUpdateLoop(_disposeBag.token.Token));
    }
    
    void OnDestroy()
    {
        _disposeBag.Dispose(); // Automatically unsubscribes and cancels tokens
    }
    
    private void OnHealthChanged(object sender, int health) { /* ... */ }
    private void OnScoreChanged(object sender, int score) { /* ... */ }
}
```

## API Reference

### Core Interfaces

#### IEventProducer&lt;T&gt;
```csharp
public interface IEventProducer<T>
{
    IStateRetainer<T> state { get; }     // Access to last published state
    IEventListener<T> listener { get; }  // Subscribe to events
    void Publish(object sender, T data); // Publish new events
}
```

#### IEventListener&lt;T&gt;
```csharp
public interface IEventListener<T>
{
    IDisposable Subscribe(EventHandler<T> handler);   // Subscribe to events
    void Unsubscribe(EventHandler<T> handler);        // Manual unsubscribe
}
```

### Core Classes

#### BaseEventProducer&lt;T&gt;
The main event producer implementation.

```csharp
// Standard event producer
var eventProducer = new BaseEventProducer<string>();

// Event producer with state retention (replays last state to new subscribers)
var eventProducerWithReplay = new BaseEventProducer<string>(repeat: true);
```

#### DisposeBag
Manages multiple disposable resources and provides cancellation tokens.

```csharp
var disposeBag = new DisposeBag();

// Add disposables for automatic cleanup
subscription1.AddToDisposables(disposeBag);
subscription2.AddToDisposables(disposeBag);

// Access cancellation token
CancellationToken token = disposeBag.token.Token;

// Dispose all resources at once
disposeBag.Dispose(); // Cancels token and disposes all added disposables
```

### Extension Methods

#### Event Piping
```csharp
using Extensions;

// Pipe events from one producer to another
var sourceEvent = new BaseEventProducer<int>();
var targetEvent = new BaseEventProducer<int>();

var pipe = sourceEvent.listener.PipeTo(targetEvent);

// Pipe with transformation
var stringEvent = new BaseEventProducer<string>();
var intEvent = new BaseEventProducer<int>();

var transformPipe = stringEvent.listener.PipeTo(intEvent, str => str.Length);
```

#### Disposable Extensions
```csharp
using Framework.Events.Extensions;

// Add any IDisposable to a DisposeBag
subscription.AddToDisposables(disposeBag);
```

## Advanced Usage

### Event Chaining and Transformation

```csharp
// Create a chain of event transformations
var rawInputEvent = new BaseEventProducer<KeyCode>();
var processedInputEvent = new BaseEventProducer<InputAction>();
var gameActionEvent = new BaseEventProducer<GameAction>();

// Chain transformations
rawInputEvent.listener.PipeTo(processedInputEvent, keyCode => 
    ConvertKeyCodeToInputAction(keyCode));

processedInputEvent.listener.PipeTo(gameActionEvent, inputAction => 
    ConvertInputActionToGameAction(inputAction));

// Subscribe to the final transformed event
gameActionEvent.listener.Subscribe((sender, action) => 
{
    ExecuteGameAction(action);
});
```

### Async Operations with Cancellation

```csharp
public class AsyncGameSystem : MonoBehaviour
{
    private DisposeBag _disposeBag = new DisposeBag();
    
    async void Start()
    {
        // Long-running async operation that respects cancellation
        await LoadGameDataAsync(_disposeBag.token.Token);
    }
    
    async Task LoadGameDataAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Perform work...
            await Task.Delay(1000, cancellationToken);
        }
    }
    
    void OnDestroy()
    {
        _disposeBag.Dispose(); // Cancels all async operations
    }
}
```

### Custom Event Types

```csharp
// Define custom event data structures
public struct PlayerDamageEvent
{
    public int damage;
    public Vector3 position;
    public DamageType type;
}

public struct GameStateEvent
{
    public GameState previousState;
    public GameState currentState;
    public float transitionTime;
}

// Use with the event system
var damageEvent = new BaseEventProducer<PlayerDamageEvent>();
var stateEvent = new BaseEventProducer<GameStateEvent>(repeat: true);
```

## Best Practices

### Resource Management
- Always use `DisposeBag` for managing multiple subscriptions
- Dispose subscriptions in `OnDestroy()` for MonoBehaviours
- Use cancellation tokens for async operations

### Event Design
- Keep event data structures small and immutable
- Use descriptive names for events (e.g., `PlayerHealthChanged`, `ItemCollected`)
- Consider using state retention for configuration and status events

### Performance
- Avoid frequent allocations in event handlers
- Use object pooling for complex event data when appropriate
- Unsubscribe from events when no longer needed

### Debugging
- Event handlers are called synchronously, making debugging straightforward
- Use Unity's console logging within event handlers
- The framework provides error handling for dispose operations

## License

MIT License - see [LICENSE.md](LICENSE.md) for details.

## Contributing

Issues and pull requests are welcome on [GitHub](https://github.com/gorengapps/dev.goreng.events).

## Version

Current version: 1.3.2
