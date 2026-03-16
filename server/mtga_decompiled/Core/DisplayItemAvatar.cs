using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using ProfileUI;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Providers;

public class DisplayItemAvatar : DisplayItemCosmeticBase, IDisplayItemCosmetic<AvatarSelection>
{
	[SerializeField]
	private Image avatarBodyImage;

	private string _defaultAvatar;

	private AssetLookupSystem _assetLookupSystem;

	private AvatarCatalog _avatarCatalog;

	private AvatarSelectPanel _selector;

	private Action<AvatarSelection> _onCosmeticSelected;

	private Action<string> _onDefaultSelected;

	private bool _showDefaultInterface;

	private readonly AssetLoader.AssetTracker<Sprite> _avatarBodyImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("DisplayItemAvatarDisplayImageSprite");

	public void Init(Transform selectorTransform, CosmeticsProvider cosmetics, AssetLookupSystem assetLookupSystem, AvatarCatalog avatarCatalog, bool isReadOnly)
	{
		string prefabPathFromALT = GetPrefabPathFromALT(assetLookupSystem);
		_selector = AssetLoader.Instantiate<AvatarSelectPanel>(prefabPathFromALT, selectorTransform);
		_assetLookupSystem = assetLookupSystem;
		_selector.Initialize(assetLookupSystem, cosmetics, avatarCatalog, nullIsValid: true);
		_selector.SetCallbacks(OnCosmeticSelected, OnClose);
		_selector.OnStoreClicked(OnStoreSelected);
		_selector.OnDefaultSelected = DefaultSelected;
		base.IsReadOnly = isReadOnly;
	}

	public void SetData(string currentSelectedAvatar, string defaultAvatar = "", bool showDefaultInterface = false, bool isReadOnly = false)
	{
		base.IsReadOnly = isReadOnly;
		_showDefaultInterface = showDefaultInterface;
		string avatarId = (string.IsNullOrEmpty(currentSelectedAvatar) ? defaultAvatar : currentSelectedAvatar);
		string avatarFullImagePath = ProfileUtilities.GetAvatarFullImagePath(_assetLookupSystem, avatarId);
		SetAvatarImage(avatarFullImagePath);
		_selector.SetData(currentSelectedAvatar, defaultAvatar);
	}

	public override void OpenSelector()
	{
		if (!base.IsReadOnly)
		{
			base.OpenSelector();
			_selector.Open(_showDefaultInterface);
		}
	}

	public override void CloseSelector()
	{
		_selector.Close();
	}

	public void OnCosmeticSelected(AvatarSelection selection)
	{
		SetAvatarImage(selection.FullSpritePath);
		_onCosmeticSelected?.Invoke(selection);
	}

	private string GetPrefabPathFromALT(AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		string text = (assetLookupSystem.TreeLoader.LoadTree<AvatarSelectPanelPrefab>()?.GetPayload(assetLookupSystem.Blackboard))?.PrefabPath;
		if (text == null)
		{
			SimpleLog.LogError("Could not find prefab for lookup: AvatarSelectPanelPrefab");
			return "";
		}
		return text;
	}

	private void DefaultSelected(string avatarId)
	{
		_onDefaultSelected?.Invoke(avatarId);
	}

	private void SetAvatarImage(string fullSpritePath)
	{
		if (!(avatarBodyImage == null))
		{
			AssetLoaderUtils.TrySetSprite(avatarBodyImage, _avatarBodyImageSpriteTracker, fullSpritePath);
		}
	}

	public void SetOnCosmeticSelected(Action<AvatarSelection> onCosmeticSelected)
	{
		_onCosmeticSelected = onCosmeticSelected;
	}

	public void SetOnDefaultSelected(Action<string> onDefaultSelected)
	{
		_onDefaultSelected = onDefaultSelected;
	}

	protected override void OnClose()
	{
		_selector.gameObject.SetActive(value: false);
		base.OnClose();
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(avatarBodyImage, _avatarBodyImageSpriteTracker);
	}
}
