using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using ProfileUI;
using UnityEngine;

namespace Core.Meta.MainNavigation.Cosmetics;

public class DisplayItemTitle : DisplayItemCosmeticBase, IDisplayItemCosmetic<string>
{
	private string _selectedTitle;

	private Action<string> _onCosmeticSelected;

	private Action _onDefaultSelected;

	private TitleSelectPanel _selector;

	private AssetLookupSystem _assetLookupSystem;

	public void Init(Transform selectorTransform, AssetLookupSystem assetLookupSystem)
	{
		_assetLookupSystem = assetLookupSystem;
		string prefabPathFromALT = GetPrefabPathFromALT(assetLookupSystem);
		_selector = AssetLoader.Instantiate<TitleSelectPanel>(prefabPathFromALT, selectorTransform);
	}

	public void SetData(string preferredTitle)
	{
		string preferredTitle2 = (string.IsNullOrEmpty(preferredTitle) ? "NoTitle" : preferredTitle);
		_selector.SetData(preferredTitle2, _assetLookupSystem);
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

	public void SetOnCosmeticSelected(Action<string> onCosmeticSelected)
	{
	}

	public void SetOnDefaultSelected(Action onDefaultSelected)
	{
	}

	private string GetPrefabPathFromALT(AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		string text = (assetLookupSystem.TreeLoader.LoadTree<TitleSelectPanelPrefab>()?.GetPayload(assetLookupSystem.Blackboard))?.PrefabPath;
		if (text == null)
		{
			SimpleLog.LogError("Could not find prefab for lookup: TitleSelectPanelPrefab");
			return "";
		}
		return text;
	}
}
