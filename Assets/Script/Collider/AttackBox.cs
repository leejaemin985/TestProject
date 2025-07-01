using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unit;

namespace Physics
{
    public class AttackBox : PhysicsObject
    {
        public override PhysicsType physicsType => PhysicsType.ATTACK;

        public Guid ignoreUid { get; private set; }
        public List<Guid> checkedHitableUIDs { get; private set; }

        private Action<HitInfos> hitEvent;

        public override void Initialize(Action<HitInfos> hitEvent = null)
        {
            base.Initialize(hitEvent);

            checkedHitableUIDs = new();
            this.hitEvent = hitEvent;
        }

        public void SetIgnoreUid(PhysicsObject ignorePhysics)
        {
            ignoreUid = ignorePhysics.uid;
        }

        public void OnCollisionEvent(HitInfos hitInfos)
        {
            if (hitInfos.hitInfos.Count == 0) return;
            this.hitEvent?.Invoke(hitInfos);

            foreach (var hitInfo in hitInfos.hitInfos)
            {
                checkedHitableUIDs.Add(hitInfo.hitObject.uid);
            }
        }

        public override void SetActive(bool set)
        {
            base.SetActive(set);
            if (set == false) checkedHitableUIDs.Clear();
        }
    }
}