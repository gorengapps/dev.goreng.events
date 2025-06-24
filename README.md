# Event Framework for Unity

A lightweight, flexible event system for Unity that enables decoupled communication between game objects and systems. This framework provides a robust foundation for implementing the observer pattern with state retention, automatic cleanup, and fluent syntax.

## Features

- **Type-safe event handling** with generic interfaces
- **State retention** - Events can remember and replay their last state
- **Automatic cleanup** with disposable subscriptions and DisposeBag
- **Fluent syntax** with extension methods for chaining operations
- **Memory efficient** with proper disposal patterns
- **Unity-optimized** with MonoBehaviour integration support

## Installation

### Package Manager (Recommended)

1. Open Unity Package Manager
2. Click the "+" button
3. Select "Add package from git URL"
4. Enter: `https://github.com/gorengapps/dev.goreng.events.git`

### Manual Installation

1. Download or clone this repository
2. Copy the `Events` folder to your Unity project's `Assets` folder
3. The framework will be available under the `Framework.Events` namespace

## Architecture Overview

The framework is built around several core interfaces:

- **`IEventProducer<T>`** - Objects that can publish events
- **`IEventListener<T>`** - Objects that can subscribe to events  
- **`IStateRetainer<T>`** - Objects that retain the last published state
- **`IEventContainer<T>`** - Combines state retention with event publishing

## Quick Start

### Basic Event Producer and Listener

```csharp
using Framework.Events;
using UnityEngine;

public class PlayerHealthExample : MonoBehaviour
{
    // Create an event producer for health changes
    private readonly BaseEventProducer<int> healthProducer = new BaseEventProducer<int>();
    
    // Expose the listener so others can subscribe
    public IEventListener<int> HealthListener => healthProducer.listener;
    
    private int currentHealth = 100;
    
    void Start()
    {
        // Subscribe to our own health changes for logging
        healthProducer.listener.Subscribe(OnHealthChanged);
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        // Publish the new health value
        healthProducer.Publish(this, currentHealth);
    }
    
    private void OnHealthChanged(object sender, int newHealth)
    {
        Debug.Log($"Health changed to: {newHealth}");
    }
}
```

### Subscribing to Events with Automatic Cleanup

```csharp
using Framework.Events;
using Framework.Events.Extensions;
using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    private DisposeBag disposeBag = new DisposeBag();
    private PlayerHealthExample playerHealth;
    
    void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealthExample>();
        
        // Subscribe to health changes and automatically clean up when destroyed
        playerHealth.HealthListener
            .Subscribe(UpdateHealthBar)
            .AddToDisposables(disposeBag);
    }
    
    private void UpdateHealthBar(object sender, int health)
    {
        // Update your health bar UI here
        Debug.Log($"Updating health bar to {health}%");
    }
    
    void OnDestroy()
    {
        // Automatically unsubscribe from all events
        disposeBag.Dispose();
    }
}
```

### State Retention and Replay

```csharp
using Framework.Events;
using UnityEngine;

public class GameStateExample : MonoBehaviour
{
    // Create producer with repeat=true to replay last state to new subscribers
    private readonly BaseEventProducer<string> gameStateProducer = new BaseEventProducer<string>(repeat: true);
    
    public IEventListener<string> GameStateListener => gameStateProducer.listener;
    public IStateRetainer<string> GameState => gameStateProducer.state;
    
    void Start()
    {
        // Set initial game state
        gameStateProducer.Publish(this, "MainMenu");
        
        // Later subscribers will immediately receive "MainMenu" when they subscribe
    }
    
    public void StartGame()
    {
        gameStateProducer.Publish(this, "Playing");
    }
    
    public void PauseGame()
    {
        gameStateProducer.Publish(this, "Paused");
    }
}
```

### Event Transformation with PipeTo

```csharp
using Extensions;
using Framework.Events;
using UnityEngine;

public class EventTransformExample : MonoBehaviour
{
    private readonly BaseEventProducer<float> temperatureProducer = new BaseEventProducer<float>();
    private readonly BaseEventProducer<string> weatherDescriptionProducer = new BaseEventProducer<string>();
    
    private DisposeBag disposeBag = new DisposeBag();
    
    void Start()
    {
        // Transform temperature values to weather descriptions
        temperatureProducer.listener.PipeTo(
            weatherDescriptionProducer,
            temp => temp > 25f ? "Hot" : temp > 10f ? "Mild" : "Cold"
        ).AddToDisposables(disposeBag);
        
        // Subscribe to weather descriptions
        weatherDescriptionProducer.listener.Subscribe((sender, weather) => 
        {
            Debug.Log($"Weather is: {weather}");
        }).AddToDisposables(disposeBag);
        
        // Publish temperature - will automatically transform to weather description
        temperatureProducer.Publish(this, 30f); // Output: "Weather is: Hot"
    }
    
    void OnDestroy()
    {
        disposeBag.Dispose();
    }
}
```

### Advanced: Custom Event Producer

```csharp
using Framework.Events;
using UnityEngine;

[System.Serializable]
public struct PlayerMoveData
{
    public Vector3 position;
    public Vector3 direction;
    public float speed;
}

public class PlayerMovementProducer : MonoBehaviour, IEventProducer<PlayerMoveData>
{
    private readonly BaseEventProducer<PlayerMoveData> eventProducer = new BaseEventProducer<PlayerMoveData>();
    
    public IStateRetainer<PlayerMoveData> state => eventProducer.state;
    public IEventListener<PlayerMoveData> listener => eventProducer.listener;
    
    public void Publish(object sender, PlayerMoveData data)
    {
        eventProducer.Publish(sender, data);
    }
    
    void Update()
    {
        if (transform.hasChanged)
        {
            var moveData = new PlayerMoveData
            {
                position = transform.position,
                direction = transform.forward,
                speed = GetComponent<Rigidbody>()?.velocity.magnitude ?? 0f
            };
            
            Publish(this, moveData);
            transform.hasChanged = false;
        }
    }
}
```

## API Reference

### Core Interfaces

#### `IEventProducer<T>`
Produces events of type T with state retention and listener management.

```csharp
public interface IEventProducer<T>
{
    IStateRetainer<T> state { get; }      // Access to last published state
    IEventListener<T> listener { get; }   // Subscribe to events
    void Publish(object sender, T data);  // Publish new events
}
```

#### `IEventListener<T>`
Subscribe to and unsubscribe from events of type T.

```csharp
public interface IEventListener<T>
{
    IDisposable Subscribe(EventHandler<T> handler);   // Returns disposable subscription
    void Unsubscribe(EventHandler<T> handler);        // Manual unsubscribe
}
```

#### `IStateRetainer<T>`
Provides access to the last published state.

```csharp
public interface IStateRetainer<T>
{
    T lastState { get; }  // The most recent published data
}
```

### Core Classes

#### `BaseEventProducer<T>`
Standard implementation of `IEventProducer<T>`.

```csharp
// Create without state replay
var producer = new BaseEventProducer<int>();

// Create with state replay for new subscribers
var producer = new BaseEventProducer<int>(repeat: true);
```

#### `DisposeBag`
Manages multiple disposables and provides cancellation token support.

```csharp
var bag = new DisposeBag();
subscription1.AddToDisposables(bag);
subscription2.AddToDisposables(bag);
// ... add more disposables

bag.Dispose(); // Disposes all and cancels token
```

### Extension Methods

#### `PipeTo<T>(target)`
Forward events from source to target without transformation.

```csharp
sourceListener.PipeTo(targetProducer);
```

#### `PipeTo<T,Y>(target, transform)`
Forward events with transformation function.

```csharp
sourceListener.PipeTo(targetProducer, data => TransformData(data));
```

#### `AddToDisposables(bag)`
Add disposable to a DisposeBag for automatic cleanup.

```csharp
subscription.AddToDisposables(disposeBag);
```

#### `CombineLatest<T1, T2>(source2)`
Combines the latest values of two event listeners into a single event stream.

```csharp
var combined = source1.CombineLatest(source2);
combined.Subscribe((sender, data) => {
    var (value1, value2) = data;
    Debug.Log($"Combined: {value1}, {value2}");
});
```

## Best Practices

1. **Always use DisposeBag** for automatic cleanup in MonoBehaviours
2. **Use state retention sparingly** - only when you need replay functionality
3. **Prefer composition over inheritance** - use `BaseEventProducer<T>` as a field
4. **Name your events clearly** - use descriptive types for event data
5. **Handle null cases** - check for null in event handlers when needed
6. **Use fluent syntax** - chain operations with PipeTo and AddToDisposables

## Contributing

This is an internal package for Goreng Apps. For questions or issues, please contact the development team.

## License

Copyright (C) 2024 Jason Meulenhoff  
Licensed under proprietary license - see LICENSE.md for details.
