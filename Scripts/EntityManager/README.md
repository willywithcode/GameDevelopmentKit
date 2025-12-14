# EntityManager Documentation

## Overview

The EntityManager is an **ECS-Lite (Entity Component System)** implementation that combines object pooling with a component-based architecture. It provides:

- **Object Pooling**: Efficient entity reuse without GC allocations
- **Component System**: Struct-based components for cache-friendly data storage
- **Lifecycle Management**: Async hooks for entity state changes
- **VContainer Integration**: Full dependency injection support

---

## Architecture

```
+------------------------------------------------------------------+
|                        EntityManager                              |
+------------------------------------------------------------------+
|  +--------------+    +--------------+    +-------------------+   |
|  |    Pools     |    |   Entities   |    |  ComponentArrays  |   |
|  | (GameObject) |    |  (IEntity)   |    |   (Struct Data)   |   |
|  +--------------+    +--------------+    +-------------------+   |
+------------------------------------------------------------------+
                              |
                              v
+------------------------------------------------------------------+
|                         EntityBase                                |
|  +-----------+  +-----------+  +-------------------------------+ |
|  | Id, Key   |  | Lifecycle |  | Local Components Dictionary   | |
|  | IsActive  |  |   Hooks   |  | (Per-Entity Component Storage)| |
|  +-----------+  +-----------+  +-------------------------------+ |
+------------------------------------------------------------------+
```

---

## Setup

### 1. Register in VContainer

```csharp
// In your LifetimeScope (e.g., GameLifeTimeScope.cs)
using GameFoundation.Scripts.EntityManager.DI;

protected override void Configure(IContainerBuilder builder)
{
    builder.RegisterEntityManager();  // Add this line
}
```

### 2. Create Addressable Prefab

Your entity prefab must:
- Be registered in Addressables with a **key** (e.g., `"snake_segment"`)
- Have a component that extends `EntityBase`

---

## Creating an Entity

### Step 1: Define Components (Optional)

```csharp
using GameFoundation.Scripts.EntityManager.Core;

// Components are structs - no GC allocations!
public struct HealthComponent : IComponent
{
    public int Current;
    public int Max;
}

public struct MovementComponent : IComponent
{
    public Vector3 Velocity;
    public float   Speed;
}

public struct SnakeSegmentComponent : IComponent
{
    public int  SegmentIndex;
    public bool IsHead;
}
```

### Step 2: Create Entity Class

```csharp
using GameFoundation.Scripts.EntityManager.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class SnakeSegmentEntity : EntityBase
{
    // Inject dependencies via VContainer
    [Inject] private IEntityManager entityManager;

    // Unity references
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Called once when entity is created (pooled)
    public override UniTask OnInstantiate()
    {
        // Initialize default components
        this.AddComponent(new HealthComponent { Current = 100, Max = 100 });
        this.AddComponent(new MovementComponent { Speed = 5f });
        return UniTask.CompletedTask;
    }

    // Called each time entity is spawned from pool
    public override UniTask OnSpawn()
    {
        // Reset state
        ref var health = ref this.GetComponent<HealthComponent>();
        health.Current = health.Max;

        spriteRenderer.color = Color.white;
        return UniTask.CompletedTask;
    }

    // Called each time entity is returned to pool
    public override UniTask OnDespawn()
    {
        // Cleanup
        ref var movement = ref this.GetComponent<MovementComponent>();
        movement.Velocity = Vector3.zero;
        return UniTask.CompletedTask;
    }

    // Called every frame for active entities
    public override UniTask OnTick(float deltaTime)
    {
        // Update logic
        if (this.HasComponent<MovementComponent>())
        {
            ref var movement = ref this.GetComponent<MovementComponent>();
            this.tf.position += movement.Velocity * deltaTime;
        }
        return UniTask.CompletedTask;
    }
}
```

---

## Using EntityManager

### Inject the Manager

```csharp
using GameFoundation.Scripts.EntityManager.Core;
using VContainer;

public class GameplayService
{
    private readonly IEntityManager entityManager;

    [Inject]
    public GameplayService(IEntityManager entityManager)
    {
        this.entityManager = entityManager;
    }
}
```

### Spawn Entities

```csharp
// Basic spawn (uses Addressable key)
var segment = entityManager.Spawn<SnakeSegmentEntity>("snake_segment");

// Spawn with position and rotation
var segment = entityManager.Spawn<SnakeSegmentEntity>(
    "snake_segment",
    new Vector3(0, 0, 0),
    Quaternion.identity
);

// Configure after spawn
segment.AddComponent(new SnakeSegmentComponent
{
    SegmentIndex = 0,
    IsHead = true
});
```

### Despawn Entities

```csharp
// Despawn single entity (returns to pool)
entityManager.Despawn(segment);

// Despawn all entities of a type
entityManager.DespawnAll<SnakeSegmentEntity>();
```

### Query Entities

```csharp
// Get all active entities by key
List<SnakeSegmentEntity> segments = entityManager.GetAll<SnakeSegmentEntity>("snake_segment");

// Get entity by ID (O(1) lookup)
IEntity entity = entityManager.GetById(entityId);

// Get all entities with a specific component
List<IEntity> movableEntities = entityManager.GetEntitiesWithComponent<MovementComponent>();

// Check if pool exists
bool exists = entityManager.IsInitialized("snake_segment");
```

---

## Working with Components

### Entity-Level Components (Per-Entity Storage)

```csharp
// Add component to entity
entity.AddComponent(new HealthComponent { Current = 100, Max = 100 });

// Get component (returns ref for zero-copy modification)
ref var health = ref entity.GetComponent<HealthComponent>();
health.Current -= 10;  // Directly modifies the struct

// Check if entity has component
if (entity.HasComponent<HealthComponent>())
{
    // ...
}

// Remove component
entity.RemoveComponent<HealthComponent>();
```

### Manager-Level Components (Global Contiguous Storage)

For high-performance scenarios, use the EntityManager's global component arrays:

```csharp
// Cast to concrete type for advanced features
var manager = (EntityManager)entityManager;

// Add to global array (better cache performance for systems)
manager.AddComponent(entity.Id, new MovementComponent { Speed = 5f });

// Get from global array
ref var movement = ref manager.GetComponent<MovementComponent>(entity.Id);

// Iterate all components of a type (cache-friendly)
var movementArray = manager.GetComponentArray<MovementComponent>();
foreach (ref var mov in movementArray)
{
    mov.Velocity += Vector3.down * 9.8f * deltaTime;  // Apply gravity
}
```

---

## Lifecycle Hooks

| Hook | When Called | Use Case |
|------|-------------|----------|
| `OnInstantiate()` | Once, when entity is first created | Initialize default components, cache references |
| `OnSpawn()` | Each time entity is activated from pool | Reset state, enable visuals |
| `OnDespawn()` | Each time entity is returned to pool | Cleanup, disable effects |
| `OnTick(deltaTime)` | Every frame for active entities | Update logic, movement, AI |

### Lifecycle Flow

```
[Prefab]
    |
    v Instantiate (first time)
[OnInstantiate] -----------------------+
    |                                  |
    v SetActive(false)                 |
[Pool] <-------------------------------+
    |                                  |
    v Spawn()                          |
[OnSpawn]                              |
    |                                  |
    v SetActive(true)                  |
[Active] <-----------------+           |
    |                      |           |
    v OnTick() per frame   |           |
[Running] -----------------+           |
    |                                  |
    v Despawn()                        |
[OnDespawn] ---------------------------+
    |
    v SetActive(false)
[Pool] (ready for reuse)
```

---

## Performance Tips

### 1. Use Struct Components

```csharp
// GOOD - No GC allocation
public struct HealthComponent : IComponent { public int Current; }

// BAD - Causes GC allocation
public class HealthComponent : IComponent { public int Current; }
```

### 2. Use `ref` Returns

```csharp
// GOOD - Zero-copy, modifies original
ref var health = ref entity.GetComponent<HealthComponent>();
health.Current -= 10;

// BAD - Copies struct, changes lost
var health = entity.GetComponent<HealthComponent>();
health.Current -= 10;  // This change is lost!
```

### 3. Cache Entity References

```csharp
// GOOD - Cache the reference
private SnakeSegmentEntity headSegment;
headSegment = entityManager.Spawn<SnakeSegmentEntity>("snake_segment");

// BAD - Query every frame
void Update()
{
    var head = entityManager.GetAll<SnakeSegmentEntity>("snake_segment")[0];  // Allocates list!
}
```

### 4. Use Global ComponentArray for Systems

```csharp
// GOOD - Cache-friendly iteration
var movements = manager.GetComponentArray<MovementComponent>();
foreach (ref var mov in movements)
{
    // Process all movement components contiguously
}

// BAD - Random memory access
foreach (var entity in entityManager.GetEntitiesWithComponent<MovementComponent>())
{
    ref var mov = ref entity.GetComponent<MovementComponent>();
}
```

---

## Complete Example: Snake Game

```csharp
public class SnakeGameService
{
    private readonly IEntityManager entityManager;
    private readonly List<SnakeSegmentEntity> snakeSegments = new();

    [Inject]
    public SnakeGameService(IEntityManager entityManager)
    {
        this.entityManager = entityManager;
    }

    public void StartGame()
    {
        // Spawn head
        var head = entityManager.Spawn<SnakeSegmentEntity>(
            "snake_segment",
            Vector3.zero,
            Quaternion.identity
        );
        head.AddComponent(new SnakeSegmentComponent { SegmentIndex = 0, IsHead = true });
        head.AddComponent(new MovementComponent { Speed = 5f, Velocity = Vector3.right });
        snakeSegments.Add(head);

        // Spawn initial body
        for (int i = 1; i < 3; i++)
        {
            AddSegment();
        }
    }

    public void AddSegment()
    {
        var lastSegment = snakeSegments[^1];
        var newSegment = entityManager.Spawn<SnakeSegmentEntity>(
            "snake_segment",
            lastSegment.tf.position - Vector3.right,
            Quaternion.identity
        );
        newSegment.AddComponent(new SnakeSegmentComponent
        {
            SegmentIndex = snakeSegments.Count,
            IsHead = false
        });
        snakeSegments.Add(newSegment);
    }

    public void GameOver()
    {
        foreach (var segment in snakeSegments)
        {
            entityManager.Despawn(segment);
        }
        snakeSegments.Clear();
    }
}
```

---

## API Reference

### IEntityManager

| Method | Description |
|--------|-------------|
| `Spawn<T>(key)` | Spawn entity from pool by Addressable key |
| `Spawn<T>(key, position, rotation)` | Spawn entity at specific transform |
| `Despawn(entity)` | Return entity to pool |
| `DespawnAll<T>()` | Despawn all entities of type |
| `GetAll<T>(key)` | Get all active entities by key |
| `GetById(id)` | Get entity by unique ID |
| `GetEntitiesWithComponent<T>()` | Get all entities with component |
| `IsInitialized(key)` | Check if pool exists |
| `Tick(deltaTime)` | Update all active entities |

### IEntity

| Property/Method | Description |
|-----------------|-------------|
| `Id` | Unique entity identifier |
| `Key` | Addressable key used to spawn |
| `IsActive` | Whether entity is currently active |
| `tf` | Transform shortcut |
| `AddComponent<T>(component)` | Add struct component |
| `GetComponent<T>()` | Get component by ref |
| `HasComponent<T>()` | Check component existence |
| `RemoveComponent<T>()` | Remove component |

### ComponentArray<T>

| Method | Description |
|--------|-------------|
| `Add(entityId, component)` | Add or update component |
| `Get(entityId)` | Get by ref (throws if missing) |
| `TryGet(entityId, out component)` | Safe get |
| `Has(entityId)` | Check existence |
| `Remove(entityId)` | Remove (swap-remove O(1)) |
| `AsSpan()` | Get ReadOnlySpan for bulk access |
| `Count` | Number of components |

---

## File Structure

```
Assets/GDK/Scripts/EntityManager/
├── Core/
│   ├── IEntity.cs           # Entity interface
│   ├── IComponent.cs        # Component marker interface
│   ├── IEntityManager.cs    # Manager interface
│   ├── EntityManager.cs     # Main implementation
│   ├── EntityBase.cs        # Base MonoBehaviour for entities
│   └── ComponentArray.cs    # Contiguous component storage
├── DI/
│   └── EntityVContainer.cs  # VContainer registration
└── README.md                # This documentation
```
