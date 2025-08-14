using UnityEngine;
using Fusion;
using Unity.VisualScripting;

namespace Unit
{
    public abstract class Unit : NetworkBehaviour
    {
        [Header("Unit")]
        [SerializeField] protected UnitStatus status;

        protected abstract float MaxHP { get; }

        protected virtual void Initialize()
        {
            status.Initialize(MaxHP);
            status.hpEventListener = (current, max)=>
            {
                if (HasStateAuthority) RPC_OnHpEvent(current, max);
            };
        }

        public float GetHp() => status.hp;

        public void OnDamaged(float damage)
        {
            if (damage < 0) return;
            status.hp = Mathf.Clamp(status.hp - damage, 0, MaxHP);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_OnHpEvent(float currentHp, float maxHp)
        {
            OnHpEvent(currentHp, maxHp);
        }

        protected virtual void OnHpEvent(float currentHp, float maxHp) { }

    }
}