using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Wotc.Mtga.Cards.ArtCrops;

public class JsonArtCropProvider : IArtCropProvider
{
	public SortedDictionary<string, ArtCropFormat> FormatsByType = new SortedDictionary<string, ArtCropFormat>();

	public SortedDictionary<string, SortedDictionary<string, ArtCrop>> Crops = new SortedDictionary<string, SortedDictionary<string, ArtCrop>>();

	public JsonArtCropProvider(string packedJsonPath)
	{
		JsonConvert.PopulateObject(File.ReadAllText(packedJsonPath), this, ArtCropDatabaseUtils.DefaultSerializerSettings);
	}

	public JsonArtCropProvider(IEnumerable<string> formatPaths, IEnumerable<string> cropPaths)
	{
		foreach (string formatPath in formatPaths)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(formatPath);
			ArtCropFormat value = JsonConvert.DeserializeObject<ArtCropFormat>(File.ReadAllText(formatPath));
			FormatsByType[fileNameWithoutExtension] = value;
		}
		foreach (string cropPath in cropPaths)
		{
			string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(cropPath);
			string key = Path.Combine("Assets/Core/CardArt/", cropPath.Split('/', '\\')[^2], fileNameWithoutExtension2).Replace('\\', '/');
			SortedDictionary<string, ArtCrop> value2 = JsonConvert.DeserializeObject<SortedDictionary<string, ArtCrop>>(File.ReadAllText(cropPath));
			Crops[key] = value2;
		}
	}

	public ArtCrop GetCrop(string artPath, string format)
	{
		if (string.IsNullOrEmpty(artPath) || string.IsNullOrEmpty(format))
		{
			return null;
		}
		if (Crops.TryGetValue(artPath, out var value) && value.TryGetValue(format, out var value2))
		{
			return value2;
		}
		int num = artPath.LastIndexOf("_AIF", StringComparison.InvariantCultureIgnoreCase);
		if (num > 0 && !artPath.EndsWith("_E") && num + 4 < artPath.Length && Crops.TryGetValue(artPath.Remove(num + 4), out value) && value.TryGetValue(format, out value2))
		{
			return value2;
		}
		return null;
	}

	public ArtCropFormat GetFormat(string cropType)
	{
		if (!FormatsByType.TryGetValue(cropType, out var value))
		{
			return null;
		}
		return value;
	}

	public IEnumerable<string> GetArtPaths()
	{
		return Crops.Keys;
	}

	public IEnumerable<string> GetFormatNames()
	{
		return FormatsByType.Keys;
	}
}
