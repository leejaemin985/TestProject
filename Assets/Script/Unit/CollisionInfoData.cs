using Physics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unit
{
    [Serializable]
    public class CollisionInfos
    {
        public List<CollisionInfoData> collisionInfos;
        public CollisionInfos()
        {
            collisionInfos = new();
        }
    }


    [Serializable]
    public class CollisionInfoData
    {
        public HitBox hitObject;
        public Vector3 hitPoint;
        public float sweepProgress;
    }
}