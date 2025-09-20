using System;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;

namespace Unit
{
    public abstract class Unit : NetworkBehaviour
    {
        [Header("Unit")]
        [SerializeField] private UnitStat unitStat;
        public UnitStat UnitStat => unitStat;
        
        public void BindUnitStat(UnitStat unitStat)
        {
            this.unitStat = unitStat;
        }

        protected async virtual void Initialize() => await Task.CompletedTask;

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