namespace LightADO;

using System.Collections.Generic;
using System.Data;


internal class StoredProcedureParameter
{
    private string storedProcedureName;

    public StoredProcedureParameter()
    {
    }

    internal StoredProcedureParameter(string storedProcedureName, LightADOSetting setting)
    {
        this.storedProcedureName = storedProcedureName;
        this.LightAdoSetting = setting;
    }

    public string Name { get; set; }

    public string Mode { get; set; }

    public string TypeName { get; set; }

    internal LightADOSetting LightAdoSetting { get; set; }

    internal ParameterDirection GetParameterDirection => !(this.Mode == "INOUT") ? ParameterDirection.Input : ParameterDirection.Output;

    internal List<StoredProcedureParameter> Parameters => new Query(this.LightAdoSetting.ConnectionString).ExecuteToListOfObject<StoredProcedureParameter>("select PARAMETER_NAME as Name, PARAMETER_MODE as Mode, Data_Type as TypeName from information_schema.parameters where specific_name= @StoredProcedureName", CommandType.Text, new Parameter("StoredProcedureName", (object)this.storedProcedureName));
}
