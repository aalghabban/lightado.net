namespace LightADO;

public class Types
{
    internal enum ConfigurationSections
    {
        ConnectionStrings,
        AppSettings,
    }

    public enum Directions
    {
        WithQuery,
        WithNonQuery,
        WithBoth,
    }

    internal enum OprationType
    {
        Encrypt,
        Descrypt,
    }

    public enum FormatType
    {
        XML,
        Json,
    }
}
