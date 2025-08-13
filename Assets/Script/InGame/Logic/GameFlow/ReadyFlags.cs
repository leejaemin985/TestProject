namespace InGame.Logic
{
    [System.Flags]
    public enum ReadyFlags : byte
    {
        None = 0,
        SceneLoaded = 1 << 0,
        SessionSingleLoaded = 2 << 0,
        PlayerSpawned = 3 << 0,
        NetReady = 4 << 0
    };

    public struct Barrier
    {
        public ReadyFlags local;
    }
}