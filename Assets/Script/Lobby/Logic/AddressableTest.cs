using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public class AddressableTest : MonoBehaviour
{
    private async void Start()
    {
        await Addressables.InitializeAsync().Task;

        // 필요하면 강제로 원격 카탈로그 지정
        await Addressables.LoadContentCatalogAsync(
            "https://unityprojectaddressable.web.app/StandaloneWindows64/catalog_0.1.json",
            true
        ).Task;

        // 에셋 로드 & 생성
        var handle = Addressables.InstantiateAsync("SamuraiModel"); // 또는 Address값
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Loaded");
        }
        else
        {
            Debug.LogError("Load Failed");
        }
    }

}
