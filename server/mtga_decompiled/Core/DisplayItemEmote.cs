using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using TMPro;
using UnityEngine;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.Mtga;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class DisplayItemEmote : DisplayItemCosmeticBase, IDisplayItemCosmetic<List<string>>
{
	[SerializeField]
	private TextMeshProUGUI stickerCountText;

	[SerializeField]
	private TextMeshProUGUI phraseCountText;

	private EmoteSelectionController _selector;

	private Action<List<string>> _onCosmeticSelected;

	private IEmoteDataProvider _emoteDataProvider;

	private IClientLocProvider _locMan;

	private const string STICKERS_LOC_KEY = "MainNav/Profile/Emotes/EquippedStickersCount_Header";

	private const string PHRASES_LOC_KEY = "MainNav/Profile/Emotes/EquippedPhrasesCount_Header";

	public void Init(Transform selectorTransform, CosmeticsProvider cosmeticsProvider, AssetLookupSystem assetLookupSystem, IEmoteDataProvider emoteDataProvider, IClientLocProvider locMan, IBILogger logger, bool isSideboarding)
	{
		EmoteSelectionScreenView emoteSelectionPopupView = AssetLoader.Instantiate<EmoteSelectionScreenView>(GetPrefabPathFromALT(assetLookupSystem), selectorTransform);
		_emoteDataProvider = emoteDataProvider;
		_locMan = locMan;
		_selector = new EmoteSelectionController(cosmeticsProvider, emoteDataProvider, assetLookupSystem, emoteSelectionPopupView, logger, locMan);
		_selector.SetCallbacks(OnEmoteSelected, OnClose);
		base.IsReadOnly = isSideboarding;
	}

	public override void OpenSelector()
	{
		if (!base.IsReadOnly)
		{
			base.OpenSelector();
			_selector.Open();
		}
	}

	public override void CloseSelector()
	{
		_selector.Close();
	}

	public void SetOnCosmeticSelected(Action<List<string>> onCosmeticSelected)
	{
		_onCosmeticSelected = onCosmeticSelected;
	}

	public void SetData(List<string> emotes, bool isReadOnly)
	{
		base.IsReadOnly = isReadOnly;
		_selector.SetData(emotes);
	}

	private string GetPrefabPathFromALT(AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		string text = (assetLookupSystem.TreeLoader.LoadTree<EmoteSelectionScreenViewPrefab>()?.GetPayload(assetLookupSystem.Blackboard))?.PrefabPath;
		if (text == null)
		{
			SimpleLog.LogError("Could not find prefab for lookup: EmoteSelectionScreenView");
			return "";
		}
		return text;
	}

	private void OnEmoteSelected(List<string> emotes)
	{
		_onCosmeticSelected(emotes);
		if (!(phraseCountText == null) && !(stickerCountText == null))
		{
			ICollection<string> emotesByType = GetEmotesByType(emotes, EmotePage.Phrase);
			ICollection<string> emotesByType2 = GetEmotesByType(emotes, EmotePage.Sticker);
			phraseCountText.text = GetLocalizationCountByType("MainNav/Profile/Emotes/EquippedPhrasesCount_Header", emotesByType.Count, 15);
			stickerCountText.text = GetLocalizationCountByType("MainNav/Profile/Emotes/EquippedStickersCount_Header", emotesByType2.Count, 10);
		}
	}

	private ICollection<string> GetEmotesByType(List<string> emotes, EmotePage emotePage)
	{
		return emotes.ToList().FilterByPage(_emoteDataProvider, emotePage);
	}

	private string GetLocalizationCountByType(string key, int currentCount, int maxCount)
	{
		return _locMan.GetLocalizedText(key, ("currentEquippedCount", currentCount.ToString()), ("maxEquippedCount", maxCount.ToString()));
	}
}
