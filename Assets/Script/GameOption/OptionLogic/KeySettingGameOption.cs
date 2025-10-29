using UnityEngine;
using EnhancedUI.EnhancedScroller;
using System.Collections.Generic;

namespace GameOption
{
    public class KeySettingGameOption : GameOptionSetterBase, IEnhancedScrollerDelegate
    {
        protected override string OptionName => "KeySetting";

        [SerializeField] private KeySettingGameOptionUI keySettingUI;
        [SerializeField] private KeySettingScroller scroller;
        [SerializeField] private KeySettingScrollCell cellPrefab;

        private const float CELL_HEIGHT = 200;


        private float mouseSensitive { get; set; }

        private InputSystemSettings inputSystemSetter;

        private List<List<KeyBindingData>> keyBindingDatas = new();

        protected override void Initialize()
        {
            base.Initialize();

            inputSystemSetter = new();
            inputSystemSetter.Initialize();
            scroller.Delegate = this;
        }

        protected override void SetActive(bool set)
        {
            if (set) UpdateKeySettings();
            //else 
        }

        private void UpdateKeySettings()
        {
            var infos = inputSystemSetter.GetKeyBindingInfos();
            if (infos == null || infos.Count == 0)
            {
                keyBindingDatas = new();
                return;
            }

            int pairCount = (infos.Count + 1) / 2;
            keyBindingDatas = new(pairCount); 

            for (int i = 0; i < infos.Count; i += 2)
            {

                var pair = new List<KeyBindingData>(2)
                {
                    infos[i]
                };

                if (i + 1 < infos.Count)
                    pair.Add(infos[i + 1]);

                keyBindingDatas.Add(pair);
            }
        }

        private void ShowKeyList()
        {
            Debug.Log($"Test - CallShow Key List");
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cell = scroller.GetCellView(cellPrefab: cellPrefab) as KeySettingScrollCell;
            cell.Initialize(keyBindingDatas[dataIndex], ShowKeyList);
            return cell;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return CELL_HEIGHT;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return keyBindingDatas.Count;
        }
    }
}