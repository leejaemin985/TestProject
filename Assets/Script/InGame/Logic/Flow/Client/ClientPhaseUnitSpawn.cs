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
            bool odd = (runner.LocalPlayer.AsIndex % 2) == 1;

            Vector3 spawnPos = Vector3.forward * (odd ? 5 : -5);
            Quaternion spawnRot = Quaternion.Euler(new Vector3(0, odd ? 180 : 0, 0));

            var spawnedOb = await runner.SpawnAsync(
                prefab: playerPrefab,
                position: spawnPos,
                rotation: spawnRot,
                inputAuthority: runner.LocalPlayer);

            
            spawnedPlayer = spawnedOb.GetComponent<Player>();
        }

        #endregion

    }
}
