using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Localinitialize
{
    public class IntroDownloadLogic : MonoBehaviour
    {
        private const string KEY_REQUIRED = "Required";

        public async Task DownloadAddressables()
        {
            //bool result = Caching.ClearCache();
            //Debug.Log("Cache cleared: " + result);
            //return;
            await AddressableManager.LoadCatalog();

            await StartRequiredAssetDownloadTask();
        }

        private async Task StartRequiredAssetDownloadTask()
        {
            var checkSize = await AddressableManager.GetDownloadBytes(KEY_REQUIRED);
            if (checkSize==0)
            {
                Debug.Log($"IntroDownload - Already Required Asset");

                var handle = await AddressableManager.LoadAsst<GameObject>("SamuraiModel");
                Instantiate(handle);

                return;
            }

            Debug.Log($"IntroDownload - Required Asset Size: {checkSize} bytes");

            await AddressableManager.AssetDownloadDependciesAsync(
                KEY_REQUIRED,
                (progress) => Debug.Log($"Test - DownloadProgress: {progress}"));
        }

    }
}