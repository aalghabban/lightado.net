namespace LightADO;


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

public class DataMapper
{
    public static List<T> ConvertDataTableToListOfObject<T>(DataTable table, OnError onError = null)
    {
        try
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            if (properties != null && (uint)properties.Length > 0U)
            {
                List<T> objList = new List<T>();
                foreach (DataRow row in (InternalDataCollectionBase)table.Rows)
                    objList.Add(EncryptEngine.EncryptOrDecryptObject<T>(DataMapper.MapDataRowToObject<T>(row, onError), Types.OprationType.Descrypt));
                return objList;
            }
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(onError, ex, string.Empty);
        }
        return (List<T>)null;
    }

    public static T ConvertDataTableToObject<T>(DataTable table, OnError onError = null)
    {
        try
        {
            if (table != null && ((InternalDataCollectionBase)table.Rows).Count > 0)
                return EncryptEngine.EncryptOrDecryptObject<T>(DataMapper.MapDataRowToObject<T>(table.Rows[0], onError), Types.OprationType.Descrypt);
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(onError, ex, string.Empty);
        }
        return default(T);
    }

    public static DataSet ConvertDataTableToDataSet(DataTable dataTable)
    {
        DataSet dataSet = (DataSet)null;
        if (dataTable != null)
        {
            dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);
        }
        return dataSet;
    }

    public static T MapDataRowToObject<T>(DataRow row, OnError onError)
    {
        try
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            T instance = Activator.CreateInstance<T>();
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (((MemberInfo)propertyInfo).GetCustomAttributes(typeof(Ignore), false).Length == 0)
                    DataMapper.MapPropertyOfObject<T>(instance, row, propertyInfo, onError);
            }
            DefaultValue.SetDefaultValus<T>(instance, Types.Directions.WithQuery);
            return instance;
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(onError, ex, string.Empty);
        }
        return default(T);
    }

    internal static List<Parameter> MapObjectToStoredProcedure<T>(
      string command,
      T objectToMap,
      LightADOSetting setting,
      OnError onError,
      params Parameter[] parameters)
    {
        List<Parameter> parameterList = new List<Parameter>();
        try
        {
            DefaultValue.SetDefaultValus<T>(objectToMap, Types.Directions.WithNonQuery);
            foreach (StoredProcedureParameter parameter1 in new StoredProcedureParameter(command, setting).Parameters)
            {
                StoredProcedureParameter parameter = parameter1;
                string str = parameter.Name.Remove(0, 1);
                PropertyInfo property = objectToMap.GetType().GetProperty(str);
                if (property != null)
                {
                    if (((MemberInfo)property).GetCustomAttributes(typeof(ForeignKey), true).Length == 0)
                    {
                        object obj = property.GetValue((object)objectToMap);
                        if (property.PropertyType.IsEnum)
                        {
                            if (DataMapper.GetCSharpType((SqlDbType)Enum.Parse(typeof(SqlDbType), parameter.TypeName, true)) == typeof(string))
                                parameterList.Add(new Parameter(str, (object)obj.ToString(), parameter.GetParameterDirection));
                            else
                                parameterList.Add(new Parameter(str, (object)(int)obj, parameter.GetParameterDirection));
                        }
                        else
                            parameterList.Add(new Parameter(str, obj, parameter.GetParameterDirection));
                    }
                    else
                        DataMapper.GetPrimaryKeyValue<T>(objectToMap, parameterList, parameter, str, onError);
                }
                else if (Array.Find<Parameter>(parameters, (Predicate<Parameter>)(x => parameter.Name == x.Name)) != null)
                    parameterList.Add(new Parameter(str, Array.Find<Parameter>(parameters, (Predicate<Parameter>)(x => parameter.Name == x.Name)).Value, parameter.GetParameterDirection));
                else
                    DataMapper.SearchForCustomColumnNames<T>(objectToMap, parameterList, parameter, str, onError);
            }
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(onError, ex, string.Empty);
        }
        return parameterList;
    }

    private static void SearchForCustomColumnNames<T>(
      T objectToMap,
      List<Parameter> mappedParameters,
      StoredProcedureParameter parameter,
      string currentParameteNameInStoredProcedure,
      OnError onError)
    {
        try
        {
            PropertyInfo[] properties = objectToMap.GetType().GetProperties();
            Func<PropertyInfo, bool> predicate = (Func<PropertyInfo, bool>)(p => (uint)((MemberInfo)p).GetCustomAttributes(typeof(ColumnName), true).Length > 0U);
            if (predicate == null)
                return;
            foreach (PropertyInfo propertyInfo in ((IEnumerable<PropertyInfo>)properties).Where<PropertyInfo>(predicate))
            {
                string columnName = DataMapper.GetColumnName(propertyInfo, onError);
                if (columnName == currentParameteNameInStoredProcedure)
                {
                    if ((uint)((MemberInfo)objectToMap.GetType().GetProperty(((MemberInfo)propertyInfo).Name)).GetCustomAttributes(typeof(ForeignKey), false).Length > 0U)
                    {
                        DataMapper.GetPrimaryKeyValue<T>(objectToMap, mappedParameters, parameter, ((MemberInfo)propertyInfo).Name, onError, columnName);
                        break;
                    }
                    mappedParameters.Add(new Parameter(currentParameteNameInStoredProcedure, objectToMap.GetType().GetProperty(((MemberInfo)propertyInfo).Name).GetValue((object)objectToMap), parameter.GetParameterDirection));
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(onError, ex, string.Empty);
        }
    }

    private static void MapPropertyOfObject<T>(
      T item,
      DataRow row,
      PropertyInfo propertyInfo,
      OnError onError)
    {
        bool flag = (uint)((MemberInfo)propertyInfo).GetCustomAttributes(typeof(ForeignKey), false).Length > 0U;
        string columnName = DataMapper.GetColumnName(propertyInfo, onError);
        try
        {
            if (row.Table.Columns[columnName] == null)
                return;
            if (!flag)
            {
                if (((object)DBNull.Value).Equals(row[columnName]))
                    propertyInfo.SetValue((object)item, (object)null);
                else if (propertyInfo.PropertyType.IsEnum)
                    propertyInfo.SetValue((object)item, Enum.Parse(propertyInfo.PropertyType, row[columnName].ToString(), true));
                else if (row[columnName] is string)
                {
                    propertyInfo.SetValue((object)item, Convert.ChangeType((object)new string(row[columnName].ToString().Trim().Where<char>((Func<char, bool>)(c => !char.IsControl(c))).ToArray<char>()), propertyInfo.PropertyType));
                }
                else
                {
                    Type type = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
                    if ((object)type == null)
                        type = propertyInfo.PropertyType;
                    Type conversionType = type;
                    object obj = row[columnName] == null ? (object)null : Convert.ChangeType(row[columnName], conversionType);
                    propertyInfo.SetValue((object)item, obj);
                }
            }
            else
                DataMapper.MapForeignObject<T>(item, row, propertyInfo, onError);
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(onError, ex, columnName);
        }
    }

    private static string GetColumnName(PropertyInfo propertyInfo, OnError onError)
    {
        try
        {
            string name = ((MemberInfo)propertyInfo).Name;
            ColumnName customAttribute = CustomAttributeExtensions.GetCustomAttribute<ColumnName>((MemberInfo)propertyInfo, true);
            if (customAttribute != null)
                name = customAttribute.Name;
            return name;
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(onError, ex, string.Empty);
        }
        return (string)null;
    }

    private static void MapForeignObject<T>(
      T item,
      DataRow row,
      PropertyInfo propertyInfo,
      OnError onError)
    {
        try
        {
            Type propertyType = propertyInfo.PropertyType;

            if (propertyType.GetConstructor(new Type[1]
            {
                    row[DataMapper.GetColumnName(propertyInfo, onError)].GetType()
            }) == null)
                return;

            object instance = Activator.CreateInstance(propertyType, row[DataMapper.GetColumnName(propertyInfo, onError)]);
            typeof(T).InvokeMember(((MemberInfo)propertyInfo).Name, (BindingFlags)8192, (Binder)null, (object)item, new object[1]
            {
          instance
            });
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(onError, ex, string.Empty);
        }
    }

    private static void GetPrimaryKeyValue<T>(
      T objectToMap,
      List<Parameter> parameters,
      StoredProcedureParameter parameter,
      string propertyName,
      OnError onError,
      string columnName = "")
    {
        try
        {
            object obj = objectToMap.GetType().GetProperty(propertyName).GetValue((object)objectToMap);
            foreach (PropertyInfo property1 in obj.GetType().GetProperties())
            {
                if ((uint)((MemberInfo)obj.GetType().GetProperty(((MemberInfo)property1).Name)).GetCustomAttributes(typeof(PrimaryKey), false).Length > 0U)
                {
                    object onExisit = DataMapper.CreateOnExisit<T>(objectToMap.GetType().GetProperty(propertyName), objectToMap);
                    if (onExisit != null)
                    {
                        foreach (PropertyInfo property2 in onExisit.GetType().GetProperties())
                        {
                            if ((uint)((MemberInfo)onExisit.GetType().GetProperty(((MemberInfo)property2).Name)).GetCustomAttributes(typeof(PrimaryKey), false).Length > 0U)
                            {
                                if (!string.IsNullOrEmpty(columnName))
                                {
                                    parameters.Add(new Parameter(columnName, onExisit.GetType().GetProperty(((MemberInfo)property2).Name).GetValue(onExisit), parameter.GetParameterDirection));
                                    return;
                                }
                                parameters.Add(new Parameter(parameter.Name, onExisit.GetType().GetProperty(((MemberInfo)property2).Name).GetValue(onExisit), parameter.GetParameterDirection));
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(columnName))
                        {
                            parameters.Add(new Parameter(columnName, obj.GetType().GetProperty(((MemberInfo)property1).Name).GetValue(obj), parameter.GetParameterDirection));
                            return;
                        }
                        parameters.Add(new Parameter(parameter.Name.Split('@')[1], obj.GetType().GetProperty(((MemberInfo)property1).Name).GetValue(obj), parameter.GetParameterDirection));
                        return;
                    }
                }
            }
            throw new LightAdoExcption(string.Format("primary key is Not defined in {0}", (object)obj.GetType().ToString()));
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(onError, ex, string.Empty);
        }
    }

    private static object CreateOnExisit<T>(PropertyInfo property, T objectToMap)
    {
        if ((uint)((MemberInfo)property).GetCustomAttributes(typeof(CreateOnNotExists), false).Length <= 0U)
            return (object)null;
        string useThisMethod = ((CreateOnNotExists)CustomAttributeExtensions.GetCustomAttribute((MemberInfo)property, typeof(CreateOnNotExists), false)).UseThisMethod;
        if (property.PropertyType.GetMethod(useThisMethod) == null)
            throw new LightAdoExcption(string.Format("The object {0}, dont't have a method named {1}", (object)((object)property).GetType().ToString(), (object)useThisMethod));
        object instance = Activator.CreateInstance(property.PropertyType);
        return ((MethodBase)property.PropertyType.GetMethod(useThisMethod)).Invoke(instance, new object[1]
        {
        property.GetValue((object) objectToMap)
        });
    }

    private static Type GetCSharpType(SqlDbType sqltype)
    {
        Dictionary<SqlDbType, Type> dictionary = new Dictionary<SqlDbType, Type>();
        dictionary.Add((SqlDbType)0, typeof(long));
        dictionary.Add((SqlDbType)1, typeof(byte[]));
        dictionary.Add((SqlDbType)2, typeof(bool));
        dictionary.Add((SqlDbType)3, typeof(string));
        dictionary.Add((SqlDbType)31, typeof(DateTime));
        dictionary.Add((SqlDbType)4, typeof(DateTime));
        dictionary.Add((SqlDbType)33, typeof(DateTime));
        dictionary.Add((SqlDbType)34, typeof(DateTimeOffset));
        dictionary.Add((SqlDbType)5, typeof(Decimal));
        dictionary.Add((SqlDbType)6, typeof(double));
        dictionary.Add((SqlDbType)7, typeof(byte[]));
        dictionary.Add((SqlDbType)8, typeof(int));
        dictionary.Add((SqlDbType)9, typeof(Decimal));
        dictionary.Add((SqlDbType)10, typeof(string));
        dictionary.Add((SqlDbType)11, typeof(string));
        dictionary.Add((SqlDbType)12, typeof(string));
        dictionary.Add((SqlDbType)13, typeof(float));
        dictionary.Add((SqlDbType)15, typeof(DateTime));
        dictionary.Add((SqlDbType)16, typeof(short));
        dictionary.Add((SqlDbType)17, typeof(Decimal));
        dictionary.Add((SqlDbType)18, typeof(string));
        dictionary.Add((SqlDbType)32, typeof(TimeSpan));
        dictionary.Add((SqlDbType)19, typeof(byte[]));
        dictionary.Add((SqlDbType)20, typeof(byte));
        dictionary.Add((SqlDbType)14, typeof(Guid));
        dictionary.Add((SqlDbType)21, typeof(byte[]));
        dictionary.Add((SqlDbType)22, typeof(string));
        Type type;
        dictionary.TryGetValue(sqltype, out type);
        return type;
    }
}
