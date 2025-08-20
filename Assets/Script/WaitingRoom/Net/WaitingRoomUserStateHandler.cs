using System;
using System.Collections.Generic;
using Fusion;

namespace WaitingRoom.Net
{
    public class WaitingRoomUserStateHandler : NetworkBehaviour
    {
        private static Dictionary<PlayerRef, WaitingRoomUserStateHandler> spawnedHandlers = new();
        private static Dictionary<PlayerRef, Action<WaitingRoomUserStateHandler>> spawnedCallbacks = new();

        public static void AddSpawnedCallback(PlayerRef userRef, Action<WaitingRoomUserStateHandler> callback)
        {
            if (spawnedHandlers.TryGetValue(userRef, out var handler))
            {
                callback?.Invoke(handler);
                return;
            }

            if (spawnedCallbacks.ContainsKey(userRef) == false) 
                spawnedCallbacks.Add(userRef, null);

            spawnedCallbacks[userRef] -= callback;
            spawnedCallbacks[userRef] += callback;
        }

        public static void RemoveSpawnedCallback(PlayerRef userRef)
        {
            spawnedHandlers.Remove(userRef);
        }

        public static void ClearAll()
        {
            spawnedHandlers.Clear();
            spawnedCallbacks.Clear();
        }

        [Networked, OnChangedRender(nameof(OnChangedReadyState))] public bool readyState { get; private set; }

        private Action<bool> onChangedReadyStateListener;


        public override void Spawned()
        {
            spawnedHandlers[Object.StateAuthority] = this;

            if (spawnedCallbacks.ContainsKey(Object.StateAuthority))
            {
                spawnedCallbacks[Object.StateAuthority]?.Invoke(this);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            spawnedHandlers.Remove(Object.StateAuthority);
            spawnedCallbacks.Remove(Object.StateAuthority);
        }

        public void AddChangedReadyStateListener(Action<bool> onChangedReadyStateListener)
        {
            this.onChangedReadyStateListener -= onChangedReadyStateListener;
            this.onChangedReadyStateListener += onChangedReadyStateListener;
        }

        private void OnChangedReadyState()
        {
            onChangedReadyStateListener?.Invoke(readyState);
        }

        public void SetReadyState(bool set)
        {
            if (HasStateAuthority == false) return;

            readyState = set;
        }
    }
}