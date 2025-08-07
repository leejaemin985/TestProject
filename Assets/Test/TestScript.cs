using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using Fusion;
using Fusion.Photon.Realtime;
using ExitGames.Client.Photon;

public class TestScript : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;
    private NetworkRunner runner;

    // Start is called before the first frame update
    private async void Start()
    {
        runner = Instantiate(runnerPrefab);

        var result = await runner.JoinSessionLobby(SessionLobby.ClientServer, null);

        //await runner.StartGame(new()
        //{
        //    GameMode = GameMode.Shared,
        //    SessionName = "TestRoom",
        //    PlayerCount = 2,
        //    SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
        //});

    }

}
