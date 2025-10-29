namespace GameOption
{
    interface IGameOptionSetter
    {
        void Initialize();

        string OptionName { get; }

        bool Modified { get; }

        void SetActive(bool set);
    }
}