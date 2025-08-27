using Fusion;

namespace Unit
{
    public class PlayerInteractionEventHandler : NetworkBehaviour
    {
        private bool HasEventAuthority => Runner.IsSharedModeMasterClient;

        public void RequestOnHitUser(PlayerRef userRef, HitInfo hitInfo)
        {
            if (HasEventAuthority == false) return;
            RPC_RequestOnHitUser(userRef, hitInfo);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_RequestOnHitUser(PlayerRef userRef, HitInfo hitInfo)
        {
            if (Player.RegistedUsers.TryGetValue(userRef, out Player user))
                user.RequestOnHitState(hitInfo);
        }


        public void RequestOnParringUser(PlayerRef userRef, HitInfo hitInfo)
        {
            if (HasEventAuthority == false) return;
            RPC_RequestOnParringUser(userRef, hitInfo);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_RequestOnParringUser(PlayerRef userRef, HitInfo hitInfo)
        {
            if (Player.RegistedUsers.TryGetValue(userRef, out Player user))
                user.RequestOnParringState(hitInfo);
        }


        public void RequestOnDiedUser(PlayerRef userRef, HitInfo hitInfo)
        {
            if (HasEventAuthority == false) return;
            RPC_RequestOnDiedUser(userRef, hitInfo);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_RequestOnDiedUser(PlayerRef userRef, HitInfo hitInfo)
        {
            if (Player.RegistedUsers.TryGetValue(userRef, out Player user))
                user.RequestOnDiedState(hitInfo);
        }
    }
}