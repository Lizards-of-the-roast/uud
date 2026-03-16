using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Cosmetic;
using GreClient.CardData;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.DeckValidation.Client;
using Wizards.Arena.Enums.Card;
using Wizards.Mtga.Inventory;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wizards.Mtga.Decks;

public class DeckView : MonoBehaviour
{
	[Header("Deck Parts")]
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private List<MeshRenderer> _meshRenderers;

	[SerializeField]
	private DeckViewImages _deckViewImages;

	[SerializeField]
	private CustomTouchButton _customButton;

	[SerializeField]
	private TMP_Text _deckNameText;

	[SerializeField]
	private TMP_InputField _deckNameInput;

	[SerializeField]
	private Transform _iconPencil;

	[SerializeField]
	private TMP_Text _warningText;

	[SerializeField]
	private TooltipTrigger _tooltipTrigger;

	[Header("Scaffolds")]
	[SerializeField]
	private Transform _symbolParent;

	[SerializeField]
	private Transform _sleeveParent0;

	[SerializeField]
	private Transform _sleeveParent1;

	[SerializeField]
	private Transform _sleeveParent2;

	[SerializeField]
	private Transform _sleeveParent3;

	[SerializeField]
	private AvatarSelection _avatarSelection;

	[SerializeField]
	private Image _petIcon;

	[Header("Prefabs")]
	[SerializeField]
	private ManaSymbolView _symbolPrefab;

	[SerializeField]
	private GameObject _fxPrefab;

	private ICardBuilder<Meta_CDC> _cardBuilder;

	private IUnityObjectPool _objectPool;

	private Action<DeckViewInfo> _onDeckClicked;

	private Action<DeckViewInfo> _onDoubleClicked;

	private Action<DeckViewInfo> _onMouseOver;

	private Action<DeckViewInfo, string> _onDeckNameEndEdit;

	private DeckViewInfo _deckViewInfo;

	private List<Meta_CDC> _metaCdcs;

	private Meta_CDC _sleeveCdc;

	private List<ManaSymbolView> spawnedManaSymbolViews = new List<ManaSymbolView>();

	private List<MeshRendererReferenceLoader> _meshRendererReferenceLoaders;

	private GameObject _fxObject;

	private const string Anim_Open_Trigger = "Open";

	private const string Anim_Hover_Trigger = "Hover";

	private const string Anim_DeckBoxOpen_State = "RewardInteract_DeckBoxLid_Open";

	private static readonly int Anim_Invalid_Bool = Animator.StringToHash("Invalid");

	private static readonly int Anim_Craftable_Bool = Animator.StringToHash("Craftable");

	private static readonly int Anim_Selected_Bool = Animator.StringToHash("Selected");

	private static readonly int Anim_UnCraftable_Bool = Animator.StringToHash("Uncraftable");

	private static readonly int Anim_Historic_Bool = Animator.StringToHash("Historic");

	private static readonly int Anim_Favorited_Bool = Animator.StringToHash("Favorited");

	private static readonly int Anim_InvalidCards_Int = Animator.StringToHash("NumberOfInvalidCards");

	private static readonly int Anim_Invalid_Companion_Bool = Animator.StringToHash("InvalidCompanion");

	private static readonly int Anim_Unavailable_Bool = Animator.StringToHash("Unavailable");

	private readonly AssetTracker _assetTracker = new AssetTracker();

	private AssetLookupSystem _assetLookupSystem;

	private int _invalidCardCount;

	private bool _animateInvalid;

	private bool _animateCraftable;

	private bool _animateUncraftable;

	private bool _animateInvalidCompanion;

	private bool _animateUseHistoricLabel;

	private bool _animateIsFavorite;

	private bool _animateIsSelected;

	private bool _animateUnavailable;

	private bool _setInputFieldActiveOnStart;

	private void Awake()
	{
		_symbolParent.DestroyChildren();
		_customButton.OnClick.AddListener(OnDeckClick);
		_customButton.OnDoubleClick.AddListener(OnDoubleClick);
		_customButton.OnMouseOver.AddListener(OnMouseOver);
		_deckNameInput.onEndEdit.AddListener(OnDeckNameEndEdit);
		if (_deckNameInput.enabled)
		{
			_deckNameInput.enabled = false;
			_setInputFieldActiveOnStart = true;
		}
	}

	private void Start()
	{
		if (_setInputFieldActiveOnStart)
		{
			_deckNameInput.enabled = true;
			_setInputFieldActiveOnStart = false;
		}
	}

	private void OnEnable()
	{
		ApplyVisualUpdates();
	}

	private void OnDestroy()
	{
		_customButton.OnClick.RemoveListener(OnDeckClick);
		_customButton.OnDoubleClick.RemoveListener(OnDoubleClick);
		_deckNameInput.onEndEdit.RemoveListener(OnDeckNameEndEdit);
		if (_meshRendererReferenceLoaders != null)
		{
			foreach (MeshRendererReferenceLoader meshRendererReferenceLoader in _meshRendererReferenceLoaders)
			{
				meshRendererReferenceLoader?.Cleanup();
			}
			_meshRendererReferenceLoaders = null;
		}
		CleanUp();
		_cardBuilder = null;
		_onDeckClicked = null;
		_onDoubleClicked = null;
		_onDeckNameEndEdit = null;
	}

	public void Initialize(ICardBuilder<Meta_CDC> cardViewBuilder, IUnityObjectPool objectPool, AssetLookupSystem assetLookupSystem)
	{
		_cardBuilder = cardViewBuilder;
		_objectPool = objectPool;
		_avatarSelection.Initialize(assetLookupSystem);
		_assetLookupSystem = assetLookupSystem;
	}

	public Guid GetDeckId()
	{
		return _deckViewInfo.deckId;
	}

	public void SetDeckModel(DeckViewInfo deckViewInfo)
	{
		if (deckViewInfo == _deckViewInfo)
		{
			return;
		}
		CleanUp();
		_deckViewInfo = deckViewInfo;
		if (_deckViewInfo != null)
		{
			_deckNameText.SetText(_deckViewInfo.deckName);
			_deckNameInput.text = _deckViewInfo.deckName;
			BuildMeshRendererReferenceLoaders();
			DeckBoxUtil.SetDeckBoxTexture(_deckViewInfo.deckImageAssetPath, _deckViewInfo.crop, _meshRendererReferenceLoaders.ToArray(), _deckViewImages.DefaultDeckTexture);
			if (!SetPet(deckViewInfo.deck.Summary.Pet, deckViewInfo.accountCosmeticDefaults.petSelection))
			{
				Debug.LogWarning($"Can't find pet for deck {deckViewInfo.deckName} ({deckViewInfo.deckId})");
			}
			string avatarId = ((!string.IsNullOrEmpty(deckViewInfo.deck.Summary.Avatar)) ? deckViewInfo.deck.Summary.Avatar : deckViewInfo.accountCosmeticDefaults.avatarSelection);
			_avatarSelection.SetAvatar(avatarId, owned: true, EStoreSection.Avatars);
			SetSleeve(_sleeveParent0, _deckViewInfo.sleeveData);
			SetSleeve(_sleeveParent1, _deckViewInfo.sleeveData);
			SetSleeve(_sleeveParent2, _deckViewInfo.sleeveData);
			SetSleeve(_sleeveParent3, _deckViewInfo.sleeveData);
			if ((bool)_symbolParent)
			{
				SetManaSymbols(_deckViewInfo.manaColors, _symbolParent);
				GameObject go = _symbolParent.gameObject;
				List<Wotc.Mtgo.Gre.External.Messaging.ManaColor> manaColors = _deckViewInfo.manaColors;
				go.UpdateActive(manaColors != null && manaColors.Count > 0);
			}
			_animateUseHistoricLabel = _deckViewInfo.useHistoricLabel;
			_animateIsFavorite = _deckViewInfo.isFavorite;
			ApplyVisualUpdates();
		}
	}

	private bool SetPet(string deckPet, ClientPetSelection defaultPet)
	{
		string text = "";
		string text2 = "";
		if (!string.IsNullOrEmpty(deckPet))
		{
			string[] array = deckPet.Split('.');
			if (array.Length > 1)
			{
				text = array[0];
				text2 = array[1];
			}
		}
		else if (defaultPet != null)
		{
			text = defaultPet.name;
			text2 = defaultPet.variant;
		}
		Sprite sprite = null;
		if (!string.IsNullOrEmpty(text))
		{
			PetPayload petPayload = GetPetPayload(text, text2);
			if (petPayload != null)
			{
				sprite = AssetLoader.AcquireAndTrackAsset(_assetTracker, text + text2, petPayload.Icon);
			}
		}
		if (sprite == null)
		{
			_petIcon.gameObject.SetActive(value: false);
		}
		else
		{
			_petIcon.sprite = sprite;
			_petIcon.gameObject.SetActive(value: true);
		}
		return sprite != null;
	}

	private void SetSleeve(Transform parentSleeve, CardData cardData)
	{
		parentSleeve.DestroyChildren();
		_sleeveCdc = _cardBuilder.CreateCDC(cardData);
		if ((bool)parentSleeve && (bool)_sleeveCdc)
		{
			_sleeveCdc.transform.parent = parentSleeve;
			_sleeveCdc.transform.ZeroOut();
			_sleeveCdc.transform.Rotate(new Vector3(0f, 180f, 0f));
			_sleeveCdc.gameObject.SetActive(value: true);
		}
	}

	private PetPayload GetPetPayload(string petKey, string variant)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.PetId = petKey;
		_assetLookupSystem.Blackboard.PetVariantId = variant;
		return _assetLookupSystem.TreeLoader.LoadTree<PetPayload>().GetPayload(_assetLookupSystem.Blackboard);
	}

	public void UpdateTooltipForFormat(string format, bool isPreconDeck = false)
	{
		string toolTipForDeck = DeckViewUtilities.GetToolTipForDeck(_deckViewInfo.GetValidationForFormat(format), !_deckViewInfo.IsNetDeck && !isPreconDeck);
		SetToolTipText(toolTipForDeck);
	}

	public void UpdateDisplayForFormat(string format, bool allowUnowned, bool isPreconDeck = false)
	{
		DeckDisplayInfo validationForFormat = _deckViewInfo.GetValidationForFormat(format);
		bool animateInvalid = !validationForFormat.IsValid;
		bool flag = false;
		bool flag2 = false;
		bool animateInvalidCompanion = false;
		uint num = validationForFormat.ValidationResult.NumberOfInvalidCards;
		uint bannedCards = 0u;
		UpdateTooltipForFormat(format, isPreconDeck);
		if (num == 0)
		{
			ClientSideDeckValidationResult validationResult = validationForFormat.ValidationResult;
			uint num2 = validationResult.NumberBannedCards + validationResult.NumberEmergencyBannedCards + validationResult.NumberNonFormatCard;
			if (num2 != 0)
			{
				animateInvalid = false;
				num = num2;
				bannedCards = validationForFormat.ValidationResult.NumberBannedCards;
			}
			else if (validationForFormat.IsMalformed)
			{
				animateInvalid = !validationForFormat.IsValid;
			}
			else if (validationForFormat.IsUnowned)
			{
				animateInvalid = false;
				if (!allowUnowned)
				{
					Dictionary<Rarity, uint> wildcardInventory = Pantry.Get<InventoryManager>().Inventory.CombinedWildcardInventory();
					if (validationResult.IsCraftable(wildcardInventory))
					{
						flag = true;
					}
					else
					{
						flag2 = true;
					}
				}
			}
			else if (!validationForFormat.ValidationResult.CompanionIsValid)
			{
				animateInvalidCompanion = true;
			}
		}
		_invalidCardCount = DeckViewUtilities.NumInvalidCards(num, bannedCards, flag, flag2);
		_animateInvalid = animateInvalid;
		_animateCraftable = flag;
		_animateUncraftable = flag2;
		_animateInvalidCompanion = animateInvalidCompanion;
		_animateUseHistoricLabel = _deckViewInfo.useHistoricLabel;
		_animateIsFavorite = _deckViewInfo.isFavorite;
		_warningText.text = num.ToString();
		ApplyVisualUpdates();
	}

	private void ApplyVisualUpdates()
	{
		_animator.SetInteger(Anim_InvalidCards_Int, _invalidCardCount);
		_animator.SetBool(Anim_Invalid_Bool, _invalidCardCount == 0 && _animateInvalid);
		_animator.SetBool(Anim_Craftable_Bool, _animateCraftable);
		_animator.SetBool(Anim_UnCraftable_Bool, _animateUncraftable);
		_animator.SetBool(Anim_Invalid_Companion_Bool, _animateInvalidCompanion);
		_animator.SetBool(Anim_Historic_Bool, _animateUseHistoricLabel);
		_animator.SetBool(Anim_Favorited_Bool, _animateIsFavorite);
		_animator.SetBool(Anim_Selected_Bool, _animateIsSelected);
		_animator.SetBool(Anim_Unavailable_Bool, _animateUnavailable);
		_warningText.text = _invalidCardCount.ToString();
		if (_deckViewInfo != null)
		{
			bool flag = !_deckViewInfo.IsNetDeck && _onDeckNameEndEdit != null;
			_deckNameInput.interactable = flag;
			_iconPencil.gameObject.SetActive(flag);
		}
	}

	public void ClearValidationIcons()
	{
		_invalidCardCount = 0;
		_animateInvalid = false;
		_animateCraftable = false;
		_animateUncraftable = false;
		_animateInvalidCompanion = false;
		SetToolTipText(string.Empty);
		_warningText.text = string.Empty;
		ApplyVisualUpdates();
	}

	public void SetDeckOnClick(Action<DeckViewInfo> onClick)
	{
		_onDeckClicked = onClick;
	}

	public void SetDeckOnDoubleClick(Action<DeckViewInfo> onDoubleClick)
	{
		_onDoubleClicked = onDoubleClick;
	}

	public void SetOnMouseOver(Action<DeckViewInfo> onMouseOver)
	{
		_onMouseOver = onMouseOver;
	}

	public void SetOnDeckNameEndEdit(Action<DeckViewInfo, string> onDeckNameEndEdit)
	{
		_onDeckNameEndEdit = onDeckNameEndEdit;
	}

	public void SetIsSelected(bool isSelected)
	{
		if (isSelected)
		{
			UpdateFX();
			_animateIsSelected = true;
		}
		else
		{
			_animateIsSelected = false;
		}
		ApplyVisualUpdates();
	}

	public void SetIsUnavailable(bool unavailable)
	{
		_animateUnavailable = unavailable;
		ApplyVisualUpdates();
	}

	public void SetToolTipLocString(LocalizedString locString)
	{
		_tooltipTrigger.LocString = locString;
	}

	private void SetToolTipText(string tooltipText)
	{
		_tooltipTrigger.LocString = null;
		_tooltipTrigger.TooltipData.Text = tooltipText;
		_tooltipTrigger.TooltipProperties.MaxVisibleLines = 6;
	}

	private void UpdateFX()
	{
		if (!_fxObject)
		{
			_fxObject = _objectPool.PopObject(_fxPrefab, base.transform);
			_fxObject.gameObject.UpdateActive(active: true);
			_fxObject.transform.SetAsFirstSibling();
		}
	}

	private void OnDeckClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deckbuilding_box_open, base.gameObject);
		_onDeckClicked?.Invoke(_deckViewInfo);
	}

	private void OnDoubleClick()
	{
		_onDoubleClicked?.Invoke(_deckViewInfo);
	}

	private void OnMouseOver()
	{
		_onMouseOver?.Invoke(_deckViewInfo);
	}

	private void OnDeckNameEndEdit(string value)
	{
		_onDeckNameEndEdit?.Invoke(_deckViewInfo, value);
	}

	private void SetManaSymbols(List<Wotc.Mtgo.Gre.External.Messaging.ManaColor> colors, Transform scaffold)
	{
		List<Sprite> list = new List<Sprite>();
		foreach (Wotc.Mtgo.Gre.External.Messaging.ManaColor item in colors.OrderBy((Wotc.Mtgo.Gre.External.Messaging.ManaColor m) => m))
		{
			if (item == Wotc.Mtgo.Gre.External.Messaging.ManaColor.White)
			{
				list.Add(_deckViewImages.WhiteMana);
			}
			if (item == Wotc.Mtgo.Gre.External.Messaging.ManaColor.Blue)
			{
				list.Add(_deckViewImages.BlueMana);
			}
			if (item == Wotc.Mtgo.Gre.External.Messaging.ManaColor.Black)
			{
				list.Add(_deckViewImages.BlackMana);
			}
			if (item == Wotc.Mtgo.Gre.External.Messaging.ManaColor.Red)
			{
				list.Add(_deckViewImages.RedMana);
			}
			if (item == Wotc.Mtgo.Gre.External.Messaging.ManaColor.Green)
			{
				list.Add(_deckViewImages.GreenMana);
			}
		}
		foreach (Sprite item2 in list)
		{
			ManaSymbolView component = _objectPool.PopObject(_symbolPrefab.gameObject, scaffold).GetComponent<ManaSymbolView>();
			component.SymbolImage.sprite = item2;
			Transform obj = component.transform;
			obj.localPosition = Vector3.zero;
			obj.localScale = Vector3.one;
			spawnedManaSymbolViews.Add(component);
		}
	}

	private void HideFX()
	{
		_hideFX(onDisable: false);
	}

	private void _hideFX(bool onDisable)
	{
		if (_fxObject != null)
		{
			_fxObject.SetActive(value: false);
			if (onDisable)
			{
				_objectPool.DeferredPushObject(_fxObject);
			}
			else
			{
				_objectPool.PushObject(_fxObject, worldPositionStays: false);
			}
			_fxObject = null;
		}
	}

	private void BuildMeshRendererReferenceLoaders()
	{
		if (_meshRendererReferenceLoaders != null)
		{
			return;
		}
		_meshRendererReferenceLoaders = new List<MeshRendererReferenceLoader>();
		foreach (MeshRenderer meshRenderer in _meshRenderers)
		{
			_meshRendererReferenceLoaders.Add(new MeshRendererReferenceLoader(meshRenderer));
		}
	}

	private void CleanUp()
	{
		CleanUpSpawnedCards();
		CleanUpManaSymbols();
		HideFX();
	}

	private void CleanUpSpawnedCards()
	{
		if (_metaCdcs != null)
		{
			foreach (Meta_CDC metaCdc in _metaCdcs)
			{
				if ((bool)metaCdc)
				{
					_cardBuilder?.DestroyCDC(metaCdc);
				}
			}
			_metaCdcs.Clear();
		}
		if (_sleeveCdc != null)
		{
			_cardBuilder?.DestroyCDC(_sleeveCdc);
			_sleeveCdc = null;
		}
	}

	private void CleanUpManaSymbols()
	{
		if (spawnedManaSymbolViews == null)
		{
			return;
		}
		foreach (ManaSymbolView spawnedManaSymbolView in spawnedManaSymbolViews)
		{
			if ((bool)spawnedManaSymbolView)
			{
				_objectPool.PushObject(spawnedManaSymbolView.gameObject);
			}
		}
		spawnedManaSymbolViews.Clear();
	}
}
