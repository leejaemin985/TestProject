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

        private bool spawnedComplete = false;
        private bool spawnedError = false;

        protected override Task<PhaseReport> OnEnter(PhaseDirective phaseDirective)
        {
            SpawnedTask();

            return Task.FromResult(CreatePhaseReport(PhaseState.Init));
        }

        protected override PhaseState OnTick(float deltaTime)
        {
            if (spawnedError) return PhaseState.Error;

            if (!spawnedComplete) 
                return PhaseState.Run;
            else 
                return PhaseState.Wait;
        }

        private async void SpawnedTask()
        {
            try
            {
                if (runner.IsSharedModeMasterClient) await SpawnUnitStat();
                await SpawnPlayer();

                //Task statSpawnTask = runner.IsSharedModeMasterClient ? SpawnUnitStat() : Task.CompletedTask;
                //Task playerSpawnTask = SpawnPlayer();
                //
                //await Task.WhenAll(statSpawnTask, playerSpawnTask);
                spawnedComplete = true;
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                spawnedError = true;
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
