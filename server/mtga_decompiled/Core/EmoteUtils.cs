using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Emotes;
using AssetLookupTree.Payloads.Prefab;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.Arena.Models.Network;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Providers;

public static class EmoteUtils
{
	public const int MAX_EQUIPPED_STICKERS = 10;

	public const int MAX_EQUIPPED_PHRASES = 15;

	public static HashSet<string> GetOwnedEmoteIds(CosmeticsProvider cosmeticsProvider, IEmoteDataProvider emoteDataProvider)
	{
		HashSet<string> hashSet = cosmeticsProvider.PlayerOwnedEmotes?.Select((CosmeticEmoteEntry item) => item.Id).ToHashSet() ?? new HashSet<string>();
		foreach (EmoteData defaultEmoteDatum in emoteDataProvider.GetDefaultEmoteData())
		{
			hashSet.Add(defaultEmoteDatum.Id);
		}
		return hashSet;
	}

	public static HashSet<string> GetEquippedEmoteIds(HashSet<string> equippedEmoteIds, HashSet<string> defaultEmoteIds)
	{
		foreach (string defaultEmoteId in defaultEmoteIds)
		{
			if (!equippedEmoteIds.Contains(defaultEmoteId))
			{
				equippedEmoteIds.Add(defaultEmoteId);
			}
		}
		return equippedEmoteIds;
	}

	public static ICollection<string> FilterByPage(this ICollection<string> emoteIds, IEmoteDataProvider emoteDataProvider, EmotePage emotePage)
	{
		for (int num = emoteIds.Count - 1; num >= 0; num--)
		{
			if (emoteDataProvider.TryGetEmoteData(emoteIds.ElementAt(num), out var emoteData) && emoteData.Entry.Page != emotePage)
			{
				emoteIds.Remove(emoteIds.ElementAt(num));
			}
		}
		foreach (string defaultEmoteId in GetDefaultEmoteIds(emoteDataProvider))
		{
			bool flag = emoteIds.Contains(defaultEmoteId);
			EmoteData emoteData2;
			bool flag2 = emoteDataProvider.TryGetEmoteData(defaultEmoteId, out emoteData2) && emoteData2.Entry.Page == emotePage;
			if (!flag && flag2)
			{
				emoteIds.Add(defaultEmoteId);
			}
			else if (flag)
			{
				emoteIds.Remove(defaultEmoteId);
			}
		}
		return emoteIds;
	}

	public static ICollection<string> FilterByDefault(this ICollection<string> emoteIds, IEmoteDataProvider emoteDataProvider, bool filterOutNonDefault = true)
	{
		HashSet<string> defaultEmoteIds = GetDefaultEmoteIds(emoteDataProvider);
		for (int num = emoteIds.Count - 1; num >= 0; num--)
		{
			bool flag = defaultEmoteIds.Contains(emoteIds.ElementAt(num));
			if (filterOutNonDefault && !flag)
			{
				emoteIds.Remove(emoteIds.ElementAt(num));
			}
			else if (!filterOutNonDefault && flag)
			{
				emoteIds.Remove(emoteIds.ElementAt(num));
			}
		}
		return emoteIds;
	}

	public static HashSet<string> GetDefaultEmoteIds(IEmoteDataProvider emoteDataProvider)
	{
		return (from data in emoteDataProvider.GetDefaultEmoteData()
			select data.Id).ToHashSet();
	}

	public static EmoteView InstantiateEmoteView(EmoteData emoteData, AssetLookupSystem assetLookupSystem)
	{
		EmoteView result = null;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<EmoteViewPrefab> loadedTree))
		{
			assetLookupSystem.Blackboard.EmotePrefabData = new EmotePrefabData
			{
				Id = emoteData.Id,
				Page = emoteData.Entry.Page
			};
			EmoteViewPrefab payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				result = AssetLoader.Instantiate<EmoteView>(payload.PrefabPath);
			}
		}
		return result;
	}

	public static EmoteView InstantiateDefaultEmoteView(string locKey, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.EmotePrefabData = new EmotePrefabData
		{
			Id = "Default",
			Page = EmotePage.Phrase
		};
		EmoteView emoteView = AssetLoader.Instantiate<EmoteView>(assetLookupSystem.TreeLoader.LoadTree<EmoteViewPrefab>(returnNewTree: false).GetPayload(assetLookupSystem.Blackboard).PrefabPath);
		emoteView.Init("Default", locKey);
		return emoteView;
	}

	public static bool IsEqualTo(this ICollection<string> emoteCollectionA, ICollection<string> emoteCollectionB)
	{
		if (emoteCollectionA.Count == emoteCollectionB.Count)
		{
			for (int i = 0; i < emoteCollectionA.Count; i++)
			{
				if (emoteCollectionA.ElementAt(i) != emoteCollectionB.ElementAt(i))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static bool IsEmoteEquipCapReached(EmotePage emotePage, IEmoteDataProvider emoteDataProvider, in ICollection<string> equippedEmotes)
	{
		ICollection<string> collection = equippedEmotes.ToList().FilterByPage(emoteDataProvider, emotePage).FilterByDefault(emoteDataProvider, filterOutNonDefault: false);
		return emotePage switch
		{
			EmotePage.Phrase => collection.Count >= 15, 
			EmotePage.Sticker => collection.Count >= 10, 
			_ => false, 
		};
	}

	public static bool AnyEmoteEquipCapExceeded(IEmoteDataProvider emoteDataProvider, in ICollection<string> equippedEmotes)
	{
		if (!IsEmoteEquipCapExceeded(EmotePage.Phrase, emoteDataProvider, in equippedEmotes))
		{
			return IsEmoteEquipCapExceeded(EmotePage.Sticker, emoteDataProvider, in equippedEmotes);
		}
		return true;
	}

	public static bool IsEmoteEquipCapExceeded(EmotePage emotePage, IEmoteDataProvider emoteDataProvider, in ICollection<string> equippedEmotes)
	{
		ICollection<string> collection = equippedEmotes.ToList().FilterByPage(emoteDataProvider, emotePage).FilterByDefault(emoteDataProvider, filterOutNonDefault: false);
		return emotePage switch
		{
			EmotePage.Phrase => collection.Count > 15, 
			EmotePage.Sticker => collection.Count > 10, 
			_ => false, 
		};
	}

	public static void InvokeActionsOnEmotePageMatch(string emoteId, IEmoteDataProvider emoteDataProvider, params KeyValuePair<EmotePage, Action>[] actionsOnMatchingEmotePage)
	{
		EmoteData emoteData = emoteDataProvider.GetEmoteData(emoteId);
		for (int i = 0; i < actionsOnMatchingEmotePage.Length; i++)
		{
			KeyValuePair<EmotePage, Action> keyValuePair = actionsOnMatchingEmotePage[i];
			if (keyValuePair.Key == emoteData.Entry.Page)
			{
				keyValuePair.Value?.Invoke();
			}
		}
	}

	public static List<EmoteViewGameObjectData> GetEmoteViewGameObjectDataForEmoteSelectionScreen(CosmeticsProvider cosmetics, IEmoteDataProvider emoteDataProvider, HashSet<string> playerSetEmotes, IReadOnlyCollection<string> instantiatedEmoteIds)
	{
		List<EmoteViewGameObjectData> list = new List<EmoteViewGameObjectData>();
		HashSet<string> ownedEmoteIds = GetOwnedEmoteIds(cosmetics, emoteDataProvider);
		HashSet<string> defaultEmoteIds = GetDefaultEmoteIds(emoteDataProvider);
		HashSet<string> equippedEmoteIds = GetEquippedEmoteIds(playerSetEmotes, defaultEmoteIds);
		foreach (string item in ownedEmoteIds)
		{
			EmoteData emoteData = emoteDataProvider.GetEmoteData(item);
			bool isEquipped = equippedEmoteIds.Contains(item) || defaultEmoteIds.Contains(item);
			bool isClickable = !defaultEmoteIds.Contains(item);
			bool isInstantiated = instantiatedEmoteIds.Contains(item);
			EmoteSelectionController.EmoteUISection emoteUISection = EmoteSelectionController.EmoteUISection.None;
			emoteUISection = (defaultEmoteIds.Contains(item) ? EmoteSelectionController.EmoteUISection.Classic : ((emoteData.Entry.Page != EmotePage.Sticker) ? EmoteSelectionController.EmoteUISection.Expansion : EmoteSelectionController.EmoteUISection.Sticker));
			list.Add(new EmoteViewGameObjectData(emoteData, emoteUISection, isClickable, isEquipped, isInstantiated));
		}
		return list;
	}

	public static int UpdateEquippedEmote(string emoteId, int currentEquippedCount, ref HashSet<string> equippedEmotes, Dictionary<string, EmoteView> instantiatedEmotes)
	{
		if (equippedEmotes.Contains(emoteId))
		{
			instantiatedEmotes[emoteId].SetEquipped(isEquipped: false);
			equippedEmotes.Remove(emoteId);
			currentEquippedCount--;
		}
		else
		{
			instantiatedEmotes[emoteId].SetEquipped(isEquipped: true);
			equippedEmotes.Add(emoteId);
			currentEquippedCount++;
		}
		return currentEquippedCount;
	}

	public static IReadOnlyCollection<ClientEmoteEntry> TranslateToClientEmoteEntries(IReadOnlyCollection<EmoteEntry> emoteEntries)
	{
		List<ClientEmoteEntry> list = new List<ClientEmoteEntry>();
		foreach (EmoteEntry emoteEntry in emoteEntries)
		{
			list.Add(new ClientEmoteEntry(emoteEntry.Id, ConvertPage(emoteEntry.Page), emoteEntry.Source.HasFlag(AcquisitionFlags.DefaultLoginGrant), emoteEntry.Category));
		}
		return list;
	}

	private static EmotePage ConvertPage(EmotePage page)
	{
		return page switch
		{
			EmotePage.Phrase => EmotePage.Phrase, 
			EmotePage.Sticker => EmotePage.Sticker, 
			_ => throw new ArgumentOutOfRangeException("page", page, null), 
		};
	}

	public static List<EmoteOptionsPage> CreateEmoteOptionsPages(EmotePage emotePage, List<EmoteData> emoteDatas, int emotesPerPage)
	{
		int i = 0;
		List<EmoteOptionsPage> list = new List<EmoteOptionsPage>();
		int num;
		for (; i < emoteDatas.Count; i += num)
		{
			num = Math.Min(emoteDatas.Count - i, emotesPerPage);
			List<EmoteData> range = emoteDatas.GetRange(i, num);
			list.Add(CreateEmoteOptionsPage(emotePage, range));
		}
		return list;
	}

	private static EmoteOptionsPage CreateEmoteOptionsPage(EmotePage emotePage, List<EmoteData> emoteDatas)
	{
		EmoteOptionsPage result = new EmoteOptionsPage(emotePage);
		foreach (EmoteData emoteData in emoteDatas)
		{
			result.EmoteData.Add(emoteData);
		}
		return result;
	}

	public static string GetPreviewLocKey(string emoteId, AssetLookupSystem assetLookupSystem)
	{
		if (!TryGetLocKeyPayload(emoteId, assetLookupSystem, out var payload))
		{
			return string.Empty;
		}
		if (!string.IsNullOrEmpty(payload.PreviewLocKey))
		{
			return payload.PreviewLocKey;
		}
		return string.Empty;
	}

	public static string GetFullLocKey(string emoteId, AssetLookupSystem assetLookupSystem)
	{
		if (!TryGetLocKeyPayload(emoteId, assetLookupSystem, out var payload))
		{
			return string.Empty;
		}
		if (!string.IsNullOrEmpty(payload.FullLocKey))
		{
			return payload.FullLocKey;
		}
		return string.Empty;
	}

	public static string GetStoreLocKey(string emoteId, AssetLookupSystem assetLookupSystem)
	{
		if (!TryGetLocKeyPayload(emoteId, assetLookupSystem, out var payload))
		{
			return string.Empty;
		}
		if (payload.UseFullLocInStore && !string.IsNullOrEmpty(payload.FullLocKey))
		{
			return payload.FullLocKey;
		}
		if (!string.IsNullOrEmpty(payload.PreviewLocKey))
		{
			return payload.PreviewLocKey;
		}
		return string.Empty;
	}

	private static bool TryGetLocKeyPayload(string emoteId, AssetLookupSystem assetLookupSystem, out EmoteLocKey payload)
	{
		payload = null;
		if (!assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<EmoteLocKey> loadedTree))
		{
			return false;
		}
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.EmoteId = emoteId;
		EmoteLocKey payload2 = loadedTree.GetPayload(assetLookupSystem.Blackboard);
		if (payload2 == null)
		{
			return false;
		}
		payload = payload2;
		return true;
	}

	public static List<string> GetAssociatedEmoteIds(string emoteId, AssetLookupSystem assetLookupSystem)
	{
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AssociatedEmoteIdPayload> loadedTree))
		{
			IBlackboard blackboard = assetLookupSystem.Blackboard;
			blackboard.EmoteId = emoteId;
			AssociatedEmoteIdPayload payload = loadedTree.GetPayload(blackboard);
			if (payload != null)
			{
				return payload.AssociatedIds;
			}
		}
		return new List<string>();
	}

	public static SfxData GetEmoteSfxData(string emoteId, AssetLookupSystem assetLookupSystem)
	{
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<EmoteSFX> loadedTree))
		{
			IBlackboard blackboard = assetLookupSystem.Blackboard;
			blackboard.EmoteId = emoteId;
			EmoteSFX payload = loadedTree.GetPayload(blackboard);
			if (payload != null)
			{
				return payload.SfxData;
			}
		}
		return null;
	}
}
