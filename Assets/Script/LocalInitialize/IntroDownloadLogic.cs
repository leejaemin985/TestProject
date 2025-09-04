using System;
using System.Threading.Tasks;
using UnityEngine;
using Addressable;

namespace Localinitialize
{
    public class IntroDownloadLogic : MonoBehaviour
    {
        private const string KEY_REQUIRED = "Required";

        public async Task DownloadAddressables()
        {
            await AddressableManager.LoadCatalog();

            await StartRequiredAssetDownloadTask();
        }

        private async Task StartRequiredAssetDownloadTask()
        {
            var checkSize = await AddressableManager.GetAssetsDownloadBytes(AddressableKey.LBL_Required);
            if (checkSize==0)
            {
                Debug.Log($"IntroDownload - Already Required Asset");

                var handle = await AddressableManager.LoadAsst<GameObject>("SamuraiModel");
                Instantiate(handle);

                return;
            }

            Debug.Log($"IntroDownload - Required Asset Size: {checkSize} bytes");

            await AddressableManager.AssetDownloadDependciesAsync(
                AddressableKey.LBL_Required,
                (progress) => Debug.Log($"Test - DownloadProgress: {progress}"));
        }

    }
}