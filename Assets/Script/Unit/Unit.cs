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
        }

        public float GetHp() => status.hp;

        public void OnDamaged(float damage)
        {
            if (damage < 0) return;
            status.hp = Mathf.Clamp(status.hp - damage, 0, MaxHP);
        }
    }
}