using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;
using Unit;
using Addressable;

namespace InGame.Logic.Flow
{
    public class ClientPhaseUnitSpawn : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.UnitSpawn;

        [Header("Prefab")]
        [SerializeField] private UnitStat unitStatPrefab;
        [SerializeField] private Player playerPrefab;

        public override async Task OnEnter()
        {
            if (runner.IsSharedModeMasterClient) await SpawnUnitStat();
            
            await SpawnPlayer();

            phaseDoneListener?.Invoke();
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
