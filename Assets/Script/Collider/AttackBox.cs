using System;
using System.Collections.Generic;

namespace Physics
{
    public class AttackBox : PhysicsObject
    {
        public override PhysicsType physicsType => PhysicsType.ATTACK;

        public Dictionary<Guid, CollisionInfo> checkedHitableUIDs { get; private set; }

        private Action<CollisionInfo> collisionEvent;

        protected override void Initialize(Action<CollisionInfo> collisionEvent = null)
        {
            base.Initialize(collisionEvent);

            this.collisionEvent = collisionEvent;
        }
    }
}