﻿using System;
using System.Data;
using System.Data.Common;
using System.Text;

namespace HigLabo.Data;

public class CommandExecutedEventArgs : EventArgs
{
    public MethodName MethodName { get; private set; }
    public string ConnectionString { get; private set; } = "";
    public DbCommand? Command { get; private set; }
    public DbDataAdapter? DataAdapter { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public TimeSpan Duration
    {
        get { return this.EndTime - this.StartTime; }
    }
    public Object? ExecutionContext { get; set; }

    protected CommandExecutedEventArgs(MethodName methodName, String connectionString, DateTimeOffset startTime, DateTimeOffset endTime, Object? executionContext)
    {
        this.MethodName = methodName;
        this.ConnectionString = connectionString;
        this.StartTime = startTime;
        this.EndTime = endTime;
        this.ExecutionContext = executionContext;
    }

    public CommandExecutedEventArgs(MethodName methodName, String connectionString, DateTimeOffset startTime, DateTimeOffset endTime, Object? executionContext, DbCommand command)
        : this(methodName, connectionString, startTime, endTime, executionContext)
    {
        this.Command = command;
    }
    public CommandExecutedEventArgs(MethodName methodName, String connectionString, DateTimeOffset startTime, DateTimeOffset endTime, Object? executionContext, DbDataAdapter dataAdapter)
        : this(methodName, connectionString, startTime, endTime, executionContext)
    {
        this.DataAdapter = dataAdapter;
    }

    public override string ToString()
    {
        var sb = new StringBuilder(128);
        try
        {
            if (String.IsNullOrEmpty(ConnectionString) == false)
            {
                sb.Append(ConnectionString);
                sb.Append(" ");
            }
            if (Command != null)
            {
                sb.Append(Command.CommandText);
                sb.Append(" ");
            }
            return sb.ToString();
        }
        catch { return nameof(CommandExecutedEventArgs); }
    }
}
