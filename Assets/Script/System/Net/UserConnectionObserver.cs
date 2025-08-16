using System;

using Fusion;

public class UserConnectionObserver : RunnerCallbacksBase
{
    private Action<PlayerRef> joinedUserListener;

    private Action<PlayerRef> leftUserListener;


    public void SetJoinedUserEventListener(Action<PlayerRef> joinedEventListener)
    {
        joinedUserListener = joinedEventListener;
    }

    public void SetLeftUserEventListener(Action<PlayerRef> leftEventListener)
    {
        leftUserListener = leftEventListener;
    }

    protected override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        joinedUserListener?.Invoke(player);
    }

    protected override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        leftUserListener?.Invoke(player);
    }
}
