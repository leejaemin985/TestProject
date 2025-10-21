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

        private IReadOnlyList<FlowPhase> InGameFlowPhase = new List<FlowPhase>()
        {
            //FlowPhase.Init,
            FlowPhase.SessionInit,
            FlowPhase.UnitSpawn,
            FlowPhase.InBattle,
            FlowPhase.End
        };
        private int currentPhaseIndex;

        //Net
        private Dictionary<PlayerRef, UserPhaseState> userPhases;
        [Networked] private FlowPhase currentPhase { get; set; }

        //Local
        [SerializeField] private InGameClientPhaseAgent phaseAgentPrefab;
        private InGameClientPhaseAgent localAgent;

        public async override void Spawned()
        {
            if (HasStateAuthority)
            {
                userPhases = new();
                foreach (PlayerRef userRef in GameNetworkManager.Instance.connectedUsers) userPhases.Add(userRef, new() { phaseType = FlowPhase.None });

                currentPhase = FlowPhase.Init;
            }

            var localAgentOb = await Runner.SpawnAsync(prefab: phaseAgentPrefab);
            localAgent = localAgentOb.GetComponent<InGameClientPhaseAgent>();
            await localAgent.Initialize(RPC_ReportPhase);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ReportPhase(PhaseReport reportInfo)
        {
            if (!HasStateAuthority || reportInfo.IsValid == false) return;

            var phaseState = userPhases[reportInfo.userRef];
            phaseState.userRef = reportInfo.userRef;
            phaseState.phaseType = reportInfo.phaseType;
            phaseState.phaseState = reportInfo.phaseState;

            userPhases[reportInfo.userRef] = phaseState;

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
                if (currentPhase != phaseState.phaseType || phaseState.phaseState != PhaseState.Complete) return;
            }

            if (currentPhase < FlowPhase.End)
            {
                currentPhase = InGameFlowPhase[currentPhaseIndex + 1];
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