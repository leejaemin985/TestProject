using Fusion;

public abstract class SessionSingleton<T> : NetworkBehaviour where T : SessionSingleton<T>
{
    protected static bool HasInstance => instance != null;

    private static T instance;

    public static T Instance => instance;


    public virtual bool Initialized { get; protected set; }

    public override void Spawned()
    {
        instance = (T)this;
        Initialize();

        Initialized = HasInstance;
    }

    protected virtual void Initialize() { }
}