using System.Collections.Generic;
using System.IO;
using Wizards.Mtga.IO;

namespace Wotc.Mtga;

public static class DataSourceUtilities
{
	public const string BuildDataSources = "BuildDataSources";

	private const string PLAYERPREFS_KEY_EDITOR_DATA_SOURCE = "EditorDataSource";

	private const string DIRECTORY_NAME_BUILD_DATA_SOURCES = "BuildDataSources";

	public const string ForceInclude = "ForceInclude";

	public const string Internal = "Internal";

	public const string External = "External";

	public static void SetCurrentDataSource(string dataSource)
	{
		PlayerPrefsExt.SetString("EditorDataSource", dataSource);
		PlayerPrefsExt.Save();
	}

	public static string GetForceIncludeDirectory()
	{
		return Path.Combine(Directory.GetCurrentDirectory(), "BuildDataSources", "ForceInclude");
	}

	public static string GetCurrentDataSource()
	{
		string text = PlayerPrefsExt.GetString("EditorDataSource", string.Empty);
		string path = Path.Combine(Directory.GetCurrentDirectory(), "BuildDataSources", text);
		if (text == string.Empty || !WindowsSafePath.DirectoryExists(path))
		{
			text = "External";
		}
		return text;
	}

	public static string GetCurrentDataSourceDirectory()
	{
		return Path.Combine(Directory.GetCurrentDirectory(), "BuildDataSources", GetCurrentDataSource());
	}

	public static List<string> GetAvailableDataSources()
	{
		List<string> list = new List<string>();
		DirectoryInfo directoryInfo = new DirectoryInfo("BuildDataSources");
		if (directoryInfo.Exists)
		{
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			foreach (DirectoryInfo directoryInfo2 in directories)
			{
				if (directoryInfo2.GetFiles("CardDatabase.sqlite").Length == 1)
				{
					list.Add(directoryInfo2.Name);
				}
			}
		}
		return list;
	}
}
