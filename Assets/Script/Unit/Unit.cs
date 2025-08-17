using UnityEngine;
using Fusion;
using Unity.VisualScripting;

namespace Unit
{
    public abstract class Unit : NetworkBehaviour
    {
        [Header("Unit")]
        [SerializeField] private UnitStat unitStat;

        public void BindUnitStat(UnitStat unitStat)
        {
            this.unitStat = unitStat;
        }

        protected virtual void Initialize()
        {

        }

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