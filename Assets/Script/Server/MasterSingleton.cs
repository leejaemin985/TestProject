using Fusion;

public abstract class MasterSingleton<T> : NetworkBehaviour where T : MasterSingleton<T>, IMasterSingleton
{
    protected static bool HasInstance => instance != null;

    private static T instance;

    public static T Instance => instance;

    public override void Spawned()
    {
        instance = (T)this;
    }

    protected virtual void Initialize() { }
}