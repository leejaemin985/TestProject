using System.Collections.Generic;

using UnityEngine;

using Fusion;

using SceneType;

namespace InGame.Logic.Flow
{
    public class PhaseSequencer : NetworkBehaviour
    {
        private class UserPhaseState
        {
            public PlayerRef userRef;

            public FlowPhase phase;

            public bool isDone;
        }

        //Net
        private Dictionary<PlayerRef, UserPhaseState> userPhases;
        [Networked] private FlowPhase currentPhase { get; set; }

        //Local
        [SerializeField] private InGameClientPhaseAgent phaseAgentPrefab;
        private InGameClientPhaseAgent localAgent;


        [SerializeField] private FlowPhaseBase[] phaseArray;
        private IFlowPhase currentFlowPhase;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                userPhases = new();
                currentPhase = FlowPhase.Init;
            }

            localAgent = Instantiate(phaseAgentPrefab);
            localAgent.Initialize(RPC_ReportPhase, RPC_PhaseDone);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ReportPhase(PhaseReport reportInfo)
        {
            if (!HasStateAuthority || GameNetworkManager.Instance.connectedUsers.Contains(reportInfo.userRef) == false) return;

            if (userPhases.ContainsKey(reportInfo.userRef) == false)
                userPhases.Add(reportInfo.userRef, new());

            var phaseState = userPhases[reportInfo.userRef];
            phaseState.userRef = reportInfo.userRef;
            phaseState.phase = reportInfo.phase;              
            phaseState.isDone = false;

            Debug.Log($"PhaseSquencer - Report {reportInfo.userRef} - {reportInfo.phase}");
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_PhaseDone(PlayerRef userRef)
        {
            if (!HasStateAuthority || GameNetworkManager.Instance.connectedUsers.Contains(userRef) == false) return;

            if (userPhases.ContainsKey(userRef) == false)
                userPhases.Add(userRef, new());

            var phaseState = userPhases[userRef];
            phaseState.isDone = true;

            Debug.Log($"PhaseSequencer - Done {userRef} - {phaseState.phase}");
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
                if (currentPhase != phaseState.phase || phaseState.isDone == false) return;
            }

            if (currentPhase < FlowPhase.End)
            {
                currentPhase = currentPhase + 1;
                RPC_ApplyPhase(new()
                {
                    phase = currentPhase,
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