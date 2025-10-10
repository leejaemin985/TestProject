using System.Collections.Generic;

using UnityEngine;

using Fusion;

using SceneType;

namespace InGame.Logic.Flow
{
    public class PhaseSequencer : NetworkBehaviour
    {
        private struct UserPhaseState : INetworkStruct
        {
            public PlayerRef userRef;

            public FlowPhase phaseType;

            public PhaseState phaseState;
        }

        //Net
        private Dictionary<PlayerRef, UserPhaseState> userPhases;
        [Networked] private FlowPhase currentPhase { get; set; }

        //Local
        [SerializeField] private InGameClientPhaseAgent phaseAgentPrefab;
        private InGameClientPhaseAgent localAgent;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                userPhases = new();
                foreach (PlayerRef userRef in GameNetworkManager.Instance.connectedUsers) userPhases.Add(userRef, new());

                currentPhase = FlowPhase.Init;
            }

            Runner.SpawnAsync(prefab: phaseAgentPrefab);
            localAgent.Initialize(RPC_ReportPhase);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ReportPhase(PhaseReport reportInfo)
        {
            if (!HasStateAuthority || reportInfo.IsValid == false) return;

            var phaseState = userPhases[reportInfo.userRef];
            phaseState.userRef = reportInfo.userRef;
            phaseState.phaseType = reportInfo.phaseType;
            phaseState.phaseState = reportInfo.phaseState;

            CheckCanEnterNextPhase();
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ApplyPhase(PhaseDirective directiveInfo)
        {
            localAgent?.ApplyPhase(directiveInfo);
        }


        private void CheckCanEnterNextPhase()
        {
            if (GameNetworkManager.Instance.connectedUsers.Count != userPhases.Count) return;

            foreach (var phaseState in userPhases.Values)
            {
                if (currentPhase != phaseState.phaseType || phaseState.phaseState == PhaseState.Wait) return;
            }

            if (currentPhase < FlowPhase.End)
            {
                currentPhase = currentPhase + 1;
                RPC_ApplyPhase(new()
                {
                    phaseType = currentPhase,
                    startTick = Runner.Tick
                });
            }
            else
            {
                Runner.LoadScene(NetScene.WaitingRoom.sceneRef, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
    }
}