using System;
using System.Collections;
using System.Collections.Generic;
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
            if (physicsMap == null)
                physicsMap = new();

            physicsMap.Clear();
            foreach (var type in Enum.GetValues(typeof(PhysicsObject.PhysicsType)))
            {
                physicsMap.Add((PhysicsObject.PhysicsType)type, new());
            }
        }

        private Dictionary<PhysicsObject.PhysicsType, List<PhysicsObject>> physicsMap = default;

        public void RegisterPhysicsObject(PhysicsObject physicsObject)
        {
            var list = physicsMap[physicsObject.physicsType];
            if (list.Contains(physicsObject) == true) return;

            list.Add(physicsObject);
        }

        public void UnRegisterPhysicsObject(PhysicsObject physicsObject)
        {
            var list = physicsMap[physicsObject.physicsType];
            if (list.Contains(physicsObject) == false) return;

            list.Remove(physicsObject);
        }
    }
}
