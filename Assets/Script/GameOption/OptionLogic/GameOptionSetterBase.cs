using UnityEngine;

namespace GameOption
{
    public abstract class GameOptionSetterBase : MonoBehaviour, IGameOptionSetter
    {
        public enum GameOptionType
        {
            KeySetting,
            Audio
        }

        protected bool wasModified;

        protected abstract GameOptionType OptionType { get; }

        protected virtual void Initialize() { }

        protected virtual void SetActive(bool set) { }

        GameOptionType IGameOptionSetter.GameOptionType => OptionType;


        bool IGameOptionSetter.Modified => wasModified;


        void IGameOptionSetter.Initialize() => Initialize();

        void IGameOptionSetter.SetActive(bool set) => SetActive(set);

    }
}