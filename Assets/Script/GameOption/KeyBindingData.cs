namespace GameOption
{
    public class KeyBindingData
    {
        public string DisPlayName { get; private set; }
        public string OriginalKeyPath { get; private set; }

        public bool IsModified { get; private set; } = false;

        private string modifiedKeyPath;

        public string CurrentKeyPath
        {
            get { return modifiedKeyPath; }
            set
            {
                if (OriginalKeyPath.Equals(value)) return;

                IsModified = true;
                modifiedKeyPath = value;
            }
        }

        public KeyBindingData(KeyBindingInfo source)
        {
            DisPlayName = source.DisplayName;
            OriginalKeyPath = source.CurrentKeyPath;
            CurrentKeyPath = source.CurrentKeyPath;
        }
    }
}