namespace Wotc.Mtga.AutoPlay;

public class AutoPlayScriptMetadata
{
	public string Name;

	public string Description;

	public bool CanRunImmediate = true;

	public bool CanRunOnRestart = true;

	public string[] PreprocessLine(string line)
	{
		if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
		{
			return null;
		}
		string[] array = line.Split('|');
		if (array[0] == "Name")
		{
			Name = array[1];
			return null;
		}
		if (array[0] == "Description")
		{
			Description = array[1];
			return null;
		}
		if (array[0] == "DisableImmediateRun")
		{
			CanRunImmediate = false;
			return null;
		}
		if (array[0] == "DisableRestartRun")
		{
			CanRunOnRestart = false;
			return null;
		}
		return array;
	}
}
