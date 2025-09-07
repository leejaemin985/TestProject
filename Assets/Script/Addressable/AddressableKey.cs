namespace Addressable
{
    public class AddressableKey
    {
        public enum KeyType
        {
            PrimaryKey,
            Label
        }

        public KeyType keyType { get; private set; }
        public string key { get; private set; }
        public string name { get; private set; }

        public AddressableKey(KeyType keyType, string key, string name)
        {
            this.keyType = keyType;
            this.key = key;
            this.name = name;
        }

        #region Label

        public static readonly AddressableKey LBL_Required = new AddressableKey(KeyType.Label, "Required", "Required Asset");

        #endregion


        #region PrimaryKey

        public static readonly AddressableKey PK_SamuraiModel = new AddressableKey(KeyType.PrimaryKey, "Model_Samurai", "User Samurai Model Fbx");

        public static readonly AddressableKey PK_Katana = new AddressableKey(KeyType.PrimaryKey, "Weapon_Katana", "Weapon Katana");

        #endregion
    }
}