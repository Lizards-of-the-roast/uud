using System.IO;

namespace Wotc.Mtga.Replays;

public class ReplayInfo
{
	public string ReplayPath { get; private set; }

	public ReplayFormat Format { get; private set; }

	public string Name => Path.GetFileName(ReplayPath);

	public ReplayInfo(string path, ReplayFormat format)
	{
		ReplayPath = path;
		Format = format;
	}
}
