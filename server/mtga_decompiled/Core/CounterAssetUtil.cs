using AssetLookupTree;
using AssetLookupTree.Payloads.Counter;
using GreClient.CardData;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public static class CounterAssetUtil
{
	public static CounterAssetData GetCounterAsset(AssetLookupSystem assetLookupSystem, CounterType counterType, ICardDataAdapter cardData, CardHolderType cardHolderType = CardHolderType.Battlefield, DuelSceneBrowserType browserType = DuelSceneBrowserType.Invalid)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		assetLookupSystem.Blackboard.CounterType = counterType;
		assetLookupSystem.Blackboard.CardBrowserType = browserType;
		CounterVisuals payload = assetLookupSystem.TreeLoader.LoadTree<CounterVisuals>().GetPayload(assetLookupSystem.Blackboard);
		if (payload == null)
		{
			return null;
		}
		return new CounterAssetData
		{
			PrefabPath = payload.PrefabPath,
			UiSpritePath = payload.SpritePath
		};
	}

	public static string GetCounterPrefabPath(AssetLookupSystem assetLookupSystem, CounterType counterType, ICardDataAdapter cardData, CardHolderType cardHolderType = CardHolderType.Battlefield)
	{
		return GetCounterAsset(assetLookupSystem, counterType, cardData, cardHolderType)?.PrefabPath;
	}

	public static CounterEffects GetCounterSpawnedEffects(AssetLookupSystem assetLookupSystem, CounterType counterType, ICardDataAdapter cardData, CardHolderType cardHolderType = CardHolderType.Battlefield)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		assetLookupSystem.Blackboard.CounterType = counterType;
		SpawnVfx payload = assetLookupSystem.TreeLoader.LoadTree<SpawnVfx>().GetPayload(assetLookupSystem.Blackboard);
		SpawnSfx payload2 = assetLookupSystem.TreeLoader.LoadTree<SpawnSfx>().GetPayload(assetLookupSystem.Blackboard);
		return new CounterEffects(payload?.PrefabData.AllPrefabs.SelectRandom()?.RelativePath, payload2?.SfxData.AudioEvents, payload?.CardVFX);
	}

	public static CounterEffects GetCounterIncrementedEffects(AssetLookupSystem assetLookupSystem, CounterType counterType, ICardDataAdapter cardData, CardHolderType cardHolderType = CardHolderType.Battlefield)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		assetLookupSystem.Blackboard.CounterType = counterType;
		VfxPrefabData vfxPrefabData = null;
		VfxData cardVfx = null;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<IncrementVfx> loadedTree))
		{
			IncrementVfx payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				vfxPrefabData = payload.PrefabData;
				cardVfx = payload.CardVFX;
				goto IL_00a1;
			}
		}
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<SpawnVfx> loadedTree2))
		{
			SpawnVfx payload2 = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
			if (payload2 != null && payload2.IsFallback)
			{
				vfxPrefabData = payload2.PrefabData;
				cardVfx = payload2.CardVFX;
			}
		}
		goto IL_00a1;
		IL_00a1:
		SfxData sfxData = null;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<IncrementSfx> loadedTree3))
		{
			IncrementSfx payload3 = loadedTree3.GetPayload(assetLookupSystem.Blackboard);
			if (payload3 != null)
			{
				sfxData = payload3.SfxData;
				goto IL_0102;
			}
		}
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<SpawnSfx> loadedTree4))
		{
			SpawnSfx payload4 = loadedTree4.GetPayload(assetLookupSystem.Blackboard);
			if (payload4 != null && payload4.IsFallback)
			{
				sfxData = payload4.SfxData;
			}
		}
		goto IL_0102;
		IL_0102:
		return new CounterEffects(vfxPrefabData?.AllPrefabs.SelectRandom()?.RelativePath ?? string.Empty, sfxData?.AudioEvents, cardVfx);
	}

	public static CounterEffects GetCounterRemovedAssets(AssetLookupSystem assetLookupSystem, CounterType counterType, ICardDataAdapter cardData, CardHolderType cardHolderType = CardHolderType.Battlefield)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		assetLookupSystem.Blackboard.CounterType = counterType;
		RemoveVfx payload = assetLookupSystem.TreeLoader.LoadTree<RemoveVfx>().GetPayload(assetLookupSystem.Blackboard);
		RemoveSfx payload2 = assetLookupSystem.TreeLoader.LoadTree<RemoveSfx>().GetPayload(assetLookupSystem.Blackboard);
		return new CounterEffects(payload?.PrefabData.AllPrefabs.SelectRandom()?.RelativePath, payload2?.SfxData.AudioEvents, payload?.CardVFX);
	}

	public static CounterEffects GetCounterDecrementedAssets(AssetLookupSystem assetLookupSystem, CounterType counterType, ICardDataAdapter cardData, CardHolderType cardHolderType = CardHolderType.Battlefield)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.CounterType = counterType;
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		VfxPrefabData vfxPrefabData = null;
		VfxData cardVfx = null;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<DecrementVfx> loadedTree))
		{
			DecrementVfx payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				vfxPrefabData = payload.PrefabData;
				cardVfx = payload.CardVFX;
				goto IL_00a1;
			}
		}
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<RemoveVfx> loadedTree2))
		{
			RemoveVfx payload2 = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
			if (payload2 != null && payload2.IsFallback)
			{
				vfxPrefabData = payload2.PrefabData;
				cardVfx = payload2.CardVFX;
			}
		}
		goto IL_00a1;
		IL_00a1:
		SfxData sfxData = null;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<DecrementSfx> loadedTree3))
		{
			DecrementSfx payload3 = loadedTree3.GetPayload(assetLookupSystem.Blackboard);
			if (payload3 != null)
			{
				sfxData = payload3.SfxData;
				goto IL_0102;
			}
		}
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<RemoveSfx> loadedTree4))
		{
			RemoveSfx payload4 = loadedTree4.GetPayload(assetLookupSystem.Blackboard);
			if (payload4 != null && payload4.IsFallback)
			{
				sfxData = payload4.SfxData;
			}
		}
		goto IL_0102;
		IL_0102:
		return new CounterEffects(vfxPrefabData?.AllPrefabs.SelectRandom()?.RelativePath ?? string.Empty, sfxData?.AudioEvents, cardVfx);
	}
}
