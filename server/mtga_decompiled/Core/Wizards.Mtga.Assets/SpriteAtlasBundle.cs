using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.U2D;

namespace Wizards.Mtga.Assets;

public static class SpriteAtlasBundle
{
	public const string SpriteAtlasBasePath = "Assets/Core/Art/SpriteAtlas";

	private static Lazy<Regex> AssetShortNameRegex = new Lazy<Regex>(() => new Regex("^.*/([^/]+)\\.[a-zA-Z0-9]+$"));

	private static Lazy<Regex> SpriteAtlasAssetShortNameRegex = new Lazy<Regex>(() => new Regex("^.*/([^/]+)\\.spriteatlas$"));

	public static string GetSpriteAtlasAssetPath(string name)
	{
		return "Assets/Core/Art/SpriteAtlas/" + name + ".spriteatlas";
	}

	public static Sprite? GetSpriteByAssetPath(this SpriteAtlas atlas, string assetPath)
	{
		Match match = AssetShortNameRegex.Value.Match(assetPath);
		if (match.Success)
		{
			return atlas.GetSprite(match.Groups[1].Value);
		}
		return null;
	}

	public static string? GetSpriteAtlasShortName(string assetPath)
	{
		Match match = SpriteAtlasAssetShortNameRegex.Value.Match(assetPath);
		if (match.Success)
		{
			return match.Groups[1].Value;
		}
		return null;
	}

	public static string GetSpriteAtlasBundleName(string assetPath)
	{
		return "Atlas_" + GetSpriteAtlasShortName(assetPath) + ".mtga";
	}

	public static HashSet<string> CreateAtlasBundlesNameHashset(IReadOnlyDictionary<string, ISet<string>> atlasAssetPathToSpritePaths)
	{
		return atlasAssetPathToSpritePaths.Keys.Select(GetSpriteAtlasBundleName).ToHashSet();
	}

	public static Dictionary<string, string> CreateAtlasPathToAtlasBundleDictionary(IReadOnlyDictionary<string, ISet<string>> atlasAssetPathToSpritePaths)
	{
		return atlasAssetPathToSpritePaths.Keys.ToDictionary((string atlasAssetPath) => atlasAssetPath, GetSpriteAtlasBundleName);
	}

	public static Dictionary<string, string> CreateSpriteToAtlasPathDictionary(IReadOnlyDictionary<string, ISet<string>> atlasAssetPathToSpritePaths)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (var (text2, set2) in atlasAssetPathToSpritePaths)
		{
			foreach (string item in set2)
			{
				if (dictionary.TryGetValue(item, out var value))
				{
					Debug.LogError(item + " is in multiple sprite atlases: " + text2 + " and " + value);
				}
				else
				{
					dictionary.Add(item, text2);
				}
			}
		}
		return dictionary;
	}
}
