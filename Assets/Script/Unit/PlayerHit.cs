using Physics;
using UnityEngine;

namespace Unit
{
    public class PlayerHit : MonoBehaviour
    {
        [SerializeField] private HitBox hitBox;

        public void Initialize()
        {
            hitBox.Initialize();
            hitBox.SetActive(true);
        }

        public PhysicsObject GetPhysicsBox() => hitBox;
    }
}