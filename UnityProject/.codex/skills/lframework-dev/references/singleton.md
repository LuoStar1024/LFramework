# Singleton

Source paths:
- `Assets/GameScripts/GameLogic/Component/Singleton/SingletonComponent.cs`
- `Assets/GameScripts/GameLogic/Component/Singleton/Singleton.cs`
- `Assets/GameScripts/GameLogic/Component/Singleton/SingletonBehaviour.cs`
- `Assets/GameScripts/GameLogic/Component/Singleton/ISingleton*.cs`

`SingletonComponent` is a GameLogic module that registers `ISingletonManager` and manages both pure C# singletons and MonoBehaviour-backed singletons.

Responsibility:

- Register and release `ISingleton` instances.
- Track `ISingletonUpdate` instances and update them each frame.
- Track MonoBehaviour singleton GameObjects and destroy them on release.
- Provide lookup for managed singleton GameObjects.

Lifecycle:

- `Awake()` registers `ISingletonManager`.
- `OnUpdate()` calls `OnUpdate()` on registered `ISingletonUpdate` instances.
- `Shutdown()` releases all tracked singletons and GameObjects.

Usage:

- Use `Singleton<T>` for pure C# managers that do not need Transform, components, or Unity messages.
- Use `SingletonBehaviour<T>` when Unity object behavior is required.
- Register singletons with `GameEntry.Singleton` or through the base singleton implementation path.

Cleanup rules:

- Singleton `Release()` should free event subscriptions, timers, resources, and owner containers.
- Behaviour singletons should not manually destroy their tracked GameObject without notifying `SingletonComponent`.
- If a singleton implements `ISingletonUpdate`, verify it is removed from the update list during release.

Design guidance:

- Avoid using singletons for short-lived scene/UI state.
- Keep dependencies explicit; singletons can hide initialization-order issues.
