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

        [Header("Opponent")]
        [SerializeField] private Image opponentHpBar;

        public void Initialize(Player user, Player opponent)
        {
            user.AddHpEventListener(SetUserHpBar);
            opponent.AddHpEventListener(SetOpponentHpBar);
        }

        private void SetUserHpBar(float currentHp, float maxHp)
        {
            userHpBar.fillAmount = currentHp / maxHp;
        }

        private void SetOpponentHpBar(float currentHp, float maxHp)
        {
            opponentHpBar.fillAmount = currentHp / maxHp;
        }
    }
}
