using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc.CachingPatterns;

namespace Wotc.Mtga.Cards.ArtCrops;

public class SqlArtCropProvider : IArtCropProvider, IDisposable
{
	private const string SELECT_FORMAT_COMMAND = "SELECT Width, Height, Fudge, Behavior FROM Formats WHERE Name = @name";

	private const string SELECT_CROP_COMMAND = "SELECT X, Y, Z, W, Generated FROM Crops WHERE Path = @path AND Format = @format";

	private const string SELECT_ALL_FORMATS_COMMAND = "SELECT Name FROM Formats";

	private const string SELECT_ALL_ART_PATHS_COMMAND = "SELECT Path FROM Crops";

	private readonly SqliteConnection _dbConnection;

	private readonly SqliteCommand _dbGetCropCommand;

	private readonly SqliteCommand _dbGetFormatCommand;

	private readonly IBILogger _biLogger;

	private readonly ICachingPattern<string, ArtCropFormat> _formatsCache;

	private readonly ICachingPattern<(string, string), ArtCrop> _cropsCache;

	private IReadOnlyCollection<string> _allArtPathsCache;

	private IReadOnlyCollection<string> _allFormatNamesCache;

	public SqlArtCropProvider(string dbPath, IBILogger biLogger, ICachingPattern<string, ArtCropFormat> formatsCache, ICachingPattern<(string, string), ArtCrop> cropsCache, ISqlHelper sqlHelper)
	{
		_biLogger = biLogger ?? new NullBILogger();
		_formatsCache = formatsCache;
		_cropsCache = cropsCache;
		SqlHelperExtensions.TryInitSql(sqlHelper);
		string connectionString = new SqliteConnectionStringBuilder
		{
			DataSource = dbPath,
			Cache = SqliteCacheMode.Private,
			Mode = SqliteOpenMode.ReadOnly,
			Pooling = false
		}.ToString();
		_dbConnection = new SqliteConnection(connectionString);
		_dbConnection.Open();
		_dbGetCropCommand = new SqliteCommand("SELECT X, Y, Z, W, Generated FROM Crops WHERE Path = @path AND Format = @format", _dbConnection)
		{
			Parameters = 
			{
				new SqliteParameter("@path", SqliteType.Text, 100)
				{
					IsNullable = false
				},
				new SqliteParameter("@format", SqliteType.Text, 50)
				{
					IsNullable = false
				}
			}
		};
		_dbGetFormatCommand = new SqliteCommand("SELECT Width, Height, Fudge, Behavior FROM Formats WHERE Name = @name", _dbConnection)
		{
			Parameters = 
			{
				new SqliteParameter("@name", SqliteType.Text, 50)
				{
					IsNullable = false
				}
			}
		};
		_dbGetCropCommand.CheckedPrepare(_biLogger);
		_dbGetFormatCommand.CheckedPrepare(_biLogger);
	}

	public ArtCrop GetCrop(string artPath, string formatName)
	{
		ArtCrop value = ArtCrop.DEFAULT;
		if (artPath == null || formatName == null)
		{
			return value;
		}
		(string, string) key = (artPath, formatName);
		bool flag = _cropsCache.TryGetCached(key, out value);
		if (!flag)
		{
			lock (_dbConnection)
			{
				_dbGetCropCommand.Parameters[0].Value = artPath;
				_dbGetCropCommand.Parameters[1].Value = formatName;
				using SqliteDataReader sqliteDataReader = _dbGetCropCommand.CheckedExecuteReader(CommandBehavior.SingleRow, _biLogger);
				if (sqliteDataReader.Read())
				{
					value = new ArtCrop
					{
						ScaleOffset = new Vector4
						{
							x = sqliteDataReader.GetFloat(0),
							y = sqliteDataReader.GetFloat(1),
							z = sqliteDataReader.GetFloat(2),
							w = sqliteDataReader.GetFloat(3)
						},
						Generated = sqliteDataReader.GetBoolean(4)
					};
					flag = true;
				}
			}
		}
		if (!flag)
		{
			int num = artPath.IndexOf("_AIF", StringComparison.InvariantCultureIgnoreCase);
			if (num > 0 && num + 4 < artPath.Length)
			{
				string artPath2 = artPath.Remove(num + 4);
				value = GetCrop(artPath2, formatName);
			}
		}
		_cropsCache.SetCached(key, value);
		return value;
	}

	public ArtCropFormat GetFormat(string formatName)
	{
		ArtCropFormat value = null;
		if (formatName != null && !_formatsCache.TryGetCached(formatName, out value))
		{
			lock (_dbConnection)
			{
				_dbGetFormatCommand.Parameters[0].Value = formatName;
				using SqliteDataReader sqliteDataReader = _dbGetFormatCommand.CheckedExecuteReader(CommandBehavior.SingleRow, _biLogger);
				value = ((!sqliteDataReader.Read()) ? null : new ArtCropFormat
				{
					Dimensions = new Vector2
					{
						x = sqliteDataReader.GetInt32(0),
						y = sqliteDataReader.GetInt32(1)
					},
					FudgePercentage = sqliteDataReader.GetFloat(2),
					UnsatisfiedBehavior = sqliteDataReader.GetFieldValue<ArtCropFormat.UnsatisfiedBehaviorType>(3)
				});
				_formatsCache.SetCached(formatName, value);
			}
		}
		return value;
	}

	public IEnumerable<string> GetArtPaths()
	{
		lock (_dbConnection)
		{
			if (_allArtPathsCache == null)
			{
				HashSet<string> hashSet = new HashSet<string>();
				using SqliteCommand command = new SqliteCommand("SELECT Path FROM Crops", _dbConnection);
				using SqliteDataReader sqliteDataReader = command.CheckedExecuteReader(CommandBehavior.Default, _biLogger);
				while (sqliteDataReader.Read())
				{
					string item = sqliteDataReader.GetString(0);
					hashSet.Add(item);
				}
				_allArtPathsCache = hashSet;
			}
		}
		return _allArtPathsCache;
	}

	public IEnumerable<string> GetFormatNames()
	{
		lock (_dbConnection)
		{
			if (_allFormatNamesCache == null)
			{
				HashSet<string> hashSet = new HashSet<string>();
				using SqliteCommand command = new SqliteCommand("SELECT Name FROM Formats", _dbConnection);
				using SqliteDataReader sqliteDataReader = command.CheckedExecuteReader(CommandBehavior.Default, _biLogger);
				while (sqliteDataReader.Read())
				{
					string item = sqliteDataReader.GetString(0);
					hashSet.Add(item);
				}
				_allFormatNamesCache = hashSet;
			}
		}
		return _allFormatNamesCache;
	}

	public void Dispose()
	{
		_dbConnection?.Close();
		_dbConnection?.Dispose();
		_dbGetCropCommand?.Dispose();
		_dbGetFormatCommand?.Dispose();
		_cropsCache?.ClearCache();
		_formatsCache?.ClearCache();
		_allArtPathsCache = null;
		_allFormatNamesCache = null;
	}
}
