
namespace SceneType
{
    public class SceneType
    {
        public int id { get; private set; }
        public string name { get; private set; }

        private SceneType(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public static SceneType Localinitialize { get; private set; } = new(0, "Localinitialize");
        
        public static SceneType Lobby { get; private set; } = new(1, "Lobby");

        public static SceneType WaitingRoom { get; private set; } = new(2, "WaitingRoom");

        public static SceneType InGame { get; private set; } = new(3, "InGame");
         
    }
}
