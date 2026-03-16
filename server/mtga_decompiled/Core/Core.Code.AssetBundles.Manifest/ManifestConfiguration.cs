using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Code.AssetBundles.Manifest;

public class ManifestConfiguration
{
	private const string Filename = "manifest.conf";

	public List<string> CategoriesToSkip = new List<string>();

	private ManifestConfiguration()
	{
	}

	public static ManifestConfiguration Load()
	{
		string path = Path.Combine("Configuration", "manifest.conf");
		if (!Application.isEditor || !File.Exists(path))
		{
			return new ManifestConfiguration();
		}
		using JsonTextReader reader = new JsonTextReader(new StreamReader(path));
		return new JsonSerializer().Deserialize<ManifestConfiguration>(reader);
	}
}
