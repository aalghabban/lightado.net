using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightADO;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

internal class OutputParmeterHandler
{
    internal static List<Parameter> GetOutputParamters(SqlCommand sqlCommand)
    {
        List<Parameter> parameterList = new List<Parameter>();
        foreach (SqlParameter parameter in (DbParameterCollection)sqlCommand.Parameters)
        {
            if (((DbParameter)parameter).Direction == ParameterDirection.Output)
                parameterList.Add(new Parameter(((DbParameter)parameter).ParameterName, ((DbParameter)parameter).Value));
        }
        return parameterList;
    }

    internal static void SetOutputParameter<T>(
      SqlCommand sqlCommand,
      T objectToMap,
      params Parameter[] parameters)
    {
        List<Parameter> outputParamters = OutputParmeterHandler.GetOutputParamters(sqlCommand);
        if (outputParamters == null || outputParamters.Count <= 0)
            return;
        foreach (Parameter parameter1 in outputParamters)
        {
            Parameter parameter = parameter1;
            if (objectToMap.GetType().GetProperty(parameter.Name.Remove(0, 2)) != null)
                objectToMap.GetType().GetProperty(parameter.Name.Remove(0, 2)).SetValue((object)objectToMap, parameter.Value);
            else if (Array.Find<Parameter>(parameters, (Predicate<Parameter>)(x => parameter.Name.Remove(0, 1) == x.Name)) != null)
            {
                Array.Find<Parameter>(parameters, (Predicate<Parameter>)(x => parameter.Name.Remove(0, 1) == x.Name)).Value = parameter.Value;
            }
            else
            {
                PropertyInfo[] properties = objectToMap.GetType().GetProperties();
                Func<PropertyInfo, bool> predicate = (Func<PropertyInfo, bool>)(p => (uint)((MemberInfo)p).GetCustomAttributes(typeof(ColumnName), true).Length > 0U);
                if (predicate != null)
                {
                    foreach (PropertyInfo propertyInfo in ((IEnumerable<PropertyInfo>)properties).Where<PropertyInfo>(predicate))
                    {
                        if (CustomAttributeExtensions.GetCustomAttribute<ColumnName>((MemberInfo)propertyInfo, true).Name == parameter.Name.Remove(0, 2))
                            objectToMap.GetType().GetProperty(((MemberInfo)propertyInfo).Name).SetValue((object)objectToMap, parameter.Value);
                    }
                }
            }
        }
    }
}
