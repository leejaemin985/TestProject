using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

using Fusion;

using Utility.Spinner;

namespace Localinitialize
{
    public class LocalInitialize : MonoBehaviour
    {
        private const int TRY_CONNECTION_DELAY_MS = 1000;

        private async void Start()
        {
            Spinner.Instance.OnSpinner(() => GameNetworkManager.Instance.isInitialized);

            while (GameNetworkManager.Instance.isInitialized == false)
            {
                await GameNetworkManager.Instance.Initialize();

                await Task.Delay(TRY_CONNECTION_DELAY_MS);
            }

            SceneManager.LoadScene(SceneType.SceneType.Lobby.id, LoadSceneMode.Single);
        }
    }
}
