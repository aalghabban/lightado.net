namespace LightADO;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class Table : Attribute
{
    public string Name { get; set; }
}
