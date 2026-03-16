using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace AssetLookupTree;

[Serializable]
public class AltAssetReference
{
	public static readonly ConcurrentDictionary<string, string> ReferencesToDeDupe = new ConcurrentDictionary<string, string>();

	private string _guid = string.Empty;

	private string _relativePath = string.Empty;

	public string Guid
	{
		get
		{
			return _guid ?? string.Empty;
		}
		set
		{
			_guid = value ?? string.Empty;
		}
	}

	public string RelativePath
	{
		get
		{
			return _relativePath ?? string.Empty;
		}
		set
		{
			_relativePath = ReferencesToDeDupe.GetOrAdd(value ?? string.Empty, value ?? string.Empty);
		}
	}
}
[Serializable]
public class AltAssetReference<T> : AltAssetReference where T : UnityEngine.Object
{
}
