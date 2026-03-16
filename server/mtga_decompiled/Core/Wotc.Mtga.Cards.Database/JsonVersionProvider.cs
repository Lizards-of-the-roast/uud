using System;
using System.IO;
using Newtonsoft.Json;

namespace Wotc.Mtga.Cards.Database;

public class JsonVersionProvider : IVersionProvider
{
	public Version DataVersion { get; }

	public Version GrpVersion { get; }

	public JsonVersionProvider(string versionDataPath)
	{
		using TextReader reader = FileSystemUtils.OpenText(versionDataPath);
		using JsonTextReader jsonTextReader = new JsonTextReader(reader);
		jsonTextReader.Read();
		jsonTextReader.Read();
		jsonTextReader.Read();
		DataVersion = Version.Parse((string)jsonTextReader.Value);
		jsonTextReader.Read();
		jsonTextReader.Read();
		GrpVersion = Version.Parse((string)jsonTextReader.Value);
		jsonTextReader.Read();
	}
}
