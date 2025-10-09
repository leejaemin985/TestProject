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

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                userPhases = new();
                foreach (PlayerRef userRef in GameNetworkManager.Instance.connectedUsers) userPhases.Add(userRef, new());

                currentPhase = FlowPhase.Init;
            }

            localAgent = Instantiate(phaseAgentPrefab);
            localAgent.Initialize(RPC_ReportPhase, RPC_PhaseDone);
        }

        private bool IsValidUser(PlayerRef userRef)
        {
            return GameNetworkManager.Instance.connectedUsers.Contains(userRef) && userPhases.ContainsKey(userRef);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ReportPhase(PhaseReport reportInfo)
        {
            if (!HasStateAuthority || IsValidUser(reportInfo.userRef) == false) return;

            var phaseState = userPhases[reportInfo.userRef];
            phaseState.userRef = reportInfo.userRef;
            phaseState.phase = reportInfo.phase;
            phaseState.isDone = false;

        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_PhaseDone(PlayerRef userRef)
        {
            if (!HasStateAuthority || IsValidUser(userRef) == false) return;

            var phaseState = userPhases[userRef];
            phaseState.isDone = true;

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