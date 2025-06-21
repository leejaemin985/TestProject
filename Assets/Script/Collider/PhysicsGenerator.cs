using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Physics
{
    public class PhysicsGenerator : MonoBehaviour
    {
        private static PhysicsGenerator instance;
        public static PhysicsGenerator Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject("PhysicsGenerator");
                    instance = obj.AddComponent<PhysicsGenerator>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Initialized();
        }

        private void Initialized()
        {
            physicsMap = new()
            {
                { PhysicsObject.PhysicsType.ATTACK, attackPhysics = new() },
                { PhysicsObject.PhysicsType.HITABLE, hittablePhysics = new() },
                { PhysicsObject.PhysicsType.HYBRID, hybridPhysics = new() },
            };
        }

        private Dictionary<PhysicsObject.PhysicsType, List<PhysicsObject>> physicsMap = default;

        private List<PhysicsObject> attackPhysics = default;
        private List<PhysicsObject> hittablePhysics = default;
        private List<PhysicsObject> hybridPhysics = default;

        public void RegisterPhysicsObject(PhysicsObject physicsObject)
        {
            if (physicsMap[physicsObject.physicsType].Contains(physicsObject) == true) return;
            physicsMap[physicsObject.physicsType].Add(physicsObject);
        }

        public void UnRegisterPhysicsObject(PhysicsObject physicsObject)
        {
            if (physicsMap[physicsObject.physicsType].Contains(physicsObject) == false) return;
            physicsMap[physicsObject.physicsType].Remove(physicsObject);
        }

        private void LateUpdate()
        {

        }

        private void CalculateOneTick()
        {
            foreach (var attackableOb in attackPhysics)
            {
                foreach (var hitableOb in hittablePhysics)
                {
                    if (attackableOb.checkedHitableUIDs.ContainsKey(hitableOb.uid) == true) continue;

                    //var collisionInfo = CollisionDetecter.CheckCollisionInfo()


                }
                
            }

        }

        private float ComputeProgressAlongMotion(float3 prev, float3 curr, float3 contactPoint)
        {
            float3 movement = curr - prev;
            float lenSq = math.lengthsq(movement);

            if (lenSq < math.EPSILON)
            {
                return math.distance(prev, contactPoint);
            }

            float3 dir = movement / math.sqrt(lenSq);
            return math.dot(contactPoint - prev, dir);
        }
    }
}
