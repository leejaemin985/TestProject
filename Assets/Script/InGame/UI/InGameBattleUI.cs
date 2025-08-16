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

        public async Task Initialize()
        {
            const int WAIT_USER_DEALY_MS = 100;
            while (PlayerRegistry.Instance != null || PlayerRegistry.Instance.RegistedUsers.Count != GameNetworkManager.Instance.connectedUsers.Count)
            {
                await Task.Delay(WAIT_USER_DEALY_MS);
            }

            

        }

        private void SetHpBar()
        {
            
        }


    }
}
