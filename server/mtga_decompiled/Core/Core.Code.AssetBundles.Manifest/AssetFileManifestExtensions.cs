using System;
using System.IO;
using Newtonsoft.Json;
using Wizards.Mtga.Assets;
using Wizards.Mtga.IO;
using Wizards.Mtga.Utils;

namespace Core.Code.AssetBundles.Manifest;

public static class AssetFileManifestExtensions
{
	private static readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
	{
		Formatting = Formatting.Indented,
		Converters = 
		{
			(JsonConverter)new AssetFileManifestConverter(),
			(JsonConverter)new AssetFileInfoConverter(new StringCache())
		}
	});

	public static bool Contains(this IAssetFileManifest me, string fileName)
	{
		return me.AssetFileInfoByName.ContainsKey(fileName);
	}

	public static bool Contains(this IAssetFileManifest me, AssetFileInfo assetInfo)
	{
		return me.Contains(assetInfo.Name);
	}

	public static AssetFileManifest LoadFromFile(string sourceFile)
	{
		if (!WindowsSafePath.FileExists(sourceFile))
		{
			return null;
		}
		try
		{
			return LoadFrom(new StreamReader(WindowsSafePath.OpenFile(sourceFile, FileMode.Open)));
		}
		catch (Exception exception)
		{
			throw new ManifestLoadException(exception);
		}
	}

	public static AssetFileManifest LoadFrom(TextReader reader)
	{
		using JsonTextReader reader2 = new JsonTextReader(reader);
		return _jsonSerializer.Deserialize<AssetFileManifest>(reader2);
	}

	public static void Serialize(this AssetFileManifest assetFileManifest, StreamWriter writer)
	{
		using JsonTextWriter jsonTextWriter = new JsonTextWriter(writer);
		jsonTextWriter.Formatting = Formatting.Indented;
		jsonTextWriter.Indentation = 2;
		_jsonSerializer.Serialize(jsonTextWriter, assetFileManifest);
	}
}
