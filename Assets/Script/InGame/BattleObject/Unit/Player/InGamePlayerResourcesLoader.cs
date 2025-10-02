using System.Threading.Tasks;
using UnityEngine;
using Addressable;

namespace Unit
{
    public static class InGamePlayerResourcesLoader
    {
        public static GameObject modelObjectAsset { get; private set; }

        public static GameObject weaponAsset { get; private set; }
        
        public static AddressableScriptable_UserSoundPack soundPack { get; private set; }

        public static async Task LoadAssets()
        {
            await Task.WhenAll(
                LoadModel(),
                LoadWeapon(),
                LoadSoundPack());
        }

        private static async Task LoadModel()
        {
            if (modelObjectAsset == null)
                modelObjectAsset = await AddressableManager.LoadAsset<GameObject>(AddressableKey.PK_SamuraiModel);
        }
            
        private static async Task LoadWeapon()
        {
            if (weaponAsset == null)
                weaponAsset = await AddressableManager.LoadAsset<GameObject>(AddressableKey.PK_Katana);
        }
            
        private static async Task LoadSoundPack()
        {
            if (soundPack == null)
                soundPack = await AddressableManager.LoadAsset<AddressableScriptable_UserSoundPack>(AddressableKey.PK_UserSoundPack);
        }

        public static void Dispose()
        {
            modelObjectAsset = null;
            weaponAsset = null;
            soundPack = null;

            AddressableManager.Release(AddressableKey.PK_SamuraiModel);
            AddressableManager.Release(AddressableKey.PK_Katana);
            AddressableManager.Release(AddressableKey.PK_UserSoundPack);
        }

    }
}