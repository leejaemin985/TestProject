using System;
using UnityEngine;
using Fusion;

namespace Unit
{
    public abstract class Unit : NetworkBehaviour
    {
        [Header("Unit")]
        [SerializeField] private UnitStat unitStat;

        private Action<float, float> onChangedHpEvent;
        private Action<float, float> onChangedPostureEvent;

        public void BindUnitStat(UnitStat unitStat)
        {
            this.unitStat = unitStat;
        }

        protected virtual void Initialize() { }

        public bool isAlive() => unitStat.hp > 0;

        public float GetHp() => unitStat.hp;

        public float GetPosture() =>unitStat.posture;

        protected void OnDamaged(float damage)
        {
            if (damage < 0) return;
            unitStat.SetHp(unitStat.hp - damage);
        }

        protected void OnDecreasePosture(float value)
        {
            if (value < 0) return;
            unitStat.SetPosture(unitStat.posture - value);
        }

        protected void OnIncreasePosture(float value)
        {
            if (value < 0) return;
            unitStat.SetPosture(unitStat.posture + value);
        }
    }
}