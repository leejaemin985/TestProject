using UnityEngine;

namespace GameOption
{
    public abstract class GameOptionSetterBase : MonoBehaviour, IGameOptionSetter
    {
        protected abstract string OptionName { get; }

        protected bool wasModified;


        protected virtual void Initialize() { }

        protected virtual void SetActive(bool set) { }


        string IGameOptionSetter.OptionName => OptionName;

        bool IGameOptionSetter.Modified => wasModified;


        void IGameOptionSetter.Initialize() => Initialize();

        void IGameOptionSetter.SetActive(bool set) => SetActive(set);

    }
}