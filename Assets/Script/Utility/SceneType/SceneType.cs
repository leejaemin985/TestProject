
using System.Collections.Generic;
using UnityEngine;

namespace SceneType
{
    public class SceneType
    {
        public int id { get; private set; }
        public string name { get; private set; }

        public bool useCursor { get; private set; }
        private SceneType(int id, string name, bool useCursor)
        {
            this.id = id;
            this.name = name;
            this.useCursor = useCursor;
        }

        public static SceneType Localinitialize { get; private set; } = new(0, "Localinitialize", false);

        public static SceneType Lobby { get; private set; } = new(1, "Lobby", true);

        public static SceneType WaitingRoom { get; private set; } = new(2, "WaitingRoom", true);

        public static SceneType InGame { get; private set; } = new(3, "InGame", false);
        

        private static readonly Dictionary<string, SceneType> mapBySceneName = new()
        {
            { Localinitialize.name, Localinitialize },
            { Lobby.name, Lobby },
            { WaitingRoom.name, WaitingRoom },
            { InGame.name, InGame }
        };
    }
}
