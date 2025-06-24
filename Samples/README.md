# Event Framework Samples

This directory contains comprehensive examples demonstrating the capabilities of the dev.goreng.events framework. Each sample showcases different aspects and patterns of event-driven architecture in Unity.

## Sample Categories

### Basic Usage
Entry-level examples demonstrating core concepts.

### Advanced Usage  
Complex patterns showing event transformation, async operations, and sophisticated event pipelines.

### Gameplay Examples
Real-world game system implementations using events for decoupled architecture.

---

## Basic Usage Samples

### BasicEventExample.cs
**Demonstrates:** Core event publishing and subscription patterns

**Features:**
- Simple event creation and subscription
- DisposeBag usage for resource management
- Basic event publishing with different data types
- Automatic cleanup patterns

**Usage:**
1. Add to a GameObject
2. Play the scene
3. Watch console for health and message events
4. Events simulate damage over time

**Key Learning Points:**
- How to create `BaseEventProducer<T>` instances
- Subscribing to events with `Subscribe()`
- Using `DisposeBag` for automatic cleanup
- Publishing events with `Publish()`

---

### StateRetentionExample.cs  
**Demonstrates:** State retention and replay functionality

**Features:**
- Event producers with state retention (`repeat: true`)
- Late subscriber behavior
- State access through `state.lastState`
- Interactive state changes

**Usage:**
1. Add to a GameObject
2. Play the scene
3. Use Space key to toggle game state
4. Use context menu "Add Late Subscriber" to see state replay
5. Watch position updates in real-time

**Key Learning Points:**
- State retention with `repeat: true` parameter
- Accessing last state via `state.lastState`
- How new subscribers receive current state immediately
- Real-time state updates and visualization

---

## Advanced Usage Samples

### AdvancedEventExample.cs
**Demonstrates:** Event transformation, chaining, and async operations

**Features:**
- Event transformation with `PipeTo<T,Y>()`
- Event filtering and conditional processing
- Multiple event aggregation
- Async operations with cancellation tokens
- Complex data structure events

**Usage:**
1. Add to a GameObject
2. Play the scene
3. Press WASD keys to generate input events
4. Watch console for transformation pipeline results
5. Observe temperature simulation and alerts

**Key Learning Points:**
- Event transformation using `PipeTo()` with transform functions
- Creating event processing pipelines
- Filtering events conditionally
- Aggregating multiple event sources
- Using `DisposeBag.token` for async operations

---

## Gameplay Examples

### PlayerEventSystem.cs
**Demonstrates:** Complete player system with multiple interconnected events

**Features:**
- Health, experience, inventory, and status effect systems
- Cross-system event communication
- Complex state management
- Status effect processing with timers
- Inventory management

**Usage:**
1. Add to a GameObject  
2. Play the scene
3. Use test keys to interact:
   - `H` - Take damage
   - `R` - Heal
   - `X` - Gain experience  
   - `I` - Add random item
   - `P` - Apply poison effect
   - `S` - Apply speed boost
   - `C` - Clear all status effects

**Key Learning Points:**
- Designing interconnected game systems with events
- Complex data structures in events
- Cross-system communication patterns
- State-dependent event processing
- Real-time system updates

---

### GameManagerEventSystem.cs
**Demonstrates:** Game coordination through events

**Features:**
- Game state management
- Score and lives tracking
- Timer-based game mechanics
- Win/lose condition handling
- Cross-system event coordination

**Usage:**
1. Add to a GameObject
2. Play the scene  
3. Use test keys:
   - `G` - Start game
   - `P` - Pause/unpause
   - `R` - Reset to main menu
   - `K` - Simulate player death
   - `S` - Add random score
   - `E` - Spawn enemy (auto-dies for testing)
   - `Q` - Simulate item collection

**Key Learning Points:**
- Central game coordination through events
- Timer-based game mechanics
- Win/lose condition handling
- Event pipelines for game logic
- Decoupled system architecture

---

## Common Patterns Demonstrated

### Resource Management
All samples demonstrate proper resource management using `DisposeBag`:
```csharp
private DisposeBag _disposeBag = new DisposeBag();

void Start()
{
    eventProducer.listener.Subscribe(OnEvent).AddToDisposables(_disposeBag);
}

void OnDestroy()
{
    _disposeBag.Dispose(); // Cleans up all subscriptions
}
```

### Event Transformation
Advanced samples show event transformation patterns:
```csharp
// Transform events from one type to another
sourceEvent.listener.PipeTo(targetEvent, data => TransformData(data));

// Filter and conditionally process events
sourceEvent.listener.Subscribe((sender, data) => {
    if (ShouldProcess(data)) {
        targetEvent.Publish(sender, ProcessedData(data));
    }
});
```

### State Retention
Multiple samples demonstrate state retention:
```csharp
// Create producer with state retention
var stateEvent = new BaseEventProducer<GameState>(repeat: true);

// New subscribers immediately receive current state
stateEvent.Publish(this, GameState.Playing);
stateEvent.listener.Subscribe(OnStateChanged); // Immediately called with GameState.Playing
```

### Async Operations
Advanced samples show async patterns with cancellation:
```csharp
private async Task ProcessAsync(CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        await Task.Delay(1000, cancellationToken);
        // Process...
    }
}

// Use DisposeBag token for automatic cancellation
await ProcessAsync(_disposeBag.token.Token);
```

---

## Integration Notes

### Unity Integration
- All samples are MonoBehaviour-based for easy Unity integration
- Use `OnDestroy()` for cleanup in Unity lifecycle
- Samples include inspector-friendly serialized fields
- Console logging for debugging and learning

### Performance Considerations
- Events are processed synchronously for predictable behavior
- Use object pooling for frequently allocated event data
- DisposeBag provides efficient cleanup of multiple subscriptions
- State retention minimizes redundant state queries

### Debugging Tips
- Enable Unity console timestamps for event timing analysis
- Use descriptive event names and logging messages
- Event handlers are called immediately, making debugging straightforward
- The framework provides error handling for disposal operations

---

## Next Steps

After reviewing these samples:

1. **Start with Basic Usage** - Understand core concepts
2. **Explore Advanced Usage** - Learn complex patterns  
3. **Study Gameplay Examples** - See real-world applications
4. **Implement Your Own** - Apply patterns to your specific needs

For more information, see the main [README.md](../README.md) for complete API documentation and best practices.