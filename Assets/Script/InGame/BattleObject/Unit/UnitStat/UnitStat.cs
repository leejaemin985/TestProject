using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using System.Collections;

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
        maxPosture,
        superArmor,
        skillTempTime,
        skillCoolTime
    }

    public abstract class UnitStat : NetworkBehaviour
    {
        #region Static Spawned Listener

        private static Dictionary<PlayerRef, UnitStat> spawnedUnitStatMap = new();

        public static Dictionary<PlayerRef, Action<UnitStat>> spawnedCallbacks = new();

        public static void AddSpawnedCallback(PlayerRef userRef, Action<UnitStat> spawnedListener)
        {
            if (spawnedListener == null) return;

            if (spawnedUnitStatMap.TryGetValue(userRef, out var unitStat))
            {
                spawnedListener?.Invoke(unitStat);
                return;
            }

            if (spawnedCallbacks.ContainsKey(userRef) == false) spawnedCallbacks.Add(userRef, null);
            spawnedCallbacks[userRef] -= spawnedListener;
            spawnedCallbacks[userRef] += spawnedListener;
        }
        
        #endregion

        private PlayerRef presetUserRef;
        [Networked] public PlayerRef userRef { get; private set; }

        protected abstract UnitType unitType { get; }

        [Networked, OnChangedRender(nameof(OnChangedMaxHp))] public float maxHp { get; private set; }
        [Networked, OnChangedRender(nameof(OnChangedHp))] public float hp { get; protected set; }

        [Networked, OnChangedRender(nameof(OnChangedMaxPosture))] public float maxPosture { get; private set; }
        [Networked, OnChangedRender(nameof(OnChangedPosture))] public float posture { get; protected set; }

        [Networked, OnChangedRender(nameof(OnChangedSuperArmor))] public bool superArmor { get; protected set; }

        public bool cachedHasSkill { get; private set; } = true;
        [Networked, OnChangedRender(nameof(OnChangedSkillTempTime))] public float skillTempTime { get; protected set; }
        [Networked, OnChangedRender(nameof(OnChangedSkillCoolTime))] public float skillCoolTime { get; private set; }

        private IEnumerator skillCoolTimeCoHandle;

        private Dictionary<StatId, Action> onStatEventListeners;
        private Dictionary<StatId, IEnumerator> statCoroutineHandlers;

        private UnitStatScriptable initDataList;

        public void SetUserRef(PlayerRef userRef) => this.presetUserRef = userRef;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                if (initDataList == null)
                    initDataList = Resources.Load<UnitStatScriptable>(UnitStatScriptable.RESOURCES_PATH);

                InitStat(initDataList.unitStatusInitDatas.FirstOrDefault(data => data.unitType == this.unitType));
                userRef = presetUserRef;
            }

            SetEventListener();

            spawnedUnitStatMap[userRef] = this;
            if (spawnedCallbacks.TryGetValue(userRef, out var callback))
            {
                callback?.Invoke(this);
                spawnedCallbacks.Remove(userRef);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            spawnedUnitStatMap.Remove(userRef);
            spawnedCallbacks.Remove(userRef);
        }

        private void InitStat(UnitStatInitData initData)
        {
            maxHp = initData.maxHp;
            hp = maxHp;

            maxPosture = initData.maxPosture;
            posture = maxPosture;

            skillCoolTime = initData.skillCoolTime;
        }

        #region Changed Value EventListener 

        private void SetEventListener()
        {
            onStatEventListeners = new();
            statCoroutineHandlers = new();
            foreach (var enumType in Enum.GetValues(typeof(StatId)))
            {
                onStatEventListeners.Add((StatId)enumType, null);
                statCoroutineHandlers.Add((StatId)enumType, null);
            }
        }

        private void OnChangedMaxHp() => onStatEventListeners[StatId.maxHp]?.Invoke();
        private void OnChangedHp() => onStatEventListeners[StatId.hp]?.Invoke();
        private void OnChangedMaxPosture() => onStatEventListeners[StatId.maxPosture]?.Invoke();
        private void OnChangedPosture() => onStatEventListeners[StatId.posture]?.Invoke();
        private void OnChangedSuperArmor() => onStatEventListeners[StatId.superArmor]?.Invoke();
        private void OnChangedSkillTempTime() => onStatEventListeners[StatId.skillTempTime]?.Invoke();
        private void OnChangedSkillCoolTime() => onStatEventListeners[StatId.skillCoolTime]?.Invoke();

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

        public void RunSkillCoolTime()
        {
            if (cachedHasSkill == false) return;

            cachedHasSkill = false;
            RPC_RequestRunSkillCoolTime();
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestRunSkillCoolTime()
        {
            skillTempTime = skillCoolTime;
            if (skillCoolTimeCoHandle != null) StopCoroutine(skillCoolTimeCoHandle);
            StartCoroutine(skillCoolTimeCoHandle = SkillCoolTimeCo());
        }

        private IEnumerator SkillCoolTimeCo()
        {
            while (skillTempTime > 0)
            {
                yield return null;
                skillTempTime -= Time.deltaTime;
            }
            skillTempTime = 0;
            RPC_RequestHasSkill(true);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_RequestHasSkill(bool set)
        {
            cachedHasSkill = set;
        }

        public void OnSuperArmor(int untilTick)
        {
            RPC_RequestOnSuperArmor(untilTick);
        }

        public void OffSuperArmor()
        {
            RPC_RequestOffSuperArmor();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestOnSuperArmor(int untilTick)
        {
            superArmor = true;

            var handler = statCoroutineHandlers[StatId.superArmor];
            if (handler != null) StopCoroutine(handler);
            StartCoroutine(handler = StatControllerWithTick(untilTick, () => superArmor = false));

            statCoroutineHandlers[StatId.superArmor] = handler;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestOffSuperArmor()
        {
            superArmor = false;

            var handler = statCoroutineHandlers[StatId.superArmor];
            if (handler != null) StopCoroutine(handler);
        }

        private IEnumerator StatControllerWithTick(int untilRunnerTick, Action completeListener)
        {
            yield return new WaitUntil(() => Runner.Tick > untilRunnerTick);
            completeListener?.Invoke();
        }
    }
}