using Fusion;

namespace InGame.Logic
{
    public enum Side : byte
    {
        A = 0,
        B = 1
    }

    public class FlowController : NetworkBehaviour
    {
        [Networked] private FlowPhase currentFlowPhase { get; set; }

        [Networked] private byte barrierA { get; set; }
        [Networked] private byte barrierB { get; set; }

        [Networked] private TickTimer phaseTimer { get; set; }


        public Side localSide => Runner.IsSharedModeMasterClient ? Side.A : Side.B;

        public void ReportReady(ReadyFlags flags)
        {

        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ReportReady(byte flags)
        {
            if (!Runner.IsSharedModeMasterClient) return;


        }


    }
}