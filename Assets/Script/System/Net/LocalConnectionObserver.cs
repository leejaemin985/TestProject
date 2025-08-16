using System;
using Fusion;

public class LocalConnectionObserver : RunnerCallbacksBase
{
    private Action<ShutdownReason> onEventShutDownListener;

    public void SetShutDownEventListener(Action<ShutdownReason> eventListener)
    {
        onEventShutDownListener = eventListener;
    }

    protected override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        onEventShutDownListener?.Invoke(shutdownReason);
    }
}
