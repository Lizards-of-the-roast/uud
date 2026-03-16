using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GreClient.CardData;
using Pooling;
using TMPro;
using UnityEngine;
using Wizards.Arena.DeckValidation.Client;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Format;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class MetaDeckView : MonoBehaviour
{
	[SerializeField]
	private GameObject _textContainer;

	[SerializeField]
	private TMP_InputField _nameText;

	[SerializeField]
	private TMP_Text _descriptionText;

	[SerializeField]
	private GameObject _cardContainer;

	[SerializeField]
	private CDCMetaCardView _cardPrefab;

	[SerializeField]
	protected MeshRenderer[] _meshRenderers;

	[SerializeField]
	private Transform _symbolParent;

	[SerializeField]
	private ManaSymbolView _symbolPrefab;

	[SerializeField]
	private CustomButton _button;

	[SerializeField]
	private CustomButton _textButton;

	[SerializeField]
	private TooltipTrigger _tooltip;

	[SerializeField]
	private RectTransform DeckBoxMeshGroup;

	[SerializeField]
	private TMP_Text _numberOfInvalidCardsText;

	[SerializeField]
	private DeckViewImages _deckViewImages;

	private bool _awakeCalled;

	private Animator _rootAnimator;

	[SerializeField]
	private Transform _cdcParent;

	private Meta_CDC _cdc;

	[SerializeField]
	private GameObject _fxPrefab;

	private GameObject _fx;

	protected MeshRendererReferenceLoader[] _meshRendererReferenceLoaders;

	public ClientSideDeckValidationResult DeckValidationResult;

	public string ColorChallengeEventLock;

	protected CardViewBuilder _cardViewBuilder;

	protected CardDatabase _cardDatabase;

	protected FormatManager _formatManager;

	private static readonly int Open = Animator.StringToHash("Open");

	private static readonly int Hover = Animator.StringToHash("Hover");

	private static readonly int Selected = Animator.StringToHash("Selected");

	private static readonly int Uncraftable = Animator.StringToHash("Uncraftable");

	private static readonly int Craftable = Animator.StringToHash("Craftable");

	private static readonly int Invalid = Animator.StringToHash("Invalid");

	private static readonly int NumberOfInvalidCards = Animator.StringToHash("NumberOfInvalidCards");

	public TMP_InputField NameText => _nameText;

	public TMP_Text DescriptionText => _descriptionText;

	public Client_Deck Model { get; private set; }

	public CustomButton Button => _button;

	public Animator RootAnimator
	{
		get
		{
			if (_rootAnimator == null)
			{
				_rootAnimator = GetComponent<Animator>();
			}
			return _rootAnimator;
		}
	}

	public bool IsValid { get; private set; } = true;

	public bool IsCraftable { get; private set; }

	public List<CardPrintingQuantity> CardsNeeded { get; private set; }

	private void Awake()
	{
		_awakeCalled = true;
		tryInitMeshRenderers();
	}

	private void tryInitMeshRenderers()
	{
		if (_meshRendererReferenceLoaders == null)
		{
			_meshRendererReferenceLoaders = new MeshRendererReferenceLoader[_meshRenderers.Length];
			for (int i = 0; i < _meshRenderers.Length; i++)
			{
				_meshRendererReferenceLoaders[i] = new MeshRendererReferenceLoader(_meshRenderers[i]);
			}
		}
	}

	public void Cleanup()
	{
		if (!_awakeCalled && !base.gameObject.activeInHierarchy)
		{
			Transform parent = base.transform.parent;
			base.transform.SetParent(null, worldPositionStays: false);
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
				base.gameObject.SetActive(value: false);
			}
			base.transform.SetParent(parent, worldPositionStays: false);
		}
		if ((bool)_symbolParent)
		{
			_symbolParent.DestroyChildren();
		}
		if ((bool)_textContainer)
		{
			_textContainer.gameObject.SetActive(value: false);
		}
		if ((bool)_symbolParent)
		{
			_symbolParent.gameObject.SetActive(value: false);
		}
	}

	public virtual void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, Client_Deck deck)
	{
		Cleanup();
		_formatManager = Pantry.Get<FormatManager>();
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		if (deck == null)
		{
			return;
		}
		Model = deck;
		Model.Summary.Name = Utils.GetLocalizedDeckName(Model.Summary.Name);
		if ((bool)_textContainer)
		{
			_textContainer.gameObject.SetActive(value: true);
		}
		if ((bool)_nameText)
		{
			_nameText.text = Model.Summary.Name;
		}
		if ((bool)_descriptionText)
		{
			string text = ((!string.IsNullOrEmpty(Model.Summary.Description)) ? Languages.ActiveLocProvider.GetLocalizedText(Model.Summary.Description) : "");
			_descriptionText.text = text;
		}
		string artPath = ((deck.Summary.DeckArtId == 0) ? _cardDatabase.CardDataProvider.GetCardPrintingById(deck.Summary.DeckTileId)?.ImageAssetPath : _cardDatabase.DatabaseUtilities.GetPrintingsByArtId(deck.Summary.DeckArtId)?.FirstOrDefault()?.ImageAssetPath);
		if (_meshRendererReferenceLoaders == null)
		{
			tryInitMeshRenderers();
		}
		DeckBoxUtil.SetDeckBoxTexture(artPath, _cardViewBuilder.CardMaterialBuilder.TextureLoader, _cardViewBuilder.CardMaterialBuilder.CropDatabase, _meshRendererReferenceLoaders, _deckViewImages.DefaultDeckTexture);
		SetDeckBoxSleeve(Model.Summary.CardBack);
		if (!_symbolParent)
		{
			return;
		}
		_symbolParent.gameObject.SetActive(value: true);
		List<Sprite> list = new List<Sprite>();
		bool isLimited = FormatUtilities.IsLimited(_formatManager.GetSafeFormat(deck.Summary.Format).FormatType);
		foreach (ManaColor item in from m in deck.GetDeckColors(_cardDatabase.CardDataProvider, isLimited)
			orderby m
			select m)
		{
			if (item == ManaColor.White)
			{
				list.Add(_deckViewImages.WhiteMana);
			}
			if (item == ManaColor.Blue)
			{
				list.Add(_deckViewImages.BlueMana);
			}
			if (item == ManaColor.Black)
			{
				list.Add(_deckViewImages.BlackMana);
			}
			if (item == ManaColor.Red)
			{
				list.Add(_deckViewImages.RedMana);
			}
			if (item == ManaColor.Green)
			{
				list.Add(_deckViewImages.GreenMana);
			}
		}
		foreach (Sprite item2 in list)
		{
			ManaSymbolView manaSymbolView = Object.Instantiate(_symbolPrefab, _symbolParent, worldPositionStays: true);
			manaSymbolView.SymbolImage.sprite = item2;
			manaSymbolView.transform.ZeroOut();
		}
	}

	public void SetIsValid(DeckDisplayInfo.DeckDisplayState displayState, ClientSideDeckValidationResult validationResult = null)
	{
		IsValid = displayState == DeckDisplayInfo.DeckDisplayState.Valid;
		IsCraftable = displayState == DeckDisplayInfo.DeckDisplayState.Craftable;
		DeckValidationResult = validationResult;
		bool value = !IsValid;
		bool value2 = false;
		bool value3 = false;
		int value4 = 0;
		if (!RootAnimator.isActiveAndEnabled)
		{
			return;
		}
		if (validationResult == null)
		{
			value = !IsValid;
		}
		else
		{
			uint num = validationResult.NumberBannedCards + validationResult.NumberEmergencyBannedCards + validationResult.NumberNonFormatCard;
			if (num != 0)
			{
				value = false;
				value4 = (int)num;
			}
			else if (validationResult.IsMalformed)
			{
				value = !IsValid;
			}
			else if (!IsValid && validationResult.HasUnOwnedCards)
			{
				value = false;
				if (IsCraftable)
				{
					value2 = true;
				}
				else
				{
					value3 = true;
				}
			}
		}
		RootAnimator.SetInteger(NumberOfInvalidCards, value4);
		RootAnimator.SetBool(Invalid, value);
		RootAnimator.SetBool(Craftable, value2);
		RootAnimator.SetBool(Uncraftable, value3);
		_numberOfInvalidCardsText.text = value4.ToString();
	}

	public void SetToolTipText(string tooltipText)
	{
		_tooltip.LocString = null;
		_tooltip.TooltipData.Text = tooltipText;
		_tooltip.TooltipProperties.MaxVisibleLines = 6;
	}

	public void SetToolTipLocString(LocalizedString locString)
	{
		_tooltip.LocString = locString;
	}

	public void SetIsSelected(bool isSelected)
	{
		if (isSelected)
		{
			UpdateFX();
			if (RootAnimator.isActiveAndEnabled)
			{
				RootAnimator.SetTrigger(Selected);
			}
		}
		else if (RootAnimator.isActiveAndEnabled)
		{
			RootAnimator.ResetTrigger(Selected);
		}
	}

	private void UpdateFX()
	{
		if (!_fx && WrapperController.Instance != null)
		{
			_fx = WrapperController.Instance.UnityObjectPool.PopObject(_fxPrefab, base.transform);
			_fx.gameObject.SetActive(value: true);
			_fx.transform.SetAsFirstSibling();
		}
	}

	public void SetSelectedFXActive(bool active)
	{
		if (active)
		{
			UpdateFX();
		}
	}

	private void HideFX()
	{
		_hideFX(onDisable: false);
	}

	private void _hideFX(bool onDisable)
	{
		if (_fx != null && WrapperController.Instance != null)
		{
			_fx.gameObject.SetActive(value: false);
			IUnityObjectPool unityObjectPool = WrapperController.Instance.UnityObjectPool;
			if (onDisable)
			{
				unityObjectPool.DeferredPushObject(_fx);
			}
			else
			{
				unityObjectPool.PushObject(_fx, worldPositionStays: false);
			}
			_fx = null;
		}
	}

	private void OnDisable()
	{
		if (DeckBoxMeshGroup != null)
		{
			DeckBoxMeshGroup.localPosition = new Vector3(0f, 0f, 200f);
			DeckBoxMeshGroup.localRotation = Quaternion.identity;
			DeckBoxMeshGroup.localScale = Vector3.one;
		}
		if (Button != null)
		{
			Button.gameObject.SetActive(value: true);
		}
		_hideFX(onDisable: true);
	}

	private void OnDestroy()
	{
		HideFX();
		if (_meshRendererReferenceLoaders != null)
		{
			MeshRendererReferenceLoader[] meshRendererReferenceLoaders = _meshRendererReferenceLoaders;
			for (int i = 0; i < meshRendererReferenceLoaders.Length; i++)
			{
				meshRendererReferenceLoaders[i]?.Cleanup();
			}
			_meshRendererReferenceLoaders = null;
		}
		if ((bool)Button)
		{
			Button.OnClick.RemoveAllListeners();
		}
		if ((bool)NameText)
		{
			NameText.onEndEdit.RemoveAllListeners();
		}
		if ((bool)_cdc)
		{
			_cardViewBuilder?.DestroyCDC(_cdc);
		}
	}

	public bool IsDeckBoxOpen()
	{
		return RootAnimator.GetCurrentAnimatorStateInfo(2).IsName("RewardInteract_DeckBoxLid_Open");
	}

	public void TriggerOpenEffect()
	{
		if (RootAnimator.isActiveAndEnabled)
		{
			RootAnimator.SetTrigger(Open);
			if (RootAnimator.ContainsParameter(Hover))
			{
				RootAnimator.SetTrigger(Hover);
			}
		}
		if (!(_cardContainer != null))
		{
			return;
		}
		List<Client_DeckCard> list = Model.Contents?.Piles[EDeckPile.Main];
		int num = Mathf.Min(3, list?.Count ?? 0);
		float num2 = 8f;
		float num3 = 0.5f;
		for (int i = 0; i < num; i++)
		{
			uint id = list[i].Id;
			CardData data = new CardData(null, _cardDatabase.CardDataProvider.GetCardPrintingById(id));
			CDCMetaCardView cDCMetaCardView = Object.Instantiate(_cardPrefab, _cardContainer.transform);
			cDCMetaCardView.InitWithData(data, _cardDatabase, _cardViewBuilder);
			float num4 = (float)i / (float)(num - 1);
			Vector3 vector = new Vector3(0f, -2f, 0.05f + (float)i * 0.12f);
			cDCMetaCardView.transform.localPosition = vector;
			foreach (Transform item in cDCMetaCardView.transform)
			{
				item.localPosition = new Vector3(0f, 2f, 0f);
			}
			vector.x = 0f - num3 + 2f * num3 * num4;
			vector.z -= 0.17f;
			cDCMetaCardView.transform.DOLocalMove(vector, 0.75f + (float)i * 0.2f);
			cDCMetaCardView.transform.DOLocalRotate(new Vector3(0f, 0f, num2 - 2f * num2 * num4), 0.5f + (float)i * 0.25f);
		}
	}

	public void ToggleTextBox(bool active)
	{
		if ((bool)_textContainer)
		{
			_textContainer.gameObject.UpdateActive(active);
		}
	}

	public void SetNameIsEditable(bool isEditable)
	{
		NameText.interactable = isEditable;
		_textButton.enabled = isEditable;
	}

	private void SetDeckBoxSleeve(string sleeve)
	{
		DecksManager decksManager = ((WrapperController.Instance != null) ? WrapperController.Instance.DecksManager : null);
		sleeve = WrapperDeckUtilities.GetSleeveOrDefault(sleeve, decksManager);
		CardData data = CardDataExtensions.CreateSkinCard(0u, _cardDatabase, "", sleeve, faceDown: true);
		if (!_cdc)
		{
			_cdc = _cardViewBuilder.CreateMetaCdc(data, _cdcParent);
		}
		else
		{
			_cdc.SetModel(data);
		}
	}
}
