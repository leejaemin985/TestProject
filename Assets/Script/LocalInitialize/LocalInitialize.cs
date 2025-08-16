using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

using Fusion;

using Utility.Spinner;

namespace Localinitialize
{
    public class LocalInitialize : MonoBehaviour
    {
        private async void Start()
        {
            Spinner.Instance.OnSpinner(() => GameNetworkManager.Instance.isInitialized);
            await GameNetworkManager.Instance.Initialize();

            SceneManager.LoadScene(SceneType.SceneType.Lobby.id, LoadSceneMode.Single);
        }
    }
}
