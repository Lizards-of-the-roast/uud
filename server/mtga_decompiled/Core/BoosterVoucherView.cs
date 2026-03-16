using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using Core.Meta.MainNavigation.BoosterChamber;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Wrapper;

public class BoosterVoucherView : MonoBehaviour, IBoosterChamberSetLogoInfo
{
	[SerializeField]
	private Localize _quantityText;

	[SerializeField]
	private GameObject _quantityContainer;

	[SerializeField]
	private Localize _availableText;

	[SerializeField]
	private StoreDisplayCardViewBundle _storeDisplay;

	[SerializeField]
	private Animator _voucherAnimator;

	[NonSerialized]
	public string VoucherId;

	[NonSerialized]
	private string _boosterSetLogoTexturePath;

	[NonSerialized]
	private string _headerSetLogoTexturePath;

	[NonSerialized]
	private ISetMetadataProvider _setMetadataProvider;

	private CollationMapping _collationMapping;

	private string _setCode = string.Empty;

	public string GetHeaderSetLogoTexturePath()
	{
		return _headerSetLogoTexturePath;
	}

	public bool IsUniversesBeyond()
	{
		return _setMetadataProvider.IsUniversesBeyond(_collationMapping);
	}

	public void Awake()
	{
		_setMetadataProvider = Pantry.Get<ISetMetadataProvider>();
	}

	public void SetQuantity(int quantity)
	{
		if (_quantityText != null)
		{
			_quantityText.SetText("MainNav/General/Simple_Number", new Dictionary<string, string> { 
			{
				"number",
				quantity.ToString()
			} });
			bool active = quantity > 1;
			((_quantityContainer != null) ? _quantityContainer : _quantityText.gameObject).SetActive(active);
		}
	}

	public void SetAvailability(string locKey, DateTime date)
	{
		if (string.IsNullOrEmpty(locKey))
		{
			_availableText.SetText(StoreManager.GetPreorderAvailableString(date, isPurchased: true));
		}
		else
		{
			_availableText.SetText(StoreManager.GetPreorderAvailableString(locKey, date, isPurchased: true));
		}
	}

	public void SetCollation(CollationMapping setCode, List<CollationMapping> collations)
	{
		if (!(_storeDisplay == null))
		{
			_storeDisplay.SetCollationIds(collations);
			_collationMapping = setCode;
			_setCode = Enum.GetName(typeof(CollationMapping), setCode);
			Refresh();
		}
	}

	public void SetCardViews(List<StoreCardData> cardDefinitions)
	{
		if (!(_storeDisplay == null))
		{
			_storeDisplay.SetCardViews(cardDefinitions);
		}
	}

	public void PresentOrOpen()
	{
	}

	public void ConditionalHover()
	{
		AudioManager.SetRTPCValue($"boosterpack_{_setCode}", 100f);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_pack_rollover, base.gameObject);
	}

	public void ConditionalHoverOff()
	{
		AudioManager.SetRTPCValue($"boosterpack_{_setCode}", 0f);
	}

	public void ClickDown()
	{
		BoosterChamberController componentInParent = GetComponentInParent<BoosterChamberController>();
		if (componentInParent != null)
		{
			componentInParent.ClickedVoucherView(this);
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_pack_tap, base.gameObject);
		_voucherAnimator.SetTrigger("Click");
	}

	public void ReleaseClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_pack_release, base.gameObject);
	}

	public void Refresh()
	{
		_storeDisplay.RefreshBoosterMaterials();
		AssetLookupSystem assetLookupSystem = WrapperController.Instance.AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.BoosterCollationMapping = _collationMapping;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Logo> loadedTree))
		{
			Logo payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			_boosterSetLogoTexturePath = payload?.TextureRef.RelativePath;
			_headerSetLogoTexturePath = payload?.GetHeaderFilePath();
		}
	}
}
