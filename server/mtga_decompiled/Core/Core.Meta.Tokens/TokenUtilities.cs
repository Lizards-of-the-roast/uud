using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using UnityEngine;

namespace Core.Meta.Tokens;

public class TokenUtilities
{
	public static AltAssetReference<TPrefab> TokenRefForId<TPayload, TPrefab>(AssetLookupSystem assetLookupSystem, string viewKey) where TPayload : PrefabPayload<TPrefab> where TPrefab : Object
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.LookupString = viewKey;
		AltAssetReference<TPrefab> prefab = assetLookupSystem.GetPrefab<TPayload, TPrefab>();
		if (prefab == null)
		{
			SimpleLog.LogError("Could not find prefab for lookup: " + viewKey);
			return null;
		}
		return prefab;
	}

	public static string GetTokenLocalizationKey(string tokenId, string locKey, int quantity)
	{
		if (!string.IsNullOrEmpty(locKey))
		{
			if (quantity <= 1)
			{
				return locKey + "Singular";
			}
			return locKey + "Plural";
		}
		return $"MainNav/General/{tokenId}";
	}
}
