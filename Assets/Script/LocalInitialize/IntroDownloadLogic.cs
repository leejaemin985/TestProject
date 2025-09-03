using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Localinitialize
{
    public class IntroDownloadLogic : MonoBehaviour
    {
        public const string KEY_SAMURAI_MODEL = "SamuraiModel";

        private static readonly IList<string> REQUIRED_KEYS = new List<string>()
        {
            KEY_SAMURAI_MODEL
        };

        public async Task DownloadAddressables()
        {
            await AddressableManager.LoadCatalog();

            var size = await GetRequiredDownloadBytes(REQUIRED_KEYS);
            Debug.Log($"required download Data Size: {size}");

            Debug.Log($"Test - done");
        }

        private async Task<long> GetRequiredDownloadBytes(IList<string> keyList)
        {
            long total = 0;
            List<Task<long>> tasks = new();
            foreach (var key in keyList)
            {
                tasks.Add(AddressableUtil.CheckSize(key));
            }

            long[] results = await Task.WhenAll(tasks);
            foreach (var size in results) total += size;

            return total;
        }
    }
}