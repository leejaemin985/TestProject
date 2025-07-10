using Physics;
using System;
using System.Collections;
using System.Collections.Generic;
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
        bool CheckRegistryInitialized() => PlayerRegistry.Instance != null && PlayerRegistry.Instance.Initialized;
        bool CheckPhysicsEventHandlerInitialized() => PhysicsEventHandler.Instance != null && PhysicsEventHandler.Instance.Initialized;

        while (!isInitialized)
        {
            if (CheckRegistryInitialized() && CheckPhysicsEventHandlerInitialized())
                isInitialized = true;

            yield return new WaitForSeconds(CHECK_INTERVAL);
        }
    }

    #region Using Reflection
    //private IEnumerator CheckMasterSingletonsReady()
    //{
    //    yield return null;

    //    List<Type> masterSingletonTypes = new();

    //    var assemblies = AppDomain.CurrentDomain.GetAssemblies();


    //    foreach (var asm in assemblies)
    //    {
    //        foreach (var type in asm.GetTypes())
    //        {
    //            if (type.IsClass &&
    //                !type.IsAbstract &&
    //                type.BaseType != null &&
    //                type.BaseType.IsGenericType &&
    //                type.BaseType.GetGenericTypeDefinition() == typeof(MasterSingleton<>))
    //            {
    //                masterSingletonTypes.Add(type);
    //            }
    //        }
    //    }

    //    var checkDelay = new WaitForSeconds(CHECK_INTERVAL);
    //    while (true)
    //    {
    //        bool allReady = true;

    //        foreach (var type in masterSingletonTypes)
    //        {
    //            if (!IsMasterSingletonInitialized(type))
    //            {
    //                allReady = false;
    //                break;
    //            }
    //        }

    //        if (allReady)
    //        {
    //            isInitialized = true;
    //            yield break;
    //        }

    //        yield return checkDelay;
    //    }

    //}

    //private static bool IsMasterSingletonInitialized(Type type)
    //{
    //    var instanceProp = type.GetProperty("Instance",
    //        System.Reflection.BindingFlags.Public |
    //        System.Reflection.BindingFlags.Static |
    //        System.Reflection.BindingFlags.FlattenHierarchy);
    //    if (instanceProp == null) return false;

    //    var instance = instanceProp.GetValue(null);
    //    if (instance == null) return false;

    //    var initializedProp = type.GetProperty("Initialized", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
    //    if (initializedProp == null) return false;


    //    return (bool)initializedProp.GetValue(instance);
    //}
    #endregion
}