using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class TestScript : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;

    private NetworkRunner runner;

    // Start is called before the first frame update
    void Start()
    {
        runner = Instantiate(runnerPrefab);

        runner.StartGame(new()
        {
            GameMode = GameMode.Shared,
            SessionName = "TestRoom",
            PlayerCount = 2,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
        }); ;
    }

}
