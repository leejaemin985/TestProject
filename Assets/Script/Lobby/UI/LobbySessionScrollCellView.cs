using System;

using UnityEngine;
using TMPro;

using Fusion;

using EnhancedUI.EnhancedScroller;

namespace Lobby.UI
{
    public class LobbySessionScrollCellView : EnhancedScrollerCellView
    {
        [SerializeField] private TMP_Text sessionName;
        [SerializeField] private TMP_Text playerCount;

        [SerializeField] private Color validColor;
        [SerializeField] private Color invalidColor;

        public const float CELL_HEIGHT = 80f;

        private SessionInfo sessionInfo;
        private Action<SessionInfo> onEventEntering;

        public void Initialize(SessionInfo info, Action<SessionInfo> onEventEntering, int roomIndex)
        {
            this.sessionInfo = info;
            this.onEventEntering = onEventEntering;

            SetName(info, roomIndex);
            SetPlayerCount(info);
        }

        private void SetName(SessionInfo info, int roomIndex)
        {
            sessionName.text = $"[{roomIndex}] {SessionMetaReader.GetWithoutUidSessionName(info.Name)}";
            sessionName.color = GameNetworkManager.Instance.CanEnterSession(info) ? validColor : invalidColor;
        }

        private void SetPlayerCount(SessionInfo info)
        {
            playerCount.text = $"{info.PlayerCount} / {info.MaxPlayers}";
            playerCount.color = GameNetworkManager.Instance.CanEnterSession(info) ? validColor : invalidColor;
        }

        public void OnClickedCellEvent() => onEventEntering?.Invoke(sessionInfo);
    }
}
