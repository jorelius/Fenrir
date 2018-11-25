namespace Fenrir.Core.Generators
{
    public class Option
    {
        public Option(OptionDescription description)
        {
            Description = description;
        }

        public OptionDescription Description { get; private set; }

        private string _value = null;
        public string Value
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_value))
                {
                    return Description.DefaultValue;
                }
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }
}