using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRunnerController : MonoBehaviour
{
    public NetworkRunner runner;
    public FusionNetworkCallbacks networkCallbacks;

    public void Start()
    {
        runner.AddCallbacks(networkCallbacks);
    }
}
