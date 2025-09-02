using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressableManager
{
    private static string SERVER_PATH => "https://unityprojectaddressable.web.app/StandaloneWindows64/catalog_0.1.json";
    private static AsyncOperationHandle<IResourceLocator> operationHandle;

    private static Dictionary<string, AsyncOperationHandle> assetHandles = new();

    public static async Task LoadCatalog()
    {
        await Addressables.InitializeAsync().Task;

        try
        {
            if (operationHandle.IsValid())
            {
                await AddressableUtil.UpdateCatalogs();
                return;
            }

            operationHandle = await AddressableUtil.LoadCatalog(SERVER_PATH);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }

    public static void ReleaseCatalog()
        => AddressableUtil.ReleaseCatalog(operationHandle);

    public static async Task<T> LoadAsst<T>(string key, CancellationToken? cancellationToken = null)
    {
        if (assetHandles.ContainsKey(key))
            return (T)assetHandles[key].Result;

        var handle = await AddressableUtil.GetTAsync<T>(key);

        assetHandles.TryAdd(key, handle);
        return handle.Result;
    }

}
