using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightADO;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


public class Query : QueryBase
{
    public Query()
    {
    }

    public Query(string connectionString)
      : base(connectionString)
    {
    }

    public event BeforeOpenConnection BeforeConnectionOpened;

    public event BeforeCloseConnection BeforeConnectionClosed;

    public event AfterOpenConnection AfterConnectionOpened;

    public event AfterCloseConnection AfterConnectionClosed;

    public event BeforeExecute BeforeQueryExecute;

    public event BeforeExecute AfterQueryExecute;

    public event LightADO.OnError OnError;

    public Task<DataTable> ExecuteToDataTableAsync(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      params Parameter[] parameters)
    {
        return Task.FromResult<DataTable>(this.ExecuteToDataTable(command, commandType, parameters));
    }

    public DataTable ExecuteToDataTable(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      params Parameter[] parameters)
    {
        return this.ExecuteToDataTable(SqlCommandFactory.Create(command, commandType, this.LightAdoSetting, parameters));
    }

    public Task<DataSet> ExecuteToDataSetAsync(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      params Parameter[] parameters)
    {
        return Task.FromResult<DataSet>(this.ExecuteToDataSet(command, commandType, parameters));
    }

    public DataSet ExecuteToDataSet(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      params Parameter[] parameters)
    {
        return DataMapper.ConvertDataTableToDataSet(this.ExecuteToDataTable(SqlCommandFactory.Create(command, commandType, this.LightAdoSetting, parameters)));
    }

    public Task<T> ExecuteToObjectAsync<T>(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      params Parameter[] parameters)
    {
        return Task.FromResult<T>(this.ExecuteToObject<T>(command, commandType, parameters));
    }

    public T ExecuteToObject<T>(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      params Parameter[] parameters)
    {
        return DataMapper.ConvertDataTableToObject<T>(this.ExecuteToDataTable(SqlCommandFactory.Create(command, commandType, this.LightAdoSetting, parameters)), this.OnError);
    }

    public void ExecuteToObject<T>(
      string command,
      T mapResultToThisObject,
      CommandType commandType = CommandType.StoredProcedure,
      params Parameter[] parameters)
    {
        T obj = DataMapper.ConvertDataTableToObject<T>(this.ExecuteToDataTable(SqlCommandFactory.Create(command, commandType, this.LightAdoSetting, parameters)), this.OnError);
        if ((object)obj == null)
            return;
        foreach (PropertyInfo property in obj.GetType().GetProperties())
            mapResultToThisObject.GetType().GetProperty(((MemberInfo)property).Name).SetValue((object)mapResultToThisObject, property.GetValue((object)obj));
    }

    public void ExecuteToObject<T>(
      string command,
      CrudBase<T> mapResultToThisObject,
      CommandType commandType = CommandType.StoredProcedure,
      params Parameter[] parameters)
    {
        T obj = DataMapper.ConvertDataTableToObject<T>(this.ExecuteToDataTable(SqlCommandFactory.Create(command, commandType, this.LightAdoSetting, parameters)), this.OnError);
        if ((object)obj == null)
            return;
        foreach (PropertyInfo property in obj.GetType().GetProperties())
            mapResultToThisObject.GetType().GetProperty(((MemberInfo)property).Name).SetValue((object)mapResultToThisObject, property.GetValue((object)obj));
    }

    public Task<List<T>> ExecuteToListOfObjectAsync<T>(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      params Parameter[] parameters)
    {
        return Task.FromResult<List<T>>(DataMapper.ConvertDataTableToListOfObject<T>(this.ExecuteToDataTable(SqlCommandFactory.Create(command, commandType, this.LightAdoSetting, parameters)), this.OnError));
    }

    public List<T> ExecuteToListOfObject<T>(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      params Parameter[] parameters)
    {
        return DataMapper.ConvertDataTableToListOfObject<T>(this.ExecuteToDataTable(SqlCommandFactory.Create(command, commandType, this.LightAdoSetting, parameters)), this.OnError);
    }

    public Task<string> ExecuteToObjectAsync<T>(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      Types.FormatType formatType = Types.FormatType.XML,
      params Parameter[] parameters)
    {
        return Task.FromResult<string>(this.ExecuteToObject<T>(command, commandType, formatType, parameters));
    }

    public string ExecuteToObject<T>(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      Types.FormatType formatType = Types.FormatType.XML,
      params Parameter[] parameters)
    {
        T obj = this.ExecuteToObject<T>(command, commandType, parameters);
        if ((object)obj != null)
        {
            switch (formatType)
            {
                case Types.FormatType.XML:
                    return this.SerializeToXml<T>(obj);
                case Types.FormatType.Json:
                    return JsonConvert.SerializeObject((object)obj);
            }
        }
        return (string)null;
    }

    public Task<List<string>> ExecuteToListOfObjectAsync<T>(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      Types.FormatType formatType = Types.FormatType.XML,
      params Parameter[] parameters)
    {
        return Task.FromResult<List<string>>(this.ExecuteToListOfObject<T>(command, commandType, formatType, parameters));
    }

    public List<string> ExecuteToListOfObject<T>(
      string command,
      CommandType commandType = CommandType.StoredProcedure,
      Types.FormatType formatType = Types.FormatType.XML,
      params Parameter[] parameters)
    {
        List<T> listOfObject = this.ExecuteToListOfObject<T>(command, commandType, parameters);
        List<string> stringList = (List<string>)null;
        if (listOfObject != null && listOfObject.Count > 0)
        {
            stringList = new List<string>();
            foreach (T obj in listOfObject)
            {
                switch (formatType)
                {
                    case Types.FormatType.XML:
                        stringList.Add(this.SerializeToXml<T>(obj));
                        break;
                    case Types.FormatType.Json:
                        stringList.Add(JsonConvert.SerializeObject((object)obj));
                        break;
                }
            }
        }
        return stringList;
    }

    private DataTable ExecuteToDataTable(SqlCommand command)
    {
        DataTable dataTable = (DataTable)null;
        try
        {
            if (command != null)
            {
                BeforeOpenConnection connectionOpened1 = this.BeforeConnectionOpened;
                if (connectionOpened1 != null)
                    connectionOpened1();
                if (((DbConnection)command.Connection).State == ConnectionState.Closed)
                    ((DbConnection)command.Connection).Open();
                AfterOpenConnection connectionOpened2 = this.AfterConnectionOpened;
                if (connectionOpened2 != null)
                    connectionOpened2();
                BeforeExecute beforeQueryExecute = this.BeforeQueryExecute;
                if (beforeQueryExecute != null)
                    beforeQueryExecute();
                SqlDataReader sqlDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                dataTable = new DataTable();
                dataTable.Load((IDataReader)sqlDataReader);
                BeforeExecute afterQueryExecute = this.AfterQueryExecute;
                if (afterQueryExecute != null)
                    afterQueryExecute();
            }
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(this.OnError, ex, string.Empty);
        }
        finally
        {
            BeforeCloseConnection connectionClosed1 = this.BeforeConnectionClosed;
            if (connectionClosed1 != null)
                connectionClosed1();
            if (((DbConnection)command.Connection).State == ConnectionState.Open)
                ((DbConnection)command.Connection).Close();
            AfterCloseConnection connectionClosed2 = this.AfterConnectionClosed;
            if (connectionClosed2 != null)
                connectionClosed2();
        }
        return dataTable;
    }

    private string SerializeToXml<T>(T value)
    {
        if ((object)value == null)
            return string.Empty;
        try
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringWriter stringWriter = new StringWriter();
            using (XmlWriter xmlWriter = XmlWriter.Create((TextWriter)stringWriter))
            {
                xmlSerializer.Serialize(xmlWriter, (object)value);
                return ((object)stringWriter).ToString();
            }
        }
        catch (Exception ex)
        {
            throw new LightAdoExcption(ex);
        }
    }
}
