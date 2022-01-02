namespace LightADO;

using System.Data;
using System.Reflection;

public class CrudBase<T>
{
    private Table table = (Table)null;

    public CrudBase() => this.table = CustomAttributeExtensions.GetCustomAttribute<Table>((MemberInfo)this.GetType(), true);

    public CrudBase(int id)
    {
        this.table = CustomAttributeExtensions.GetCustomAttribute<Table>((MemberInfo)this.GetType(), true);
        new Query().ExecuteToObject<T>(this.table.Name + "_GetById", this, CommandType.StoredProcedure, new Parameter("ID", (object)id));
    }

    public void Create() => this.DoNonQuery(nameof(Create));

    public void Update() => this.DoNonQuery(nameof(Update));

    public void Delete() => this.DoNonQuery(nameof(Delete));

    public T Get(int id) => new Query().ExecuteToObject<T>(this.table.Name + "_GetById", CommandType.StoredProcedure, new Parameter("ID", (object)id));

    private void DoNonQuery(string actionName)
    {
        if (this.table == null)
            throw new LightAdoExcption("In order to use Base Crud you will need to add a table name attribute to the class, lighado will call SP like following: tablename_getById, tablename_create, tablename_update, tablename_delete");
        new NonQuery().Execute<CrudBase<T>>(this.table.Name + "_" + actionName, this);
    }
}
