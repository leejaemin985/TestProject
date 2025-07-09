using Physics;
using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance =
                    new GameObject("GameManager").
                    AddComponent<GameManager>();

                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private static readonly Type[] requiredMasterSingletons =
    {
        typeof(PlayerRegistry),
        typeof(PhysicsEventHandler),
    };

    public bool isInitialized { get; private set; } = false;

    private const float CHECK_INTERVAL = 0.2f;
    private IEnumerator checkMasterSingletonHandle = default;

    private void Start()
    {
        if (checkMasterSingletonHandle != null) StopCoroutine(checkMasterSingletonHandle);
        StartCoroutine(checkMasterSingletonHandle = CheckMasterSingletonsReady());
    }

    private IEnumerator CheckMasterSingletonsReady()
    {
        //while (true)
        //{
        //    bool allReady = true;

        //    foreach (IMasterSingleton type in requiredMasterSingletons)
        //    {
        //        if (type.initialized == false)
        //            allReady = false;
        //        break;
        //    }

        //    if (allReady)
        //    {
        //        isInitialized = true;
        //        yield break;
        //    }

        //    yield return new WaitForSeconds(CHECK_INTERVAL);
        //}
    }
}