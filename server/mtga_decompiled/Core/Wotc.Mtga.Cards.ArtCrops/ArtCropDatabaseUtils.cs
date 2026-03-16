using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc.CachingPatterns;

namespace Wotc.Mtga.Cards.ArtCrops;

public static class ArtCropDatabaseUtils
{
	public const string CARD_ART_FOLDER_ROOT = "Assets/Core/CardArt/";

	public const string OLD_MONOLITHIC_DATABASE_PATH = "Assets/Core/Shared/Code/Tools/CardArtCropDatabase/ArtCropDatabase.json";

	public static JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
	{
		Formatting = Formatting.Indented,
		DefaultValueHandling = DefaultValueHandling.Ignore,
		NullValueHandling = NullValueHandling.Ignore,
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore
	};

	public static IArtCropProvider LoadBestProvider(IBILogger biLogger)
	{
		return LoadProviderFromSql(biLogger);
	}

	public static IArtCropProvider LoadProviderFromSql(IBILogger biLogger)
	{
		string text = AssetLoader.GetRawFilePaths(string.Empty, "ArtCropDatabase.sqlite").FirstOrDefault();
		if (File.Exists(text))
		{
			return new SqlArtCropProvider(text, biLogger, new DictionaryCache<string, ArtCropFormat>(10), new DictionaryCache<(string, string), ArtCrop>(250), Pantry.Get<ISqlHelper>());
		}
		return null;
	}

	public static IArtCropProvider LoadProviderFromPackedJson()
	{
		string text = AssetLoader.GetRawFilePaths(string.Empty, "ArtCropDatabase_Packed.json").FirstOrDefault();
		if (File.Exists(text))
		{
			return new JsonArtCropProvider(text);
		}
		return null;
	}

	public static IArtCropProvider LoadProviderFromSplitJson()
	{
		IEnumerable<string> rawFilePaths = AssetLoader.GetRawFilePaths("CardArt/Formats", string.Empty);
		IEnumerable<string> rawFilePaths2 = AssetLoader.GetRawFilePaths("CardArt/Definitions", string.Empty);
		return new JsonArtCropProvider(rawFilePaths, rawFilePaths2);
	}
}
