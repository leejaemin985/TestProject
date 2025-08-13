using Fusion;
using System;

namespace Unit
{
    public class UnitStatus : NetworkBehaviour
    {
        public float maxHp { get; set; }
        [Networked, OnChangedRender(nameof(OnHpEvent))] public float hp { get; set; }

        public Action<float, float> hpEventListener { get; set; }

        public void Initialize(float maxHp)
        {
            if (!HasStateAuthority) return;
            this.maxHp = maxHp;
            this.hp = maxHp;
        }

        private void OnHpEvent() => hpEventListener?.Invoke(hp, maxHp);

    }
}