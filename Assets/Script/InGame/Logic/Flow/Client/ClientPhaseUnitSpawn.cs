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

        protected override Task<PhaseState> OnEnter(PhaseDirective phaseDirective)
        {
            SpawnedTask();
            return Task.FromResult(PhaseState.Init);
            SpawnedTask 먼저 도는 경우가 있음. OnTick처럼 매 프레임 도는건 아니지만 한번 실행한 Task정도는 필요.
        }

        private async void SpawnedTask()
        {
            try
            {
                if (runner.IsSharedModeMasterClient) await SpawnUnitStat();
                await SpawnPlayer();

                reportPhase?.Invoke(PhaseState.Wait);
                //Task statSpawnTask = runner.IsSharedModeMasterClient ? SpawnUnitStat() : Task.CompletedTask;
                //Task playerSpawnTask = SpawnPlayer();
                //
                //await Task.WhenAll(statSpawnTask, playerSpawnTask);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                reportPhase?.Invoke(PhaseState.Error);
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
