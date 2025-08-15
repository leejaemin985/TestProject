using UnityEngine;
using Fusion;
using System.Collections.Generic;

namespace InGame.Logic.Flow
{
    public class PhaseSequencer : NetworkBehaviour
    {
        //Net
        private Dictionary<PlayerRef, FlowPhase> userPhases;
        [Networked] private FlowPhase currentPhase { get; set; }

        //Local
        [SerializeField] private InGameClientPhaseAgent phaseAgentPrefab;
        private InGameClientPhaseAgent localAgent;


        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                userPhases = new();
                currentPhase = FlowPhase.Init;
            }

            localAgent = Instantiate(phaseAgentPrefab);
            localAgent.Initialize(RPC_ReportPhase);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ReportPhase(PhaseReport reportInfo)
        {
            if (!HasStateAuthority || GameNetworkManager.Instance.connectedUsers.Contains(reportInfo.userRef) == false) return;

            userPhases[reportInfo.userRef] = reportInfo.phase;
            CheckUsersPhase();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ApplyPhase(PhaseDirective directiveInfo)
        {
            localAgent?.ApplyPhase(directiveInfo);
        }

        private void CheckUsersPhase()
        {
            if (GameNetworkManager.Instance.connectedUsers.Count != userPhases.Count) return;

            foreach (var userPhase in userPhases.Values)
            {
                if (userPhase != currentPhase) return;
            }

            currentPhase = (FlowPhase)((int)currentPhase + 1);
            RPC_ApplyPhase(new()
            {
                phase = currentPhase
            });
        }
    }
}