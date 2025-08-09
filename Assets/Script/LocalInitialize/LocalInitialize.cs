using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LocalInitialize : MonoBehaviour
{
    private const int TRY_CONNECTION_DELAY_MS = 1000;

    private async void Start()
    {
        while (true)
        {
            Debug.Log($"Try initialize");
            await GameNetworkManager.Instance.Initialize();
            if (GameNetworkManager.Instance.isInitialized == true) break;

            await Task.Delay(TRY_CONNECTION_DELAY_MS);
        }

        Debug.Log($"Done initialized");
    }
}
