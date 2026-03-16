using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class CardFilterView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private CardFilterType _filtertype;

	[FormerlySerializedAs("OnImage")]
	[SerializeField]
	private Image _onImage;

	[FormerlySerializedAs("OffImage")]
	[SerializeField]
	private Image _offImage;

	[FormerlySerializedAs("FilterLocalizer")]
	[SerializeField]
	private Localize _filterLocalizer;

	private AssetTracker _assetTracker = new AssetTracker();

	private CanvasGroup _canvasGroup;

	private Toggle _filterToggle;

	private ParticleSystem _highlight;

	public CardFilterType FilterType => _filtertype;

	public Toggle FilterToggle
	{
		get
		{
			if (_filterToggle == null)
			{
				_filterToggle = GetComponentInChildren<Toggle>();
			}
			return _filterToggle;
		}
	}

	public CanvasGroup CanvasGroup
	{
		get
		{
			if (_canvasGroup == null)
			{
				_canvasGroup = GetComponent<CanvasGroup>();
			}
			return _canvasGroup;
		}
	}

	private ParticleSystem Highlight
	{
		get
		{
			if (_highlight == null)
			{
				_highlight = GetComponentInChildren<ParticleSystem>(includeInactive: true);
			}
			return _highlight;
		}
	}

	private void Awake()
	{
		FilterToggle.onValueChanged.AddListener(OnToggleValueChanged);
	}

	private void OnDestroy()
	{
		FilterToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
		_assetTracker.Cleanup();
	}

	private void OnEnable()
	{
		if (Highlight != null && _filterToggle.interactable)
		{
			Highlight.gameObject.UpdateActive(_filterToggle.isOn);
		}
	}

	public void Initialize(CardFilterType filterType, string setCode)
	{
		AssetLookupManager assetLookupManager = Pantry.Get<AssetLookupManager>();
		_filtertype = filterType;
		_onImage.sprite = SpriteForSetName(setCode, assetLookupManager.AssetLookupSystem, _assetTracker, CardRarity.MythicRare);
		_offImage.sprite = SpriteForSetName(setCode, assetLookupManager.AssetLookupSystem, _assetTracker, CardRarity.Common);
		_filterLocalizer.SetText("General/Sets/" + setCode);
	}

	private void OnToggleValueChanged(bool toggleValue)
	{
		if (_filtertype.ToString().Contains("Color"))
		{
			AudioManager.SetSwitch("color", AudioManager.Instance.GetColorKey(_filtertype), base.gameObject);
			AudioManager.PlayAudio(WwiseEvents.sfx_mana_type_filter_select, base.gameObject);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		}
		if (Highlight != null)
		{
			Highlight.gameObject.UpdateActive(toggleValue);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_filtertype.ToString().Contains("Color"))
		{
			AudioManager.SetSwitch("color", AudioManager.Instance.GetColorKey(_filtertype), base.gameObject);
			AudioManager.PlayAudio(WwiseEvents.sfx_mana_type_filter_rollover, base.gameObject);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
		}
		if (Highlight != null && _filterToggle.interactable)
		{
			Highlight.gameObject.UpdateActive(active: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (Highlight != null && _filterToggle.interactable && !_filterToggle.isOn)
		{
			Highlight.gameObject.UpdateActive(active: false);
		}
	}

	public static Sprite SpriteForSetName(string collationId, AssetLookupSystem assetLookupSystem, AssetTracker assetTracker, CardRarity cardRarity)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(CardDataExtensions.CreateBlankExpansionCard(collationId, collationId));
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ExpansionSymbol> loadedTree))
		{
			AltAssetReference<Sprite> altAssetReference = (loadedTree?.GetPayload(assetLookupSystem.Blackboard))?.GetIconRef(cardRarity);
			if (altAssetReference != null && altAssetReference.RelativePath != null)
			{
				return AssetLoader.AcquireAndTrackAsset(assetTracker, "ExpansionSymbol._symbol", altAssetReference);
			}
		}
		return null;
	}
}
