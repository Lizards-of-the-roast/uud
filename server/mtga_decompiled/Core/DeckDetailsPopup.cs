using System;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Code.Decks;
using Core.Meta.MainNavigation.Cosmetics;
using GreClient.CardData;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.MDN;
using Wizards.MDN.DeckManager;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class DeckDetailsPopup : PopupBase
{
	public CanvasGroup CanvasGroup;

	public CustomButton[] CloseButtons;

	public DeckColorsDetails ColorsWidget;

	public DeckCostsDetails CostsWidget;

	public DeckTypesDetails TypesWidget;

	public GameObject FormatContainer;

	public TMP_Dropdown FormatDropdown;

	public CustomButton FormatButton;

	public MeshRenderer[] _deckBoxRenderers;

	public Transform _cdcParent;

	public CosmeticSelectorController CosmeticSelector;

	public Transform DisplayItemGridTransform;

	public Transform SelectorTransform;

	public TMP_InputField _deckNameInput;

	[SerializeField]
	private EventTrigger _deckBoxHitbox;

	[SerializeField]
	private DeckViewImages _deckViewImages;

	public Action<Action> OnStoreSelected;

	private List<DeckFormat> _availableFormats;

	private Meta_CDC _cdc;

	private MeshRendererReferenceLoader[] _meshRendererReferenceLoaders;

	private Animator _deckDetailsAnimator;

	private bool _isReadOnly;

	private CosmeticsProvider _cosmeticsProvider;

	private static readonly int Locked = Animator.StringToHash("Locked");

	private DeckBuilderActionsHandler ActionsHandler => Pantry.Get<DeckBuilderActionsHandler>();

	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderContext Context => ContextProvider.Context;

	private DeckBuilderModelProvider DeckBuilderModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderModel Model => DeckBuilderModelProvider.Model;

	public event Action<DeckFormat> OnFormatSelected;

	public event Action<string> OnNameChanged;

	public event Action<AvatarSelection> OnAvatarSelected;

	public event Action<string> OnSleeveSelected;

	public event Action<PetEntry> OnPetSelected;

	public event Action<CosmeticType> OnDefaultCosmeticSelected;

	public event Action<List<string>> OnEmotesSelected;

	protected override void Awake()
	{
		base.Awake();
		_deckDetailsAnimator = GetComponent<Animator>();
		CustomButton[] closeButtons = CloseButtons;
		for (int i = 0; i < closeButtons.Length; i++)
		{
			closeButtons[i].OnClick.AddListener(delegate
			{
				base.gameObject.UpdateActive(active: false);
			});
		}
		FormatButton.OnMouseover.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
		});
		FormatButton.OnClick.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_filter_toggle, AudioManager.Default);
		});
		_meshRendererReferenceLoaders = new MeshRendererReferenceLoader[_deckBoxRenderers.Length];
		for (int num = 0; num < _deckBoxRenderers.Length; num++)
		{
			_meshRendererReferenceLoaders[num] = new MeshRendererReferenceLoader(_deckBoxRenderers[num]);
		}
		_deckNameInput.onEndEdit.AddListener(delegate(string s)
		{
			Pantry.Get<DeckBuilderModelProvider>().SetDeckName(s);
			this.OnNameChanged?.Invoke(s);
		});
	}

	private void OnDestroy()
	{
		if (_meshRendererReferenceLoaders != null)
		{
			MeshRendererReferenceLoader[] meshRendererReferenceLoaders = _meshRendererReferenceLoaders;
			for (int i = 0; i < meshRendererReferenceLoaders.Length; i++)
			{
				meshRendererReferenceLoaders[i]?.Cleanup();
			}
			_meshRendererReferenceLoaders = null;
		}
		ActionsHandler.DeckDetailsRequested -= OnRequestDeckDetails;
		ActionsHandler.DeckDetailsCosmeticsSelectorRequested -= DeckBuilderOpenCosmeticSelector;
	}

	public override void OnEnter()
	{
	}

	public override void OnEscape()
	{
	}

	public void Init(IClientLocProvider locMan, CosmeticsProvider cosmetics, AvatarCatalog avatarCatalog, PetCatalog petCatalog, AssetLookupSystem assetLookupSystem, IDeckSleeveProvider deckManager, ICardRolloverZoom zoomHandler, IBILogger logger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, IEmoteDataProvider emoteDataProvider, IUnityObjectPool objectPool, StoreManager storeManager, bool isReadOnly)
	{
		_cosmeticsProvider = cosmetics;
		CosmeticSelector.Init(DisplayItemGridTransform, SelectorTransform, locMan, cosmetics, avatarCatalog, petCatalog, assetLookupSystem, zoomHandler, logger, cardDatabase, cardViewBuilder, emoteDataProvider, objectPool, deckManager, storeManager, isReadOnly);
		CosmeticSelector.SetOnAvatarSelected(delegate(AvatarSelection avatar)
		{
			this.OnAvatarSelected?.Invoke(avatar);
			DeckBuilderModelProvider.SetSelectedAvatar(avatar);
		});
		CosmeticSelector.SetOnSleeveSelected(delegate(string sleeve)
		{
			this.OnSleeveSelected?.Invoke(sleeve);
			DeckBuilderModelProvider.SetSelectedSleeve(sleeve);
		});
		CosmeticSelector.SetOnPetSelected(delegate(PetEntry petEntry)
		{
			this.OnPetSelected?.Invoke(petEntry);
			DeckBuilderModelProvider.SetSelectedPet(petEntry);
		});
		CosmeticSelector.SetOnDefaultCosmeticSelected(HandleDefaultCosmeticSelected);
		CosmeticSelector.SetOnEmoteSelected(delegate(List<string> emoteList)
		{
			this.OnEmotesSelected?.Invoke(emoteList);
			DeckBuilderModelProvider.SetSelectedEmotes(emoteList);
		});
		CosmeticSelector.SetOnStoreCallback(onStoreRedirect);
	}

	public void SetDeckDetailsRequested(bool addListener)
	{
		if (addListener)
		{
			ActionsHandler.DeckDetailsRequested += OnRequestDeckDetails;
			ActionsHandler.DeckDetailsCosmeticsSelectorRequested += DeckBuilderOpenCosmeticSelector;
		}
		else
		{
			ActionsHandler.DeckDetailsRequested -= OnRequestDeckDetails;
			ActionsHandler.DeckDetailsCosmeticsSelectorRequested -= DeckBuilderOpenCosmeticSelector;
		}
	}

	public void UpdateDetailsPopup(bool isReadOnly)
	{
		SetInteractable(!isReadOnly);
		SetDeck(Model.GetFilteredMainDeck(), Pantry.Get<CardDatabase>().GreLocProvider);
		Client_Deck clientDeckInfo = DeckServiceWrapperHelpers.ToClientModel(Model.GetServerModel());
		SetCosmeticsData(clientDeckInfo, isReadOnly);
		SetDeckName(Model._deckName);
		_isReadOnly = isReadOnly;
		CosmeticSelector.UpdateReadOnly(isReadOnly);
		if (Context.IsEvent)
		{
			SetStaticFormat(Context.Format);
			return;
		}
		List<DeckFormat> availableFormats = DeckBuilderWidgetUtilities.GetAvailableFormats(Pantry.Get<FormatManager>().GetAllFormats(), Pantry.Get<EventManager>().EventContexts, Context.Format);
		SetSelectableFormat(Context.Format, availableFormats);
	}

	public void SetDeck(IReadOnlyList<CardPrintingQuantity> deck, IGreLocProvider locMan)
	{
		ColorsWidget.SetDeck(deck);
		CostsWidget.SetDeck(deck);
		TypesWidget.SetDeck(deck, locMan);
	}

	public void SetDeckName(string deckName)
	{
		_deckNameInput.text = deckName;
	}

	public void SetCosmeticsData(Client_Deck clientDeckInfo, bool isReadOnly = false)
	{
		CosmeticSelector.SetData(clientDeckInfo, _cosmeticsProvider._vanitySelections, isReadOnly);
		CosmeticSelector.CloseAllCosmeticSelectors();
	}

	public void SetDeckBoxTexture(string artAssetPath, ArtCrop crop)
	{
		DeckBoxUtil.SetDeckBoxTexture(artAssetPath, crop, _meshRendererReferenceLoaders, _deckViewImages.DefaultDeckTexture);
	}

	public void OnDeckCardBackSet(string cardBack)
	{
		if (base.IsShowing)
		{
			SetDeckBoxSleeve(cardBack, Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>());
		}
	}

	public void SetDeckBoxSleeve(string sleeve, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		CardData data = CardDataExtensions.CreateSkinCard(0u, cardDatabase, null, sleeve, faceDown: true);
		if (_cdc == null)
		{
			_cdc = cardViewBuilder.CreateMetaCdc(data, _cdcParent);
			return;
		}
		_cdc.SetModel(data);
		_cdc.UpdateVisuals();
	}

	public void SetStaticFormat(DeckFormat format)
	{
		SetFormat(format, new List<DeckFormat> { format }, interactable: false);
	}

	public void SetInteractable(bool isInteractable)
	{
		_deckBoxHitbox.enabled = isInteractable;
		_deckNameInput.interactable = isInteractable;
		_deckDetailsAnimator.SetBool(Locked, !isInteractable);
	}

	public void FocusDeckNameInput()
	{
		EventSystem.current.SetSelectedGameObject(_deckNameInput.gameObject, null);
	}

	public void SetSelectableFormat(DeckFormat format, List<DeckFormat> availableFormats)
	{
		SetFormat(format, availableFormats, interactable: true);
	}

	public void OnRequestDeckDetails()
	{
		Activate(activate: true);
		AudioManager.PlayAudio("sfx_ui_main_deck_details", base.gameObject);
	}

	public void DeckBuilderOpenCosmeticSelector(CardDatabase cardDatabase, DisplayCosmeticsTypes cosmeticsType)
	{
		Activate(activate: true);
		SetDeck(Model.GetFilteredMainDeck(), cardDatabase.GreLocProvider);
		Client_Deck clientDeckInfo = DeckServiceWrapperHelpers.ToClientModel(Model.GetServerModel());
		SetCosmeticsData(clientDeckInfo);
		SetDeckName(Model._deckName);
		CosmeticSelector.OpenCosmeticSelector(cosmeticsType);
	}

	private void onStoreRedirect(Action storeRedirect)
	{
		if (OnStoreSelected != null)
		{
			OnStoreSelected(storeRedirect);
		}
		else
		{
			storeRedirect();
		}
	}

	private void SetFormat(DeckFormat format, List<DeckFormat> availableFormats, bool interactable)
	{
		FormatDropdown.onValueChanged.RemoveListener(FormatDropdown_OnValueChanged);
		_availableFormats = availableFormats;
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		int value = 0;
		for (int i = 0; i < _availableFormats.Count; i++)
		{
			list.Add(new TMP_Dropdown.OptionData(_availableFormats[i].GetLocalizedName()));
			if (format == _availableFormats[i])
			{
				value = i;
			}
		}
		FormatDropdown.options = list;
		FormatDropdown.value = value;
		FormatDropdown.interactable = interactable;
		if (interactable)
		{
			FormatDropdown.onValueChanged.AddListener(FormatDropdown_OnValueChanged);
		}
	}

	protected override void Show()
	{
		base.Show();
		var (artAssetPath, crop) = DeckBuilderModelProvider.GetDeckBoxTextureInformation(Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>(), DeckBuilderModelProvider.Model._deckTileId, DeckBuilderModelProvider.Model._deckArtId);
		SetDeckBoxTexture(artAssetPath, crop);
		OnDeckCardBackSet(DeckBuilderModelProvider.Model._cardBack);
		UpdateDetailsPopup(Context.IsReadOnly || Context.IsSideboarding);
	}

	private void FormatDropdown_OnValueChanged(int value)
	{
		ContextProvider.SelectFormat(_availableFormats[value]);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
	}

	public void DisableSleeveSelection()
	{
		if (_deckBoxHitbox != null)
		{
			Image component = _deckBoxHitbox.GetComponent<Image>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}

	private void HandleDefaultCosmeticSelected(CosmeticType cosmeticType)
	{
		this.OnDefaultCosmeticSelected?.Invoke(cosmeticType);
		DeckBuilderModelProvider.OnDefaultCosmeticSelected(cosmeticType);
	}

	private void ResetLanguage()
	{
		if (base.IsShowing)
		{
			UpdateDetailsPopup(Context.IsReadOnly || Context.IsSideboarding);
		}
	}

	private void OnEnable()
	{
		Languages.LanguageChangedSignal.Listeners += ResetLanguage;
		_deckDetailsAnimator.SetBool(Locked, _isReadOnly);
		DeckBuilderModelProvider.OnDeckBoxTextureChanged += SetDeckBoxTexture;
		DeckBuilderModelProvider.OnDeckCardBackSet += OnDeckCardBackSet;
	}

	private void OnDisable()
	{
		Languages.LanguageChangedSignal.Listeners -= ResetLanguage;
		DeckBuilderModelProvider.OnDeckBoxTextureChanged -= SetDeckBoxTexture;
		DeckBuilderModelProvider.OnDeckCardBackSet -= OnDeckCardBackSet;
	}
}
