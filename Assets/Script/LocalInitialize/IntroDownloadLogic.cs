using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Localinitialize
{
    public class IntroDownloadLogic : MonoBehaviour
    {
        private static readonly IList<string> REQUIRED_KEYS = new List<string>()
        {
            "SamuraiModel"
        };

        public async Task DownloadAddressables()
        {
            await AddressableManager.LoadCatalog();

            //GameObject prefab = await Addressables.LoadAssetAsync<GameObject>("SamuraiModel").Task;
            //Instantiate(prefab);
            //Debug.Log($"Done");
            var result = await AddressableManager.LoadAsst<GameObject>("SamuraiModel");
            Instantiate(result);
            Debug.Log($"Test - done");
            try
            {

                //Debug.Log($"MissingBytes: {GetRequiredDownloadBytes(REQUIRED_KEYS)}");
            }
            catch(Exception e)
            {

            }

        }

        //private long GetRequiredDownloadBytes(IList<string> keyList)
        //{
        //
        //}
    }
}