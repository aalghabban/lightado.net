namespace LightADO;

public class CreateOnNotExists : Attribute
{
    public CreateOnNotExists(string useThisMethod = "CreateOnNotExists") => this.UseThisMethod = useThisMethod;

    public string UseThisMethod { get; set; }
}
