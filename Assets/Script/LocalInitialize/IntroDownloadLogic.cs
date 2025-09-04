using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utility.Spinner;
using Addressable;

namespace Localinitialize
{
    public class IntroDownloadLogic : MonoBehaviour
    {
        private bool isLoaded;

        public async Task DownloadAddressables()
        {
            //bool result = Caching.ClearCache(); // Addressable Testing

            isLoaded = false;
            Spinner.Instance.OnSpinner(until: () => isLoaded, onLoadingImage: false, text: "Check Resources...");
            await AddressableManager.LoadCatalog();

            await StartRequiredAssetDownloadTask();

            isLoaded = true;
        }

        private async Task StartRequiredAssetDownloadTask()
        {
            var checkSize = await AddressableManager.GetAssetsDownloadBytes(AddressableKey.LBL_Required);
            if (checkSize==0)
            {
                Debug.Log($"IntroDownload - Already Required Asset");
                return;
            }

            Debug.Log($"IntroDownload - Required Asset Size: {checkSize} bytes");

            await AddressableManager.AssetDownloadDependciesAsync(
                AddressableKey.LBL_Required,
                (progress) => Spinner.Instance.SetText($"Download Resource ({(int)(progress * 100)}%)"));
        }

    }
}