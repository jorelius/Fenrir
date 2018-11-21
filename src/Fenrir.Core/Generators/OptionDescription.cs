namespace Fenrir.Core.Generators
{
    public class OptionDescription
    {
        public OptionDescription(string key, string defaultValue, string description, bool isRequired = true)
        {
            Key = key;
            DefaultValue = defaultValue;
            Description = description;
            IsRequired = isRequired; 
        }

        public string Key { get; private set; }

        public bool IsRequired { get; private set; } = true;

        public string DefaultValue { get; private set; }

        public string Description { get; private set; }
    }
}