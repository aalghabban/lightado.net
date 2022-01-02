namespace LightADO;
using System.Data;

public class Transaction
{
    public object Data { get; set; }

    public string Command { get; set; }

    public CommandType CommandType { get; set; }

    public Parameter[] Parameters { get; set; }
}
