using System.Data.SqlClient;

namespace LightADO;

internal class SqlConnectionHandler
{
    internal static string ValdiateGivenConnectionString(string value)
    {
        try
        {
            SqlConnection sqlConnection = new SqlConnection(value);
            return value;
        }
        catch (Exception ex)
        {
            throw new LightAdoExcption(ex);
        }
    }

    internal static bool IsConnectionStringValid(string connectionString)
    {
        try
        {
            SqlConnectionHandler.ValdiateGivenConnectionString(connectionString);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}