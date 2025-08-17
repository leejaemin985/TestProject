using System.Threading.Tasks;

using UnityEngine;
using Unit;
using System.Collections.Generic;

namespace InGame.Logic.Flow
{
    public class ClientPhaseSessionSpawn : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.SessionSpawn;

        [Header("Prefab")]
        [SerializeField] private PlayerRegistry PlayerRegistry;
        [SerializeField] private PhysicsEventHandler physicsEventHandler;
        [SerializeField] private EventDispatcher eventDispatcher;

        public override async Task OnEnter()
        {
            if (runner.IsSharedModeMasterClient)
                await EnsureAuthoritySingletonsAsync();
            else
                SetSpawnedCallbacks();
        }

        #region StateAuthority

        private async Task EnsureAuthoritySingletonsAsync()
        {
            List<Task> tasks = new()
        {
            SpawnPlayerRegisterAsync(),
            SpawnPhysicEventHandlerAsync(),
            SpawnEventDispatcherAsync()
        };

            await Task.WhenAll(tasks);
            phaseDoneListener?.Invoke();
        }

        private async Task SpawnPlayerRegisterAsync()
        => await runner.SpawnAsync(PlayerRegistry, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

        private async Task SpawnPhysicEventHandlerAsync()
            => await runner.SpawnAsync(physicsEventHandler, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

        private async Task SpawnEventDispatcherAsync()
            => await runner.SpawnAsync(eventDispatcher, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

        #endregion


        #region NonStateAuthority
        private void SetSpawnedCallbacks()
        {
            PlayerRegistry.AddSpawnedCallback(CheckSessionSpawn);
            EventDispatcher.AddSpawnedCallback(CheckSessionSpawn);
            PhysicsEventHandler.AddSpawnedCallback(CheckSessionSpawn);
        }

        private void CheckSessionSpawn()
        {
            if (PlayerRegistry.isInitialized &&
                EventDispatcher.isInitialized &&
                PhysicsEventHandler.isInitialized)
            {
                phaseDoneListener?.Invoke();
            }
        }
        #endregion

    }
}