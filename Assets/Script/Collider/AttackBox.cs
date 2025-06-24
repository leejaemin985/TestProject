using System;
using System.Collections.Generic;
using Unit;

namespace Physics
{
    public class AttackBox : PhysicsObject
    {
        public override PhysicsType physicsType => PhysicsType.ATTACK;

        public List<Guid> checkedHitableUIDs { get; private set; }

        private Action<HitInfo> hitEvent;

        public override void Initialize(Action<HitInfo> hitEvent = null)
        {
            base.Initialize(hitEvent);

            checkedHitableUIDs = new();
            this.hitEvent = hitEvent;
        }

        public void OnCollisionEvent(HitInfo hitInfo)
        {
            this.hitEvent?.Invoke(hitInfo);
            checkedHitableUIDs.Add(hitInfo.hitObject.uid);
        }
    }
}