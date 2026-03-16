using AssetLookupTree;
using AssetLookupTree.Payloads.Browser;
using Pooling;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectCounters;

public class ButtonStateBuilder : IButtonStateBuilder
{
	private readonly IUnityObjectPool _pool;

	private readonly IGreLocProvider _locProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	public ButtonStateBuilder(IUnityObjectPool pool, IGreLocProvider locProvider, AssetLookupSystem assetLookupSystem)
	{
		_pool = pool ?? NullUnityObjectPool.Default;
		_locProvider = locProvider ?? NullGreLocManager.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public ButtonStateData CreateButtonStateData(uint counterType, uint count, ButtonStyle.StyleType style, bool enabled, DuelSceneBrowserType browserType = DuelSceneBrowserType.Invalid)
	{
		ButtonStateData buttonStateData = new ButtonStateData();
		buttonStateData.BrowserElementKey = "ButtonDefault";
		string localizedTextForEnumValue = _locProvider.GetLocalizedTextForEnumValue("CounterType", (int)counterType);
		buttonStateData.LocalizedString = new UnlocalizedMTGAString(localizedTextForEnumValue);
		buttonStateData.StyleType = style;
		buttonStateData.Enabled = enabled;
		CounterAssetData counterAsset = CounterAssetUtil.GetCounterAsset(_assetLookupSystem, (CounterType)counterType, null, CardHolderType.Battlefield, browserType);
		if (counterAsset != null)
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.CounterType = (CounterType)counterType;
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ViewCounterUIPrefab> loadedTree))
			{
				ViewCounterUIPrefab payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					ViewCounter_UI component = _pool.PopObject(payload.ViewCounterRef.RelativePath).GetComponent<ViewCounter_UI>();
					component.gameObject.name = $"{counterType}_CounterView";
					component.SetBackground(counterAsset.UiSpritePath);
					component.SetCount(count);
					buttonStateData.ChildView = component.transform as RectTransform;
				}
			}
		}
		return buttonStateData;
	}
}
