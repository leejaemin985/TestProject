using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.UI
{
    public class InGameBattleUI : MonoBehaviour
    {
        [SerializeField] private InGameUnitStatusUI opponentStatus = default;
        [SerializeField] private InGameUnitStatusUI userStatus = default;

        public void SetUserHp(float current, float max)
        {
            userStatus.SetHpBar(current, max);
        }

        public void SetOpponentHp(float current, float max)
        {
            opponentStatus.SetHpBar(current, max);
        }
    }
}
