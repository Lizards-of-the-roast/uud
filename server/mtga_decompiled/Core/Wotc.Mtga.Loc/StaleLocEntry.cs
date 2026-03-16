namespace Wotc.Mtga.Loc;

public readonly struct StaleLocEntry
{
	public readonly string Key;

	public readonly string MinVersion;

	public readonly string LocalText;

	public readonly string CurrentText;

	public readonly bool IsMultiline;

	public StaleLocEntry(string key, string minVersion, string local, string current)
	{
		Key = key;
		MinVersion = minVersion;
		LocalText = local ?? string.Empty;
		CurrentText = current ?? string.Empty;
		IsMultiline = LocalText.Contains('\n') || CurrentText.Contains('\n');
	}
}
