namespace LightADO;

internal static class ConfigurationLoader
{
    internal static string GetValueOfKey(string keyName, Types.ConfigurationSections section = Types.ConfigurationSections.ConnectionStrings)
    {
        return ConfigReader.GetValueOfKey(section.ToString(), keyName);
    }
}
