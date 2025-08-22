using Fusion;

namespace SceneType
{
    public class NetScene
    {
        public SceneType sceneType { get; private set; }

        public SceneRef sceneRef { get; private set; }

        private NetScene(SceneType sceneType)
        {
            this.sceneType = sceneType;
            this.sceneRef = SceneRef.FromIndex(sceneType.id);
        }

        //public static NetScene Localinitialize { get; private set; } = new(SceneType.Localinitialize);
        //public static NetScene Lobby { get; private set; } = new(SceneType.Lobby);

        public static NetScene WaitingRoom { get; private set; } = new(SceneType.WaitingRoom);

        public static NetScene InGame { get; private set; } = new(SceneType.InGame);
    }
}