using System;
using AssetLookupTree;
using Core.Meta.MainNavigation.Store.Data;
using Core.Meta.MainNavigation.Store.Utils;
using UnityEngine;
using UnityEngine.UI;

public class StoreSetFilterDropdownItem : MonoBehaviour
{
	[SerializeField]
	private Image _symbol;

	[SerializeField]
	private RawImage _logo;

	[SerializeField]
	private Button _button;

	private AssetLoader.AssetTracker<Texture> _textureTracker = new AssetLoader.AssetTracker<Texture>("StoreSetFilterLogoTextureTracker");

	private readonly AssetTracker _assetTracker = new AssetTracker();

	public Action<StoreSetFilterModel> OnClicked;

	private string _setName;

	private DeckFormat _deckFormat;

	private StoreSetFilterModel _setFilterModel;

	public void Initialize(AssetLookupSystem assetLookupSystem, StoreSetFilterModel setFilterModel)
	{
		_assetTracker.Cleanup();
		_setFilterModel = setFilterModel;
		_symbol.sprite = StoreSetUtils.SpriteForSetName(_setFilterModel.SetSymbolAsCollationMapping, assetLookupSystem, _assetTracker, _setFilterModel.Availability);
		StoreSetUtils.LogoForSetName(assetLookupSystem, ref _textureTracker, ref _logo, _setFilterModel.SetSymbolAsCollationMapping);
		_button.onClick.RemoveAllListeners();
		_button.onClick.AddListener(OnButtonClicked);
	}

	private void OnButtonClicked()
	{
		OnClicked?.Invoke(_setFilterModel);
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupRawImage(_logo, _textureTracker);
		_assetTracker.Cleanup();
	}
}
