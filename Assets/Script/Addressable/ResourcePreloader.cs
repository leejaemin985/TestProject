using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using System;

public class ResourcePreloader : MonoSingleton<ResourcePreloader>
{
    private bool loadedModelCatalog;

    public async Task DownloadResourcesAsync()
    {

    }

    public async Task LoadCatalog()
    {
        Task[] loadTasks = new Task[]
        {
            LoadModelCatalog(),
        };

        await Task.WhenAll(loadTasks);
    }

    private async Task LoadModelCatalog()
    {
        if (loadedModelCatalog) return;

        const string CATALOG_PATH = "https://unityprojectaddressable.web.app/StandaloneWindows64/catalog_0.1.json";
        var handle = Addressables.LoadContentCatalogAsync(CATALOG_PATH, true);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            loadedModelCatalog = true;
        else
            Debug.LogError("Failed to load ModelCatalog");
    }


    public async Task DownloadSamuraiModel(Action<GameObject> completeListener)
    {
        const string MODEL_KEY = "SamuraiModel";

        var handle = Addressables.InstantiateAsync(MODEL_KEY);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            completeListener?.Invoke(handle.Result);
        }
        else
        {
            Debug.LogError("Failed Load SamuraiModel");
            return;
        }
    }

}
