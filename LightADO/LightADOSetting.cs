namespace LightADO;

public sealed class LightADOSetting
{
    private string connectionStirng = string.Empty;

    public LightADOSetting() => this.ConnectionString = this.LoadConnectionString();

    public LightADOSetting(string connectionString)
    {
        if (SqlConnectionHandler.IsConnectionStringValid(connectionString))
            this.ConnectionString = connectionString;
        else
            this.ConnectionString = this.LoadConnectionString(connectionString);
    }

    public string ConnectionString
    {
        get => this.connectionStirng;
        set => this.connectionStirng = !string.IsNullOrWhiteSpace(value) ? SqlConnectionHandler.ValdiateGivenConnectionString(value) : throw new LightAdoExcption("Can't set null or empty as connection string");
    }

    private string LoadConnectionString(string connectionStringName = "DefaultConnection") => ConfigurationLoader.GetValueOfKey(connectionStringName) ?? throw new LightAdoExcption("Lightado did not find a connection string with name DefaultConnection, in both appsettings.json or the app.confg");
}
