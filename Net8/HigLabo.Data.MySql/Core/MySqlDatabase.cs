﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace HigLabo.Data;

public partial class MySqlDatabase : Database
{
    public MySqlDatabase(String connectionString)
    {
        this.ConnectionString = connectionString;
    }
    public MySqlDatabase(String serverName, String databaseName)
    {
        this.ConnectionString = MySqlDatabaseConnectionString.Create(serverName, databaseName, 100);
    }
    public MySqlDatabase(String serverName, String databaseName, String userID, String password)
    {
        this.ConnectionString = MySqlDatabaseConnectionString.Create(serverName, databaseName, userID, password, 100);
    }
    public MySqlDatabase(MySqlDatabaseConnectionString connectionString)
    {
        this.ConnectionString = connectionString.Create();
    }
    protected override DbConnection CreateDbConnection()
    {
        return new MySqlConnection();
    }
    protected override DbCommand CreateDbCommand()
    {
        return new MySqlCommand();
    }
    public override DbDataAdapter CreateDataAdapter()
    {
        return new MySqlDataAdapter();
    }
    public override DbParameter CreateParameter(string name, Enum dbType, byte? precision, byte? scale)
    {
        if (dbType is MySqlDbType)
        {
            var p = new MySqlParameter(name, (MySqlDbType)dbType);
            if (precision.HasValue == true) p.Precision = precision.Value;
            if (scale.HasValue == true) p.Scale = scale.Value;
            return p;
        }
        throw new ArgumentException("dbType must be MySqlDbType.");
    }
    public MySqlParameter CreateParameter(string parameterName, MySqlDbType dbType, object value)
    {
        return (MySqlParameter)base.CreateParameter(parameterName, dbType, value);
    }

    public Int32 ExecuteCommand(MySqlScript script)
    {
        var affectRecordNumber = Int32.MinValue;
        ConnectionState state = this.ConnectionState;
        DateTimeOffset? startTime = null;
        DateTimeOffset? endTime = null;
        Object? ec = null;

        try
        {
            var e = this.OnCommandExecuting(new MySqlScriptExecutingEventArgs(this.ConnectionString, script));
            if (e != null && e.Cancel == true) { return -1; }
            if (e != null)
            {
                ec = e.ExecutionContext;
            }

            this.Open();
            script.Connection = this.Connection as MySqlConnection;
            startTime = DateTimeOffset.Now;
            affectRecordNumber = script.Execute();
            endTime = DateTimeOffset.Now;
        }
        catch (Exception ex)
        {
            this.CatchException(new MySqlScriptErrorEventArgs(this.ConnectionString, ex, ec, script));
        }
        finally
        {
            if (state == ConnectionState.Closed)
            {
                this.Close();
            }
        }
        if (startTime.HasValue == true && endTime.HasValue == true)
        {
            this.OnCommandExecuted(new MySqlScriptExecutedEventArgs(this.ConnectionString
                , startTime.Value, endTime.Value, ec, script));
        }
        return affectRecordNumber;
    }

    protected override Exception CreateException(Exception exception)
    {
        var ex = exception as MySql.Data.MySqlClient.MySqlException;
        if (ex == null) { return new DatabaseException(exception); }

        switch (ex.ErrorCode)
        {
            case 1022: return new UniqueConstraintException(ex);
            case 1044:
            case 1045:
                return new LoginException(ex);
            case 1159:
            case 1161:
                return new TimeoutException(ex);
            case 1205:
                return new LockTimeoutException(ex);
            case 1213:
                return new DeadLockException(ex);
            case 1215:
            case 1216:
            case 1217:
                return new ConstraintException(ex);
            case 2002:
            case 2003:
                return new ConnectionException(ex);
            default: return new DatabaseException(exception);
        }
    }
    protected override Exception CreateException(CommandErrorEventArgs e)
    {
        var exception = base.CreateException(e);
        var ex = exception as DatabaseException;
        var ee = e as MySqlScriptErrorEventArgs;
        if (ex != null && ee != null)
        {
            ex.Data["MySqlScript"] = ee.MySqlScript;
        }
        return exception;
    }
}
