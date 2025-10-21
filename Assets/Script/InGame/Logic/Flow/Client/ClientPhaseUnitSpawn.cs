using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

using Fusion;

using Unit;

namespace InGame.Logic.Flow
{
    public class ClientPhaseUnitSpawn : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.UnitSpawn;

        [Header("Prefab")]
        [SerializeField] private UnitStat unitStatPrefab;
        [SerializeField] private Player playerPrefab;

        private CancellationTokenSource cts;
        private Task spawnTask;

        protected async override Task<PhaseState> OnPhase()
        {
            try
            {
                if (runner.IsSharedModeMasterClient) await SpawnUnitStat();
                await SpawnPlayer();

                return PhaseState.Wait;
            }
            catch (Exception e)
            {
                Debug.LogError(e);

                return PhaseState.Error;
            }
        }

        #region MasterClient

        private async Task SpawnUnitStat()
        {
            if (runner.IsSharedModeMasterClient)
            {
                var statSpawnTasks = new List<Task>();

                foreach (var userRef in GameNetworkManager.Instance.connectedUsers)
                {
                    statSpawnTasks.Add(SpawnUnitStat(userRef));
                }

                await Task.WhenAll(statSpawnTasks);
            }
        }

        private async Task SpawnUnitStat(PlayerRef userRef)
            => await runner.SpawnAsync(
                prefab: unitStatPrefab,
                inputAuthority: runner.LocalPlayer,
                onBeforeSpawned: (runner, obj) =>
                {
                    obj.GetComponent<UnitStat>().SetUserRef(userRef);
                });

        #endregion


        #region NonMasterClient

        private async Task SpawnPlayer()
        {
            var spawnPosContainter = FindObjectOfType<InGameUserSpawnPos>();
            var targetPos = spawnPosContainter.GetUserSpawnPos(runner.IsSharedModeMasterClient);

            var spawnedOb = await runner.SpawnAsync(
                prefab: playerPrefab,
                position: targetPos.Item1,
                rotation: targetPos.Item2,
                inputAuthority: runner.LocalPlayer);
        }

        #endregion

    }
}
