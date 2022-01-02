namespace LightADO;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;


public class NonQuery : QueryBase
{
    public NonQuery()
    {
    }

    public NonQuery(string connectionString)
      : base(connectionString)
    {
    }

    public event BeforeExecute BeforeNonQueryExecute;

    public event AfterExecute AfterNonQueryExecute;

    public event BeforeOpenConnection BeforeConnectionOpened;

    public event BeforeCloseConnection BeforeConnectionClosed;

    public event AfterOpenConnection AfterConnectionOpened;

    public event AfterCloseConnection AfterConnectionClosed;

    public event OnError OnError;

    public bool Execute(string command, CommandType commandType = CommandType.Text, params Parameter[] parameters) => this.ExcecuteNonQueryCommand(SqlCommandFactory.Create(command, commandType, this.LightAdoSetting, parameters));

    public Task<bool> ExecuteAsync(
      string command,
      CommandType commandType = CommandType.Text,
      params Parameter[] parameters)
    {
        return Task.FromResult<bool>(this.Execute(command, commandType, parameters));
    }

    public Task<bool> ExecuteAsync<T>(
      string command,
      T objectToMap,
      params Parameter[] parameters)
    {
        return Task.FromResult<bool>(this.Execute<T>(command, objectToMap, parameters));
    }

    public bool Execute<T>(string command, T objectToMap, params Parameter[] parameters)
    {
        AutoValidation.ValidateObject<T>(objectToMap);
        EncryptEngine.EncryptOrDecryptObject<T>(objectToMap, Types.OprationType.Encrypt);
        return this.ExcecuteNonQueryCommand<T>(SqlCommandFactory.Create(command, CommandType.StoredProcedure, this.LightAdoSetting, DataMapper.MapObjectToStoredProcedure<T>(command, objectToMap, this.LightAdoSetting, this.OnError, parameters).ToArray()), objectToMap, parameters);
    }

    public Task<bool> ExecuteAsync<T>(
      string command,
      List<T> objectToMap,
      params Parameter[] parameters)
    {
        return Task.FromResult<bool>(this.Execute<T>(command, objectToMap, parameters));
    }

    public bool Execute<T>(string command, List<T> objectToMap, params Parameter[] parameters)
    {
        foreach (T objectTo in objectToMap)
        {
            AutoValidation.ValidateObject<T>(objectTo);
            EncryptEngine.EncryptOrDecryptObject<T>(objectTo, Types.OprationType.Encrypt);
            this.ExcecuteNonQueryCommand<T>(SqlCommandFactory.Create(command, CommandType.StoredProcedure, this.LightAdoSetting, DataMapper.MapObjectToStoredProcedure<T>(command, objectTo, this.LightAdoSetting, this.OnError, parameters).ToArray()), objectTo, parameters);
        }
        return true;
    }

    public Task<bool> ExecuteAsync(List<Transaction> transactions, bool rollbackOnError = true) => Task.FromResult<bool>(this.Execute(transactions, rollbackOnError));

    public bool Execute(List<Transaction> transactions, bool rollbackOnError = true)
    {
        SqlConnection sqlConnection = new SqlConnection(this.LightAdoSetting.ConnectionString);
        ((DbConnection)sqlConnection).Open();
        SqlTransaction transaction1 = sqlConnection.BeginTransaction();
        try
        {
            foreach (Transaction transaction2 in transactions)
                this.ExcecuteNonQueryCommand<object>(SqlCommandFactory.Create(transaction2.Command, transaction2.CommandType, this.LightAdoSetting, transaction1, DataMapper.MapObjectToStoredProcedure<object>(transaction2.Command, transaction2.Data, this.LightAdoSetting, this.OnError, transaction2.Parameters).ToArray()), transaction2.Data, true, transaction2.Parameters);
            ((DbTransaction)transaction1).Commit();
            ((DbConnection)sqlConnection).Close();
        }
        catch (Exception ex)
        {
            if (rollbackOnError)
                ((DbTransaction)transaction1).Rollback();
            QueryBase.ThrowExacptionOrEvent(this.OnError, ex, string.Empty);
        }
        return false;
    }

    private SqlCommand ExcecuteNonQueryAndGetSqlCommand(
      SqlCommand sqlCommand,
      bool keepConnectionOpend = false)
    {
        try
        {
            BeforeOpenConnection connectionOpened1 = this.BeforeConnectionOpened;
            if (connectionOpened1 != null)
                connectionOpened1();
            if (((DbConnection)sqlCommand.Connection).State == ConnectionState.Closed)
                ((DbConnection)sqlCommand.Connection).Open();
            AfterOpenConnection connectionOpened2 = this.AfterConnectionOpened;
            if (connectionOpened2 != null)
                connectionOpened2();
            BeforeExecute beforeNonQueryExecute = this.BeforeNonQueryExecute;
            if (beforeNonQueryExecute != null)
                beforeNonQueryExecute();
            ((DbCommand)sqlCommand).ExecuteNonQuery();
            AfterExecute afterNonQueryExecute = this.AfterNonQueryExecute;
            if (afterNonQueryExecute != null)
                afterNonQueryExecute();
        }
        catch (Exception ex)
        {
            QueryBase.ThrowExacptionOrEvent(this.OnError, ex, string.Empty);
        }
        finally
        {
            if (!keepConnectionOpend)
            {
                BeforeCloseConnection connectionClosed1 = this.BeforeConnectionClosed;
                if (connectionClosed1 != null)
                    connectionClosed1();
                if (((DbConnection)sqlCommand.Connection).State == ConnectionState.Open)
                    ((DbConnection)sqlCommand.Connection).Close();
                AfterCloseConnection connectionClosed2 = this.AfterConnectionClosed;
                if (connectionClosed2 != null)
                    connectionClosed2();
            }
        }
        return sqlCommand;
    }

    private bool ExcecuteNonQueryCommand(SqlCommand sqlCommand)
    {
        this.ExcecuteNonQueryAndGetSqlCommand(sqlCommand);
        return true;
    }

    private bool ExcecuteNonQueryCommand<T>(
      SqlCommand sqlCommand,
      T objectToMap,
      params Parameter[] parameters)
    {
        OutputParmeterHandler.SetOutputParameter<T>(this.ExcecuteNonQueryAndGetSqlCommand(sqlCommand), objectToMap, parameters);
        return true;
    }

    private bool ExcecuteNonQueryCommand<T>(
      SqlCommand sqlCommand,
      T objectToMap,
      bool keepConnectionOpend = false,
      params Parameter[] parameters)
    {
        OutputParmeterHandler.SetOutputParameter<T>(this.ExcecuteNonQueryAndGetSqlCommand(sqlCommand, keepConnectionOpend), objectToMap, parameters);
        return true;
    }
}
