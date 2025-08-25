using System;
using System.Collections.Generic;
using Fusion;

namespace WaitingRoom.Net
{
    public class WaitingRoomUserStateHandler : NetworkBehaviour
    {
        #region Static Spawned Callback

        private static Dictionary<PlayerRef, WaitingRoomUserStateHandler> spawnedHandlers = new();
        private static Dictionary<PlayerRef, Action<WaitingRoomUserStateHandler>> spawnedCallbacks = new();

        public static void AddSpawnedCallback(PlayerRef userRef, Action<WaitingRoomUserStateHandler> callback)
        {
            if (callback == null) return;

            if (spawnedHandlers.TryGetValue(userRef, out var handler))
            {
                callback?.Invoke(handler);

                if (spawnedCallbacks.TryGetValue(userRef, out var waiting))
                {
                    spawnedCallbacks.Remove(userRef);
                    waiting.Invoke(handler);
                }

                return;
            }

            if (spawnedCallbacks.ContainsKey(userRef) == false) 
                spawnedCallbacks.Add(userRef, null);

            spawnedCallbacks[userRef] -= callback;
            spawnedCallbacks[userRef] += callback;
        }

        public static void RemoveSpawnedCallback(PlayerRef userRef, Action<WaitingRoomUserStateHandler> callback)
        {
            if (callback == null) return;

            if (spawnedCallbacks.TryGetValue(userRef, out var del))
            {
                del -= callback;
                if (del == null) spawnedCallbacks.Remove(userRef);
                else spawnedCallbacks[userRef] = del;
            }
        }

        public static void ClearAll()
        {
            spawnedHandlers.Clear();
            spawnedCallbacks.Clear();
        }

        #endregion

        [Networked] public bool readyState { get; private set; }

        public override void Spawned()
        {
            spawnedHandlers[Object.StateAuthority] = this;

            if (spawnedCallbacks.TryGetValue(Object.StateAuthority, out var callbacks))
            {
                spawnedCallbacks.Remove(Object.StateAuthority);
                callbacks?.Invoke(this);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            spawnedHandlers.Remove(Object.StateAuthority);
            spawnedCallbacks.Remove(Object.StateAuthority);
        }

        public void SetReadyState(bool set)
        {
            if (HasStateAuthority == false) return;

            readyState = set;
        }
    }
}