using EnhancedUI.EnhancedScroller;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameOption
{
    public class KeySettingScrollCell : EnhancedScrollerCellView
    {
        [SerializeField] private KeySettingScrollCellItem[] items;

        public void Initialize(List<KeyBindingInfo> bindingDatas, Action<KeySettingScrollCellItem> cellSelectEvent)
        {
            int index = 0;
            for (; index < bindingDatas.Count; ++index)
            {
                items[index].Initialize(bindingDatas[index], cellSelectEvent);
                items[index].SetActive(true);
            }
            for (; index < items.Length; ++index) items[index].SetActive(false);
        }
    }
}
