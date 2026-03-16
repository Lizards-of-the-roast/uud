using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using UnityEngine;
using Wizards.Mtga;

namespace Wotc.Mtga.Extensions;

public class UnitySqlHelper : ISqlHelper
{
	private ISQLite3Provider _provider;

	public void TryInit()
	{
		if (_provider == null)
		{
			_provider = new SQLite3Provider_sqlite3();
			raw.SetProvider(_provider);
		}
	}

	public void CheckedPrepare(SqliteCommand command, IBILogger biLogger)
	{
		try
		{
			command.Connection?.Open();
			if (command.Connection == null)
			{
				SimpleLog.LogErrorFormat("Trying to run a SQLite command without a connection established to the database.\nCommand: {0}\nStack:{1}", command.CommandText, StackTraceUtility.ExtractStackTrace());
			}
			command.Prepare();
		}
		catch (SqliteException ex)
		{
			if (ex.SqliteErrorCode == 11)
			{
				MDNPlayerPrefs.FileToHashOnStartup = command.Connection.DataSource;
				ResourceErrorLogger.LogAssetBundleError(biLogger, ex.GetType().Name + ": " + ex.Message, new Dictionary<string, string>
				{
					["Database"] = command.Connection.DataSource,
					["ConnectionState"] = command.Connection.State.ToString(),
					["CommandType"] = command.CommandType.ToString(),
					["CommandText"] = command.CommandText
				});
			}
			throw;
		}
	}

	public SqliteDataReader CheckedExecuteReader(SqliteCommand command, CommandBehavior commandBehavior, IBILogger biLogger)
	{
		try
		{
			return command.ExecuteReader(commandBehavior);
		}
		catch (SqliteException ex)
		{
			if (ex.SqliteErrorCode == 11)
			{
				MDNPlayerPrefs.FileToHashOnStartup = command.Connection.DataSource;
				ResourceErrorLogger.LogAssetBundleError(biLogger, ex.GetType().Name + ": " + ex.Message, new Dictionary<string, string>
				{
					["Database"] = command.Connection.DataSource,
					["ConnectionState"] = command.Connection.State.ToString(),
					["CommandType"] = command.CommandType.ToString(),
					["CommandText"] = command.CommandText
				});
			}
			throw;
		}
	}
}
