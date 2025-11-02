using UnityEngine;
using Utility.Sound;

namespace GameOption
{
    public class GameOption : MonoSingleton<GameOption>
    {
        [SerializeField] private GameOptionUI optionUI;

        [SerializeField] private GameOptionSetterBase[] setterBases;

        public void Initialize()
        {
            foreach (var setter in setterBases)
            {
                (setter as IGameOptionSetter).Initialize();
            }
        }

    }
}