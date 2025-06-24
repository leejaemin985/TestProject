using Physics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unit
{
    [Serializable]
    public class HitInfos
    {
        public List<HitInfoData> hitInfos;
    }


    [Serializable]
    public class HitInfoData
    {
        public PhysicsObject hitObject;
        public Vector3 hitPoint;
        public float sweepProgress;
    }
}