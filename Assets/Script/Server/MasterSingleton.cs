using Fusion;

public abstract class MasterSingleton<T> : NetworkBehaviour where T : MasterSingleton<T>
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