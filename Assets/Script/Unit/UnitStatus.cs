using Fusion;

namespace Unit
{
    public class UnitStatus : NetworkBehaviour
    {
        [Networked] public float hp { get; set; }

        public void Initialize(float maxHp)
        {
            this.hp = maxHp;
        }
    }
}