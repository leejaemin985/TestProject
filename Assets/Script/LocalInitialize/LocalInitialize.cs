using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

using Utility.Spinner;
using Utility.CommonPopup;

namespace Localinitialize
{
    public class LocalInitialize : MonoBehaviour
    {
        private async void Start()
        {
            while (true)
            {
                await TryConnect();

                if (GameNetworkManager.Instance.isInitialized) break;

                await OnReconnectConfirmCommonPopup();
            }
            
            SceneManager.LoadScene(SceneType.SceneType.Lobby.id, LoadSceneMode.Single);
        }

        private async Task TryConnect()
        {
            bool isTryConnecting = true;

            Spinner.Instance.OnSpinner(() => isTryConnecting == false, true);
            await GameNetworkManager.Instance.Connect();

            isTryConnecting = false;
        }

        private async Task OnReconnectConfirmCommonPopup()
        {
            bool waitInput = true;

            const string title = "Connection Failed";
            const string content = "Would you like to try reconnecting to the lobby?";
            const string confirmText = "reconnect";

            CommonPopup.PopupPolicy popupPolicy = 
                new(CommonPopup.PopupPolicy.PopupKind.Confirm, title, content, confirmText, null, () => waitInput = false);

            CommonPopup.Instance.OnPopup(popupPolicy);
            while (waitInput) await Task.Yield();
        }
    }
}
