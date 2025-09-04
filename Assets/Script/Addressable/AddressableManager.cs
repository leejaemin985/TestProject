using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Addressable
{
    public static class AddressableManager
    {
        private static AddressableUtil addressableUtil = new();

        private static string SERVER_PATH => "https://unityprojectaddressable.web.app/StandaloneWindows64/catalog_0.1.json";
        private static AsyncOperationHandle<IResourceLocator> operationHandle;

        private static Dictionary<string, AsyncOperationHandle> assetHandles = new();

        public static bool ClearAssets()
        {
            return Caching.ClearCache();
        }

        public static async Task LoadCatalog()
        {
            await Addressables.InitializeAsync().Task;

            try
            {
                if (operationHandle.IsValid())
                {
                    await addressableUtil.UpdateCatalogs();
                    return;
                }

                operationHandle = await addressableUtil.LoadCatalog(SERVER_PATH);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void ReleaseCatalog()
            => addressableUtil.ReleaseCatalog(operationHandle);

        public static async Task<T> LoadAsst<T>(string key, CancellationToken? cancellationToken = null)
        {
            if (assetHandles.ContainsKey(key))
                return (T)assetHandles[key].Result;

            var handle = await addressableUtil.GetTAsync<T>(key);

            assetHandles.TryAdd(key, handle);
            return handle.Result;
        }

        public static async Task<long> GetAssetsDownloadBytes(params AddressableKey[] addressableKeys)
        {
            long total = 0;
            List<Task<long>> tasks = new();
            foreach (var addressableKey in addressableKeys)
            {
                tasks.Add(addressableUtil.CheckSize(addressableKey.key));
            }

            long[] results = await Task.WhenAll(tasks);
            foreach (var size in results) total += size;

            return total;
        }

        public static async Task AssetDownloadDependciesAsync(AddressableKey addressableKey, Action<float> loadProgress = null)
        {
            var handle = Addressables.DownloadDependenciesAsync(addressableKey.key);
            
            const int PROGRESS_DELAY = 50;
            while (handle.IsDone == false)
            {
                loadProgress?.Invoke(handle.PercentComplete);
                await Task.Delay(PROGRESS_DELAY);
            }

            Addressables.Release(handle);
        }

    }
}
