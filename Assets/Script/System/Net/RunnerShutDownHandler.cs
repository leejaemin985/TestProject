using System.Threading.Tasks;
using System.Collections.Generic;

using Fusion;
using UnityEngine.SceneManagement;
using Utility.CommonPopup;
using Utility.Spinner;

public static class RunnerShutDownHandler
{
    private static readonly Dictionary<ShutdownReason, CommonPopup.PopupPolicy> Policies = new()
    {
        // Normal / benign
        //{ ShutdownReason.Ok,                         new(CommonPopup.PopupPolicy.PopupKind.Info,    "Disconnected",           "The session was closed normally.",                         "OK",    null, null) },
        //{ ShutdownReason.OperationCanceled,          new(CommonPopup.PopupPolicy.PopupKind.Info,    "Operation Canceled",     "The requested operation was canceled.",                    "OK",    null, null) },

        // Room state
        { ShutdownReason.GameClosed,                 new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Room Closed",            "This game is no longer joinable.",                         "OK") },
        { ShutdownReason.GameNotFound,               new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Room Not Found",         "The game you tried to join does not exist.",               "Refresh") },
        { ShutdownReason.GameIsFull,                 new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Room Full",              "The room has reached its player capacity.",                "Find Another") },
        { ShutdownReason.GameIdAlreadyExists,        new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Duplicate Name",         "A session with the same name already exists.",             "OK") },

        // Region / capacity
        { ShutdownReason.InvalidRegion,              new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Invalid Region",         "The selected region is not available.",                    "OK") },
        { ShutdownReason.MaxCcuReached,              new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Capacity Reached",       "The application's CCU limit has been reached.",            "Retry") },

        // Network / timeouts
        { ShutdownReason.PhotonCloudTimeout,         new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Network Timeout",        "Timed out waiting for Photon Cloud.",                      "Retry") },
        { ShutdownReason.ConnectionTimeout,          new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Connection Timeout",     "Connection to the server timed out.",                      "Retry") },
        { ShutdownReason.OperationTimeout,           new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Operation Timeout",      "The current operation took too long.",                     "Retry") },
        { ShutdownReason.ConnectionRefused,          new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Connection Refused",     "The server refused the connection.",                       "Retry") },

        // Misconfiguration / code issues
        { ShutdownReason.IncompatibleConfiguration,  new(CommonPopup.PopupPolicy.PopupKind.Confirm,   "Configuration Mismatch", "Local game mode does not match the room's mode.",          "OK") },
        { ShutdownReason.InvalidArguments,           new(CommonPopup.PopupPolicy.PopupKind.Confirm,   "Invalid Arguments",      "StartGame arguments are not valid.",                       "OK") },
        { ShutdownReason.AlreadyRunning,             new(CommonPopup.PopupPolicy.PopupKind.Confirm,   "Already Running",        "A NetworkRunner is already running.",                      "OK") },

        // Authentication / policy
        { ShutdownReason.InvalidAuthentication,      new(CommonPopup.PopupPolicy.PopupKind.Confirm,   "Authentication Failed",  "Authentication values are invalid.",                        "OK") },
        { ShutdownReason.CustomAuthenticationFailed, new(CommonPopup.PopupPolicy.PopupKind.Confirm,   "Custom Auth Failed",     "Access was denied by custom authentication.",               "OK") },
        { ShutdownReason.AuthenticationTicketExpired,new(CommonPopup.PopupPolicy.PopupKind.Confirm,   "Authentication Expired", "Your authentication ticket has expired.",                   "OK") },
        { ShutdownReason.DisconnectedByPluginLogic,  new(CommonPopup.PopupPolicy.PopupKind.Confirm,   "Disconnected by Policy", "You were disconnected by server plugin logic.",             "OK") },

        // Generic internal error
        //{ ShutdownReason.Error,                      new(CommonPopup.PopupPolicy.PopupKind.Confirm,   "Internal Error",         "The runner shut down due to an internal error.",            "Retry", null, Reconnect) },
    };

    private static readonly CommonPopup.PopupPolicy InternalErrorPopupPolicy = new(CommonPopup.PopupPolicy.PopupKind.Confirm, "Internal Error", "The runner shut down due to an internal error.", "Retry");

    public static async void OnShutdownPopup(ShutdownReason reason)
    {
        if (reason == ShutdownReason.Ok) return;

        if (Policies.TryGetValue(reason, out var policy))
            CommonPopup.Instance.OnPopup(policy);
        else
            CommonPopup.Instance.OnPopup(InternalErrorPopupPolicy);

        await Reconnect();
    }

    private static async Task Reconnect()
    {
        bool isReconnecting = true;

        Spinner.Instance.OnSpinner(() => isReconnecting == false);
        await GameNetworkManager.Instance.Connect();

        isReconnecting = false;

        if (GameNetworkManager.Instance.isInitialized)
        {
            SceneManager.LoadScene(SceneType.SceneType.Lobby.id, LoadSceneMode.Single);
            return;
        }
        CommonPopup.Instance.OffPopup();

        SceneManager.LoadScene(SceneType.SceneType.Localinitialize.id, LoadSceneMode.Single);
    }
}