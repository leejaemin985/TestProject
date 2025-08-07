using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unit
{
    [CreateAssetMenu(fileName = "AttackMotionInfos",menuName = "Scriptable/AttackMotions")]
    public class PlayerAttackMotionInfoScriptable : ScriptableObject
    {
        public List<AttackMotionInfo> attackMotionInfos;
    }

    [Serializable]
    public class AttackMotionInfo
    {
        public AttackMotionType motionType;

        public AnimationClip clip;
        public string motionName;
        public float motionDuration;

        public float damage;
        public float weight;
        public AttackType attackType;

        public List<AttackTiming> attackTimings;
    }

    [Serializable]
    public class AttackTiming
    {
        public int startTick;
        public int endTick;
    }
}