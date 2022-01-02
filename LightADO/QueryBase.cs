namespace LightADO;

public class QueryBase
{
    public QueryBase() => this.LightAdoSetting = new LightADOSetting();

    public QueryBase(string connectionString) => this.LightAdoSetting = new LightADOSetting(connectionString);

    public LightADOSetting LightAdoSetting { get; set; }

    internal static void ThrowExacptionOrEvent(
      OnError onError,
      Exception exception,
      string extraInfo = "")
    {
        if (exception.InnerException != null && exception.InnerException.GetType() == typeof(ValidationException))
            throw exception.InnerException;

        if (onError == null)
        {
            if (exception.GetType() != typeof(LightAdoExcption))
                throw new LightAdoExcption(exception, exception.Message);

            throw exception;
        }

        exception.Source = extraInfo;
        onError(new LightAdoExcption(exception, exception.Message));
    }
}
