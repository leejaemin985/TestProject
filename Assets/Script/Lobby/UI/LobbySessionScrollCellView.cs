using EnhancedUI.EnhancedScroller;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Lobby
{
    public class LobbySessionScrollCellView : EnhancedScrollerCellView
    {
        [SerializeField] private TMP_Text sessionName = default;
        [SerializeField] private TMP_Text playerCount = default;

        [SerializeField] private Color validColor = default;
        [SerializeField] private Color invalidColor = default;

        public const float CELL_HEIGHT = 80f;

        private SessionInfo sessionInfo = default;
        private Action<SessionInfo> onEventEntering = default;

        public void Initialize(SessionInfo info, Action<SessionInfo> onEventEntering)
        {
            this.sessionInfo = info;
            this.onEventEntering = onEventEntering;

            SetName(info);
            SetPlayerCount(info);
        }

        private void SetName(SessionInfo info)
        {
            string[] parts = info.Name.Split("//"); //parts[0] => session Uid
            sessionName.text = parts[1];
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
