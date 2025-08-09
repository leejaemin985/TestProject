using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using Fusion;
using System;

namespace Lobby
{
    public class LobbySessionScrollController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private LobbySessionScroller scroller;
        [SerializeField] private LobbySessionScrollCellView cellPrefab;

        private List<SessionInfo> datas = default;
        private Action<string> onClickCellEvent = default;

        public void Initialize(Action<string> onClickCellEvent)
        {
            this.onClickCellEvent = onClickCellEvent;
            scroller.Delegate = this;
        }

        public void UpdateSessionList(List<SessionInfo> sessionList)
        {
            datas = sessionList;
            scroller.ReloadData();
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cell = scroller.GetCellView(cellPrefab) as LobbySessionScrollCellView;
            cell.Initialize(datas[dataIndex], onClickCellEvent);

            return cell;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return LobbySessionScrollCellView.CELL_HEIGHT;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return datas.Count;
        }
    }

}
