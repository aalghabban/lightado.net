namespace LightADO;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnName : Attribute
{
    public ColumnName(string name) => this.Name = !string.IsNullOrEmpty(name) ? name : throw new LightAdoExcption("Column name can't be null");


    /// <summary>
    /// Gets or sets the name of the Column
    /// </summary>
    public string Name { get; set; }
}