using System.Threading.Tasks;
using UnityEngine;
using Fusion;
using System.Collections.Generic;

namespace InGame.Logic
{
    public enum MatchPhase : byte { Init, Countdown, Playing }

    [System.Flags]
    public enum ReadyFlags : byte
    {
        None = 0,
        SessionSpawn = 1 << 1,
        PlayerSpawn = 1 << 2
    }

    public class MatchReadyBarrier : NetworkBehaviour
    {
        [SerializeField] private SessionSingletonSpawner sessionSingletonSpawner = default;

        [SerializeField] private PlayerSpawner playerSpawner = default;

        [Networked] private bool initialized { get; set; }

        [Networked] public MatchPhase phase { get; private set; }

        private Dictionary<PlayerRef, ReadyFlags> userReadyFlags = default;

        private const int WARMUP_SEC = 10;

        public async override void Spawned()
        {
            await SpawnAsync();
        }

        private async Task SpawnAsync()
        {
            if (HasStateAuthority)
            {
                phase = MatchPhase.Init;

                userReadyFlags = new();
                foreach (var userRef in GameNetworkManager.Instance.connectedUsers)
                    userReadyFlags.Add(userRef, ReadyFlags.None);

                initialized = true;
            }

            const int CHECK_DELAY = 100;
            while (initialized == false) await Task.Delay(CHECK_DELAY);



            await SpawnSessionSingleton();

            await SpawnPlayer();
        }

        private async Task SpawnSessionSingleton()
        {
            await sessionSingletonSpawner.SpawnAsync();
            RPC_ReportReady(Runner.LocalPlayer, ReadyFlags.SessionSpawn);
        }

        private async Task SpawnPlayer()
        {
            await playerSpawner.SpawnAsync();
            RPC_ReportReady(Runner.LocalPlayer, ReadyFlags.PlayerSpawn);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ReportReady(PlayerRef userRef, ReadyFlags readyFlags)
        {
            if (!HasStateAuthority) return;
            if (!GameNetworkManager.Instance.connectedUsers.Contains(userRef)) return;

            userReadyFlags[userRef] = readyFlags;
            Debug.Log($"Test - {userRef} - {readyFlags}");

            CheckUsersReadyFlag();
        }

        private void CheckUsersReadyFlag()
        {
            foreach (var userRef in userReadyFlags.Keys)
            {
                if (userReadyFlags[userRef] != ReadyFlags.PlayerSpawn)
                    return;
            }

            Debug.Log($"Test - Start Battle");
        }
    }

}