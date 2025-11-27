using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unit
{
    [CreateAssetMenu(fileName ="UnitStatScriptable", menuName = "Scriptable/UnitStat")]
    public class UnitStatScriptable : ScriptableObject
    {
        public const string RESOURCES_PATH = "Scriptable/UnitStatScriptable";

        public List<UnitStatInitData> unitStatusInitDatas;
    }

    [Serializable]
    public class UnitStatInitData
    {
        public UnitType unitType;

        public float maxHp;

        public float maxPosture;

        // TODO: Implement skill cost logic here.
        public float skillCoolTime;
    }
}