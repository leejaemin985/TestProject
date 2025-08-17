using System;
using Fusion;

public abstract class SessionSingleton<T> : NetworkBehaviour where T : SessionSingleton<T>
{
    public static bool isInitialized { get; protected set; }

    private static T instance;

    public static T Instance => instance;

    private static Action spawnedCallback;

    public static void AddSpawnedCallback(Action spawnedListener)
    {
        spawnedCallback -= spawnedListener;
        spawnedCallback += spawnedListener;

        if (isInitialized) spawnedCallback?.Invoke();
    }

    public override void Spawned()
    {
        instance = (T)this;
        Initialize();

        spawnedCallback?.Invoke();
    }

    protected virtual void Initialize() => isInitialized = true;

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        isInitialized = false;

        instance = null;
        spawnedCallback = null;
    }
}