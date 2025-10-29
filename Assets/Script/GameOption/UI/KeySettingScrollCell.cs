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

        public void Initialize(List<KeyBindingData> bindingDatas, Action showKeyListEvent)
        {
            for (int index = 0; index < items.Length; ++index)
            {
                items[index].Initialize(bindingDatas[index], showKeyListEvent);
            }
        }
    }
}
