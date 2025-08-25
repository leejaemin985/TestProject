using System;
using System.Collections.Generic;

using UnityEngine;

using Fusion;

using EnhancedUI.EnhancedScroller;

namespace Lobby.Logic
{
    using UI;

    public class LobbySessionScrollController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private LobbySessionScroller scroller;
        [SerializeField] private LobbySessionScrollCellView cellPrefab;

        private List<SessionInfo> datas = default;
        private Action<SessionInfo> onClickCellEvent = default;

        public void Initialize(Action<SessionInfo> onClickCellEvent)
        {
            scroller.Delegate = this;
            this.onClickCellEvent = onClickCellEvent;
        }

        public void UpdateSessionList(List<SessionInfo> sessionList)
        {
            datas = new();
            foreach (var session in sessionList)
            {
                if (session.IsOpen) datas.Add(session);
            }

            scroller.ReloadData(scroller.NormalizedScrollPosition);
        }


        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cell = scroller.GetCellView(cellPrefab) as LobbySessionScrollCellView;
            cell.Initialize(datas[dataIndex], onClickCellEvent, dataIndex);

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
