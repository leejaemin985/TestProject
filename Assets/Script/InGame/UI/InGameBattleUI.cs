using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Fusion;
using Unit;



namespace InGame.UI
{
    public class InGameBattleUI : MonoBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        [Header("User")]
        [SerializeField] private Image userHpBar;
        [SerializeField] private Image skillTempFilled;

        [Header("Opponent")]
        [SerializeField] private Image opponentHpBar;

        private void Start()
        {
            foreach (var userRef in GameNetworkManager.Instance.connectedUsers)
            {
                UnitStat.AddSpawnedCallback(userRef, BindStatUI);
            }
        }

        private void BindStatUI(UnitStat unitStat)
        {
            if (unitStat.userRef == runner.LocalPlayer)
            {
                unitStat.AddStatEventListener(StatId.hp, () => SetUserHpBar(unitStat.hp, unitStat.maxHp));
                unitStat.AddStatEventListener(StatId.skillTempTime, () => SetSkillTempTime(unitStat.skillTempTime, unitStat.skillCoolTime));
            }
            else
                unitStat.AddStatEventListener(StatId.hp, () => SetOpponentHpBar(unitStat.hp, unitStat.maxHp));
        }

        private void SetUserHpBar(float currentHp, float maxHp)
        {
            userHpBar.fillAmount = currentHp / maxHp;
        }

        private void SetOpponentHpBar(float currentHp, float maxHp)
        {
            opponentHpBar.fillAmount = currentHp / maxHp;
        }

        private void SetSkillTempTime(float temp, float max)
        {
            skillTempFilled.fillAmount = temp / max;
        }
    }
}
