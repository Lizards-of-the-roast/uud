using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Meta.MainNavigation.Store;

public class StoreDisplayPhraseBundle : StoreItemDisplay
{
	[SerializeField]
	[Header("List of all EmoteViews. Must match the listing SKUs!")]
	private List<EmoteView> _emoteViews = new List<EmoteView>();

	private void Awake()
	{
		foreach (EmoteView emoteView in _emoteViews)
		{
			if ((bool)emoteView)
			{
				emoteView.OnClick += OnClick;
			}
		}
	}

	protected override void OnDestroy()
	{
		foreach (EmoteView emoteView in _emoteViews)
		{
			if ((bool)emoteView)
			{
				emoteView.OnClick -= OnClick;
			}
		}
		base.OnDestroy();
	}

	public void SetEmotes(List<Sku> skus, AssetLookupSystem assetLookupSystem, Wizards.Arena.Client.Logging.Logger logger)
	{
		if (skus.Count != _emoteViews.Count)
		{
			logger.Error("Failed to init Emote " + base.gameObject.name + "! SKU count is different from emote count.");
			return;
		}
		for (int i = 0; i < skus.Count; i++)
		{
			string id = skus[i].Id;
			if (!string.IsNullOrEmpty(id))
			{
				SfxData emoteSfxData = EmoteUtils.GetEmoteSfxData(id, assetLookupSystem);
				_emoteViews[i].Init(id, EmoteUtils.GetStoreLocKey(id, assetLookupSystem), emoteSfxData);
				_emoteViews[i].SetClickable(emoteSfxData != null);
				continue;
			}
			logger.Error("Failed to find init emoteView for " + base.gameObject.name + "! " + skus[i].Id + " is not a valid emoteId!");
		}
	}

	private void OnClick(string emoteId)
	{
		EmoteView emoteView = _emoteViews.FirstOrDefault((EmoteView x) => x.Id == emoteId);
		if (emoteView != null)
		{
			emoteView.PlaySfx();
		}
	}
}
