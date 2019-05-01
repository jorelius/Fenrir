using PowerArgs;

namespace Fenrir.Cli
{
    [TabCompletion]
    public class ListArgs
    {
        [ArgDescription("Plugin Type"), DefaultValue(PluginType.Generator)]
        PluginType PluginType { get; set; }
    }

    internal enum PluginType
    {
        Generator
    }
}