using System;
using System.IO;

namespace Wotc.Mtga.AutoPlay;

public class AutoPlayScriptFactory
{
	private static string[] GetFileContents(string filename)
	{
		string path = (filename.Contains(".autoplay") ? filename : (filename + ".autoplay"));
		string path2 = Path.Combine(new DirectoryInfo(AutoPlayManager.GetConfigRoot).ToString(), path);
		if (File.Exists(path2))
		{
			return File.ReadAllLines(path2);
		}
		return null;
	}

	public static AutoPlayScript CreateAutoPlayScript(string filename, Action<string> logAction, AutoPlayComponentGetters componentGetters, AutoPlayManager autoPlayManager)
	{
		string[] fileContents = GetFileContents(filename);
		if (fileContents == null)
		{
			logAction("File " + filename + " cannot be found");
			return null;
		}
		return new AutoPlayScript(in fileContents, logAction, componentGetters, autoPlayManager);
	}

	public static AutoPlayScriptMetadata CreateAutoplayMetadata(string filename)
	{
		string[] fileContents = GetFileContents(filename);
		if (fileContents == null)
		{
			return null;
		}
		AutoPlayScriptMetadata autoPlayScriptMetadata = new AutoPlayScriptMetadata();
		string[] array = fileContents;
		foreach (string line in array)
		{
			autoPlayScriptMetadata.PreprocessLine(line);
		}
		return autoPlayScriptMetadata;
	}
}
