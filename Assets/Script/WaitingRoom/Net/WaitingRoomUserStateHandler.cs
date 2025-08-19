using System;
using System.Collections.Generic;
using Fusion;

namespace WaitingRoom.Net
{
    public class WaitingRoomUserStateHandler : NetworkBehaviour
    {
        private static Dictionary<PlayerRef, WaitingRoomUserStateHandler> spawnedHandlers = new();
        private static Dictionary<PlayerRef, Action<WaitingRoomUserStateHandler>> spawnedCallbacks = new();

        public static void AddSpawnedListener(PlayerRef userRef, Action<WaitingRoomUserStateHandler> spawnedListener)
        {
            if (spawnedListener == null) return;

            if (spawnedHandlers.TryGetValue(userRef, out var handler))
            {
                spawnedListener?.Invoke(handler);
                return;
            }

            if (spawnedCallbacks.ContainsKey(userRef) == false) spawnedCallbacks.Add(userRef, null);
            spawnedCallbacks[userRef] -= spawnedListener;
            spawnedCallbacks[userRef] += spawnedListener;
        }

        public static void RemoveSpanwedListener(PlayerRef userRef)
        {
            spawnedCallbacks.Remove(userRef);
        }

        [Networked, OnChangedRender(nameof(OnChangedReadyState))] public bool readyState { get; private set; }

        private Action<bool> onChangedReadyStateListener;

        public override void Spawned()
        {
            PlayerRef authority = Object.StateAuthority;

            spawnedHandlers[authority] = this;
            if (spawnedCallbacks.ContainsKey(authority) == true)
            {
                spawnedCallbacks[authority]?.Invoke(this);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            PlayerRef authority = Object.StateAuthority;

            spawnedHandlers.Remove(authority);
            spawnedCallbacks.Remove(authority);
        }

        public void AddChangedReadyStateListener(Action<bool> onChangedReadyStateListener)
        {
            this.onChangedReadyStateListener -= onChangedReadyStateListener;
            this.onChangedReadyStateListener += onChangedReadyStateListener;
        }

        private void OnChangedReadyState() => onChangedReadyStateListener?.Invoke(readyState);

        public void SetReadyState(bool set)
        {
            if (HasStateAuthority == false) return;

            readyState = set;
        }
    }
}