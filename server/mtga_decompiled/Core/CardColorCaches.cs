using System;
using System.Collections.Generic;
using AssetLookupTree;
using UnityEngine;

public class CardColorCaches : IDisposable
{
	private readonly Dictionary<string, CardTextColorTable> _cardTextColorTables = new Dictionary<string, CardTextColorTable>();

	private readonly Dictionary<string, CardColorTable> _cardColorTables = new Dictionary<string, CardColorTable>();

	private readonly Dictionary<string, FieldTextColorSettings> _fieldTextColorSettings = new Dictionary<string, FieldTextColorSettings>();

	public static CardColorCaches Create()
	{
		return new CardColorCaches();
	}

	public CardTextColorTable GetCardTextColorTable(string path)
	{
		return GetAndCache(path, _cardTextColorTables);
	}

	public CardTextColorTable GetCardTextColorTable(AltAssetReference<CardTextColorTable> reference)
	{
		return GetAndCache(reference?.RelativePath, _cardTextColorTables);
	}

	public CardColorTable GetCardColorTable(string path)
	{
		return GetAndCache(path, _cardColorTables);
	}

	public CardColorTable GetCardColorTable(AltAssetReference<CardColorTable> reference)
	{
		return GetAndCache(reference?.RelativePath, _cardColorTables);
	}

	public FieldTextColorSettings GetFieldTextColorSettings(string path)
	{
		return GetAndCache(path, _fieldTextColorSettings);
	}

	public FieldTextColorSettings GetFieldTextColorSettings(AltAssetReference<FieldTextColorSettings> reference)
	{
		return GetAndCache(reference?.RelativePath, _fieldTextColorSettings);
	}

	private static T GetAndCache<T>(string path, Dictionary<string, T> cache) where T : UnityEngine.Object
	{
		if (cache.ContainsKey(path))
		{
			return cache[path];
		}
		return cache[path] = AssetLoader.GetObjectData<T>(path);
	}

	public void Dispose()
	{
		_cardColorTables.Clear();
		_cardColorTables.Clear();
		_fieldTextColorSettings.Clear();
	}
}
