using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressableUtil
{
    public static async Task<AsyncOperationHandle<IResourceLocator>> LoadCatalog(string serverPath)
    {
        AsyncOperationHandle<IResourceLocator> loadCatalogOperation = Addressables.LoadContentCatalogAsync(serverPath, autoReleaseHandle: false);

        TaskCompletionSource<bool> completionSource = new();

        var failedException = new Exception("LoadResourceCatalogAsync Failed");

        loadCatalogOperation.Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Failed || handle.Result == null)
            {
                completionSource.TrySetException(failedException);
                return;
            }

            IEnumerable<object> keys = handle.Result.Keys.Where(s => s.ToString().Contains(".bundle"));
            Debug.Log($"keys:\n {string.Join("\n", keys)}");

            completionSource.TrySetResult(true);
        };

        await completionSource.Task;
        Debug.Log($"LoadReouceCatalogAsync Complete - {nameof(serverPath)}");

        return loadCatalogOperation;
    }

    public static void ReleaseCatalog(AsyncOperationHandle<IResourceLocator> locatorHandle)
    {
        if (locatorHandle.IsValid() == false) return;

        SafeRelease(locatorHandle);
        Debug.Log($"ReleaseResourceCatalogAsync Complete - {nameof(locatorHandle)}");
    }

    public static async Task UpdateCatalogs()
    {
        var handle = Addressables.CheckForCatalogUpdates(autoReleaseHandle: false);
        var result = await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"AddressableUtil: UpdateCatalogs - Failed {handle.OperationException}");
            return;
        }

        if (result.Count == 0) return;

        var updates = new List<string>();
        Debug.Log($"AddressableUtil: UpdateCatalogs - {string.Join(", ", updates)}");
        updates.AddRange(result);

        var updateHandle = Addressables.UpdateCatalogs(updates, autoReleaseHandle: false);
        await updateHandle.Task;

        Debug.Log($"AddressableUtil: UpdateCatalogs Complete - {string.Join(", ", updates)}");

        Addressables.Release(updateHandle);
        Addressables.Release(handle);
    }

    public static void SafeRelease<T>(AsyncOperationHandle<T> handle)
    {
        if (handle.IsValid() == false)
        {
            Debug.LogWarning($"AddressableUtil: SafeRelease - {handle.DebugName} is not valid");
            return;
        }

        try
        {
            Addressables.Release(handle);
        }
        catch (Exception ex)
        {
            Debug.LogError($"AddressableUtil: ERROR - SafeRelease - {handle.DebugName} - {ex}");
        }
    }

    public static async Task<long> CheckSize(string key)
    {
        AsyncOperationHandle<long> handle = Addressables.GetDownloadSizeAsync(key);
        await handle.Task;

        var size = handle.Result;
        SafeRelease(handle);

        return size;
    }

    public static async Task<bool> AddressableExists(string key)
    {
        //현재 등록된 로케이터 안에 존재하는지 확인후 비어있으면 없음.
        AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(key);
        try
        {
            await handle.Task;

            if (handle.Result.Count == 0)
            {
                return false;
            }

            return true;
        }
        finally
        {
            SafeRelease(handle);
        }
    }

    public static async Task<AsyncOperationHandle<T>> GetTAsync<T>(string itemPath)
    {
        bool isExist = await AddressableExists(itemPath);

        if (!isExist)
        {
            return default;
        }

        var item = await LoadAddressableAsset<T>(itemPath);
        if (item.IsValid() == false)
        {
            return default;
        }

        return item;
    }

    public static async Task<AsyncOperationHandle<T>> LoadAddressableAsset<T>(string key)
    {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;

        return handle;
    }
}