namespace GameOption
{
    interface IGameOptionSetter
    {
        GameOptionSetterBase.GameOptionType GameOptionType { get; }

        void Initialize();

        bool Modified { get; }

        void SetActive(bool set);
    }
}