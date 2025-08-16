using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

using Fusion;

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
        protected abstract UnitType unitType { get; }

        private UnitStatInitData initData;


        [Networked, OnChangedRender(nameof(OnChangedMaxHp))] public float maxHp { get; private set; }
        [Networked, OnChangedRender(nameof(OnChangedHp))] public float hp { get; protected set; }

        [Networked, OnChangedRender(nameof(OnChangedMaxPosture))] public float maxPosture { get; private set; }
        [Networked, OnChangedRender(nameof(OnChangedPosture))] public float posture { get; protected set; }


        private Dictionary<StatId, Action> onStatEventListeners;

        public void SetInitData(UnitStatInitData initData)
        {
            this.initData = initData;
        }

        public async override void Spawned()
        {
            if (HasStateAuthority)
            {
                InitStat(this.initData);
            }

            SetEventListener();
        }

        private void InitStat(UnitStatInitData initData)
        {
            maxHp = initData.maxHp;
            hp = maxHp;

            maxPosture = initData.maxPosture;
            posture = maxPosture;
        }

        public void SetEventListener()
        {
            onStatEventListeners = new();
            foreach (var enumType in Enum.GetValues(typeof(StatId)))
            {
                onStatEventListeners.Add((StatId)enumType, null);
            }
        }

        public void AddStatEventListener(StatId statId, Action eventListener)
        {
            if (eventListener == null) return;

            onStatEventListeners[statId] -= eventListener;
            onStatEventListeners[statId] += eventListener;
        }

        private void OnChangedMaxHp() => onStatEventListeners[StatId.maxHp]?.Invoke();

        private void OnChangedHp() => onStatEventListeners[StatId.hp]?.Invoke();

        private void OnChangedMaxPosture() => onStatEventListeners[StatId.maxPosture]?.Invoke();

        private void OnChangedPosture() => onStatEventListeners[StatId.posture]?.Invoke();

    }
}