using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace InGame.Logic.Flow
{
    public class ClientPhaseUnitSpawn : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.UnitSpawn;

        [Header("Prefab")]
        [SerializeField] private UnitStat unitStatPrefab;
        [SerializeField] private Player playerPrefab;

        private Player spawnedPlayer;

        public override async Task OnEnter()
        {
            if (runner.IsSharedModeMasterClient) await SpawnUnitStat();
            
            await SpawnPlayer();
        }


        #region StateAuthority

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

        private async Task SpawnPlayer()
        {
            var spawnedOb = await runner.SpawnAsync(
                prefab: playerPrefab,
                position: Vector3.up,
                rotation: Quaternion.identity,
                inputAuthority: runner.LocalPlayer);

            spawnedPlayer = spawnedOb.GetComponent<Player>();
        }

        #endregion


        #region NonStateAuthority

        private void BindUnitStat(UnitStat stat)
        {
            spawnedPlayer.BindUnitStat(stat);
        }


        #endregion

    }
}
