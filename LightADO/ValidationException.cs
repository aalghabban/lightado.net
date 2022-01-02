namespace LightADO;

public class ValidationException : Exception
{
    public ValidationException(string message) => this.Message = message;

    public new string Message { get; private set; }
}
