using Microsoft.Extensions.Configuration;

namespace LightADO;

internal class ConfigReader
{
    public static string GetValueOfKey(string section, string key)
    {
        return new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection(section)[key];
    }
}
