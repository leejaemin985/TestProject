using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using System.Linq;

namespace Unit
{
    public enum UnitType
    {
        Default
    }


    public enum StatId : byte
    {
        hp,
        maxHp, 
        posture,
        maxPosture
    }

    public abstract class UnitStat : NetworkBehaviour
    {
        private static Dictionary<PlayerRef, UnitStat> spawnedUnitStatMap = new();

        public static Dictionary<PlayerRef, Action<UnitStat>> spawnedCallbacks = new();

        public static void AddSpawnedCallback(PlayerRef userRef, Action<UnitStat> spawnedListener)
        {
            spawnedCallbacks.TryGetValue(userRef, out var callBack);

            callBack -= spawnedListener;
            callBack += spawnedListener;

            spawnedCallbacks[userRef] = callBack;

            if (spawnedUnitStatMap[userRef] != null)
                spawnedCallbacks[userRef]?.Invoke(spawnedUnitStatMap[userRef]);
        }

        public PlayerRef userRef { get; private set; }

        protected abstract UnitType unitType { get; }

        [Networked, OnChangedRender(nameof(OnChangedMaxHp))] public float maxHp { get; private set; }
        [Networked, OnChangedRender(nameof(OnChangedHp))] public float hp { get; protected set; }

        [Networked, OnChangedRender(nameof(OnChangedMaxPosture))] public float maxPosture { get; private set; }
        [Networked, OnChangedRender(nameof(OnChangedPosture))] public float posture { get; protected set; }


        private Dictionary<StatId, Action> onStatEventListeners;

        private UnitStatScriptable initDataList;

        public void SetUserRef(PlayerRef userRef) => this.userRef = userRef;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                if (initDataList == null)
                    initDataList = Resources.Load<UnitStatScriptable>(UnitStatScriptable.RESOURCES_PATH);

                InitStat(initDataList.unitStatusInitDatas.FirstOrDefault(data => data.unitType == this.unitType));
            }

            SetEventListener();

            spawnedUnitStatMap[userRef] = this;
            if (spawnedCallbacks.ContainsKey(userRef)) spawnedCallbacks[userRef]?.Invoke(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            spawnedUnitStatMap.Remove(userRef);
        }

        private void InitStat(UnitStatInitData initData)
        {
            maxHp = initData.maxHp;
            hp = maxHp;

            maxPosture = initData.maxPosture;
            posture = maxPosture;
        }

        #region Changed Value EventListener 

        private void SetEventListener()
        {
            onStatEventListeners = new();
            foreach (var enumType in Enum.GetValues(typeof(StatId)))
            {
                onStatEventListeners.Add((StatId)enumType, null);
            }
        }

        private void OnChangedMaxHp() => onStatEventListeners[StatId.maxHp]?.Invoke();
        private void OnChangedHp() => onStatEventListeners[StatId.hp]?.Invoke();
        private void OnChangedMaxPosture() => onStatEventListeners[StatId.maxPosture]?.Invoke();
        private void OnChangedPosture() => onStatEventListeners[StatId.posture]?.Invoke();


        public void AddStatEventListener(StatId statId, Action eventListener)
        {
            if (eventListener == null) return;

            onStatEventListeners[statId] -= eventListener;
            onStatEventListeners[statId] += eventListener;
        }

        #endregion

        public void SetHp(float value)
        {
            if (HasStateAuthority == false) return;

            hp = Mathf.Clamp(value, 0, maxHp);
        }

        public void SetPosture(float value)
        {
            if (HasStateAuthority == false) return;

            posture = Mathf.Clamp(value, 0, maxPosture);
        }

    }
}