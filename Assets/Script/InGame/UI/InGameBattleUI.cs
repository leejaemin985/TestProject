using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unit;

namespace InGame.UI
{
    public class InGameBattleUI : MonoBehaviour
    {
        //[SerializeField] private InGameUnitStatusUI opponentStatus = default;
        //[SerializeField] private InGameUnitStatusUI userStatus = default;

        //private async void Start() => await Bind();

        //public async Task Bind()
        //{
        //    const int CHECK_DELAY_MS = 100;
        //    while (PlayerRegistry.Instance == null ||
        //        PlayerRegistry.Instance.RegistedUsers.Count != GameNetworkManager.Instance.connectedUsers.Count)
        //    {
        //        await Task.Delay(CHECK_DELAY_MS);
        //    }

        //    Player localUser = null;
        //    Player opponentUser = null;

        //    foreach (var user in PlayerRegistry.Instance.RegistedUsers)
        //    {
        //        if (user.Key == GameNetworkManager.Instance.runner.LocalPlayer) localUser = user.Value;
        //        else opponentUser = user.Value;
        //    }

        //    localUser.AddHpEventListener(SetUserHp);
        //    opponentUser.AddHpEventListener(SetOpponentHp);
        //}

        //public void SetUserHp(float current, float max)
        //{
        //    userStatus.SetHpBar(current, max);
        //}

        //public void SetOpponentHp(float current, float max)
        //{
        //    opponentStatus.SetHpBar(current, max);
        //}
    }
}
