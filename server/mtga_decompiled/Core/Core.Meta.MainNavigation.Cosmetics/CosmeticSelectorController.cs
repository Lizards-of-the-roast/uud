using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Pooling;
using UnityEngine;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.MDN.DeckManager;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Inventory;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Cosmetics;

public class CosmeticSelectorController : MonoBehaviour
{
	[SerializeField]
	private List<DisplayCosmeticsTypes> _displayItemOrder;

	private Dictionary<DisplayCosmeticsTypes, DisplayItemCosmeticBase> _displayItems = new Dictionary<DisplayCosmeticsTypes, DisplayItemCosmeticBase>();

	private Action _onClose;

	private Action _onOpen;

	private Action<PetEntry> _onPetSelected;

	private Action<AvatarSelection> _onAvatarSelected;

	private Action<string> _onSleeveSelected;

	private Action<CosmeticType> _onDefaultCosmeticSelected;

	private Action<List<string>> _onEmoteSelected;

	private Transform _selectorTransform;

	private CosmeticsProvider _cosmeticsProvider;

	private IBILogger _logger;

	private IClientLocProvider _locMan;

	private AvatarCatalog _avatarCatalog;

	private PetCatalog _petCatalog;

	private AssetLookupSystem _assetLookupSystem;

	private ICardRolloverZoom _zoomHandler;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private IEmoteDataProvider _emoteDataProvider;

	private IUnityObjectPool _objectPool;

	private IDeckSleeveProvider _sleeveProvider;

	private Action<Action> _storeAction;

	private StoreManager _storeManager;

	private Transform _displayItemGrid;

	private bool _isReadOnly;

	public void Init(Transform selectorTransform, IClientLocProvider locMan, CosmeticsProvider cosmeticsProvider, AvatarCatalog avatarCatalog, PetCatalog petCatalog, AssetLookupSystem assetLookupSystem, ICardRolloverZoom zoomHandler, IBILogger logger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, IEmoteDataProvider emoteDataProvider, IUnityObjectPool objectPool, IDeckSleeveProvider sleeveUtils, StoreManager storeManager)
	{
		_storeManager = storeManager;
		_selectorTransform = selectorTransform;
		_locMan = locMan;
		_cosmeticsProvider = cosmeticsProvider;
		_avatarCatalog = avatarCatalog;
		_petCatalog = petCatalog;
		_assetLookupSystem = assetLookupSystem;
		_zoomHandler = zoomHandler;
		_logger = logger;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_emoteDataProvider = emoteDataProvider;
		_objectPool = objectPool;
		_sleeveProvider = sleeveUtils;
	}

	public void Init(Transform displayItemGridTransform, Transform selectorTransform, IClientLocProvider locMan, CosmeticsProvider cosmeticsProvider, AvatarCatalog avatarCatalog, PetCatalog petCatalog, AssetLookupSystem assetLookupSystem, ICardRolloverZoom zoomHandler, IBILogger logger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, IEmoteDataProvider emoteDataProvider, IUnityObjectPool objectPool, IDeckSleeveProvider sleeveUtils, StoreManager storeManager, bool isReadOnly)
	{
		_displayItemGrid = displayItemGridTransform;
		Init(selectorTransform, locMan, cosmeticsProvider, avatarCatalog, petCatalog, assetLookupSystem, zoomHandler, logger, cardDatabase, cardViewBuilder, emoteDataProvider, objectPool, sleeveUtils, storeManager);
		UpdateReadOnly(isReadOnly);
	}

	public void UpdateReadOnly(bool isReadOnly)
	{
		_isReadOnly = isReadOnly;
		foreach (DisplayCosmeticsTypes item in _displayItemOrder)
		{
			if (!_displayItems.ContainsKey(item))
			{
				SetupCosmeticDisplayItemByType(item, _displayItemGrid);
			}
		}
	}

	private void SetupCosmeticDisplayItemByType(DisplayCosmeticsTypes type, Transform displayItemGridTransform)
	{
		switch (type)
		{
		case DisplayCosmeticsTypes.Avatar:
			SetupAvatarDisplayItem(displayItemGridTransform);
			break;
		case DisplayCosmeticsTypes.Emote:
			SetupEmoteDisplayItem(displayItemGridTransform);
			break;
		case DisplayCosmeticsTypes.Pet:
			SetupPetDisplayItem(displayItemGridTransform);
			break;
		case DisplayCosmeticsTypes.Sleeve:
			SetupSleeveDisplayItem(displayItemGridTransform);
			break;
		case DisplayCosmeticsTypes.Title:
			SetupTitleDisplayItem(displayItemGridTransform);
			break;
		}
	}

	public void InitAvatar(DisplayItemAvatar displayItemAvatar)
	{
		displayItemAvatar.Init(_selectorTransform, _cosmeticsProvider, _assetLookupSystem, _avatarCatalog, _isReadOnly);
		displayItemAvatar.SetOnCosmeticSelected(OnAvatarSelected);
		displayItemAvatar.SetOnDefaultSelected(SetAvatarDefault);
		SetOpenCloseCallbacks(displayItemAvatar);
		_displayItems.Add(DisplayCosmeticsTypes.Avatar, displayItemAvatar);
	}

	public void InitPet(DisplayItemPet displayItemPet)
	{
		displayItemPet.Init(_selectorTransform, _cosmeticsProvider, _assetLookupSystem, _petCatalog, _objectPool, _locMan, _isReadOnly);
		displayItemPet.SetOnCosmeticSelected(OnPetSelected);
		displayItemPet.SetOnDefaultSelected(SetPetDefault);
		SetOpenCloseCallbacks(displayItemPet);
		_displayItems.Add(DisplayCosmeticsTypes.Pet, displayItemPet);
	}

	public void InitSleeve(DisplayItemSleeve displayItemSleeve)
	{
		displayItemSleeve.Init(_selectorTransform, _cosmeticsProvider, _assetLookupSystem, _zoomHandler, _logger, _cardDatabase, _cardViewBuilder, _storeManager, _isReadOnly);
		displayItemSleeve.SetOnCosmeticSelected(OnSleeveSelected);
		displayItemSleeve.SetOnDefaultSelected(SetSleeveDefault);
		SetOpenCloseCallbacks(displayItemSleeve);
		_displayItems.Add(DisplayCosmeticsTypes.Sleeve, displayItemSleeve);
	}

	public void InitEmote(DisplayItemEmote displayItemEmote)
	{
		displayItemEmote.Init(_selectorTransform, _cosmeticsProvider, _assetLookupSystem, _emoteDataProvider, _locMan, _logger, _isReadOnly);
		displayItemEmote.SetOnCosmeticSelected(OnEmoteSelected);
		SetOpenCloseCallbacks(displayItemEmote);
		_displayItems.Add(DisplayCosmeticsTypes.Emote, displayItemEmote);
	}

	public void InitTitle(DisplayItemTitle displayItemTitle)
	{
		displayItemTitle.Init(_selectorTransform, _assetLookupSystem);
		SetOpenCloseCallbacks(displayItemTitle);
		_displayItems.Add(DisplayCosmeticsTypes.Title, displayItemTitle);
	}

	public void SetData(Client_Deck deck, ClientVanitySelectionsV3 defaultData, bool isReadOnly = false)
	{
		Client_DeckSummary summary = deck.Summary;
		DisplayItemSleeve displayCosmetic = GetDisplayCosmetic<DisplayItemSleeve>(DisplayCosmeticsTypes.Sleeve);
		if (displayCosmetic != null)
		{
			displayCosmetic.SetData(summary.CardBack, defaultData.cardBackSelection, showDefaultInterface: true, isReadOnly);
		}
		DisplayItemAvatar displayCosmetic2 = GetDisplayCosmetic<DisplayItemAvatar>(DisplayCosmeticsTypes.Avatar);
		if (displayCosmetic2 != null)
		{
			displayCosmetic2.SetData(summary.Avatar, defaultData.avatarSelection, showDefaultInterface: true, isReadOnly);
		}
		DisplayItemPet displayCosmetic3 = GetDisplayCosmetic<DisplayItemPet>(DisplayCosmeticsTypes.Pet);
		if (displayCosmetic3 != null)
		{
			string defaultPet = string.Empty;
			if (defaultData.petSelection != null)
			{
				defaultPet = defaultData.petSelection.name + "." + defaultData.petSelection.variant;
			}
			displayCosmetic3.SetData(_cosmeticsProvider.PlayerOwnedPets, summary.Pet, defaultPet, showDefaultInterface: true, isReadOnly);
		}
		DisplayItemEmote displayCosmetic4 = GetDisplayCosmetic<DisplayItemEmote>(DisplayCosmeticsTypes.Emote);
		if (displayCosmetic4 != null)
		{
			displayCosmetic4.SetData(summary.Emotes, isReadOnly);
		}
	}

	public void SetData(ClientVanitySelectionsV3 profileCosmetics)
	{
		DisplayItemSleeve displayCosmetic = GetDisplayCosmetic<DisplayItemSleeve>(DisplayCosmeticsTypes.Sleeve);
		if (displayCosmetic != null)
		{
			displayCosmetic.SetData(profileCosmetics.cardBackSelection);
		}
		DisplayItemAvatar displayCosmetic2 = GetDisplayCosmetic<DisplayItemAvatar>(DisplayCosmeticsTypes.Avatar);
		if (displayCosmetic2 != null)
		{
			displayCosmetic2.SetData(profileCosmetics.avatarSelection);
		}
		DisplayItemPet displayCosmetic3 = GetDisplayCosmetic<DisplayItemPet>(DisplayCosmeticsTypes.Pet);
		if (displayCosmetic3 != null)
		{
			string currentPet = string.Empty;
			if (profileCosmetics.petSelection != null)
			{
				currentPet = profileCosmetics.petSelection.name + "." + profileCosmetics.petSelection.variant;
			}
			displayCosmetic3.SetData(_cosmeticsProvider.PlayerOwnedPets, currentPet);
		}
		DisplayItemEmote displayCosmetic4 = GetDisplayCosmetic<DisplayItemEmote>(DisplayCosmeticsTypes.Emote);
		if (displayCosmetic4 != null)
		{
			displayCosmetic4.SetData(profileCosmetics.emoteSelections, isReadOnly: false);
		}
		DisplayItemTitle displayCosmetic5 = GetDisplayCosmetic<DisplayItemTitle>(DisplayCosmeticsTypes.Title);
		if (displayCosmetic5 != null)
		{
			displayCosmetic5.SetData(profileCosmetics.titleSelection);
		}
	}

	public void CloseAllCosmeticSelectors()
	{
		foreach (KeyValuePair<DisplayCosmeticsTypes, DisplayItemCosmeticBase> displayItem in _displayItems)
		{
			displayItem.Value.CloseSelector();
		}
	}

	public void OpenCosmeticSelector(DisplayCosmeticsTypes cosmeticsType)
	{
		if (!_isReadOnly && _displayItems.TryGetValue(cosmeticsType, out var value))
		{
			value.OpenSelector();
		}
	}

	private (string key, string varriant) SplitPetString(string petId)
	{
		(string, string) result = ("", "");
		if (!string.IsNullOrEmpty(petId) && petId.Contains("."))
		{
			string[] array = petId.Split('.');
			return (key: array[0], varriant: array[1]);
		}
		return result;
	}

	private T GetDisplayCosmetic<T>(DisplayCosmeticsTypes type) where T : DisplayItemCosmeticBase
	{
		if (_displayItems.TryGetValue(type, out var value))
		{
			return (T)value;
		}
		return null;
	}

	private void SetAvatarDefault(string avatarId)
	{
		StartCoroutine(Coroutine_SetAvatarDefault(avatarId));
	}

	private IEnumerator Coroutine_SetAvatarDefault(string avatarId)
	{
		Promise<PreferredCosmetics> promise = _cosmeticsProvider.SetAvatarSelection(avatarId);
		yield return promise.AsCoroutine();
		if (promise.Successful)
		{
			_onDefaultCosmeticSelected?.Invoke(CosmeticType.Avatar);
		}
	}

	private void SetEmoteDefault(List<string> emotes)
	{
		_cosmeticsProvider.SetEmoteSelections(emotes);
		_onDefaultCosmeticSelected?.Invoke(CosmeticType.Emote);
	}

	private void SetPetDefault()
	{
		_onDefaultCosmeticSelected?.Invoke(CosmeticType.Pet);
	}

	private void SetSleeveDefault()
	{
		_onDefaultCosmeticSelected?.Invoke(CosmeticType.Sleeve);
	}

	public void SetOnCloseCallback(Action onClose)
	{
		_onClose = onClose;
	}

	public void SetOnOpenCallback(Action onOpen)
	{
		_onOpen = onOpen;
	}

	public void SetOnStoreCallback(Action<Action> storeAction)
	{
		_storeAction = storeAction;
	}

	public void SetOnPetSelected(Action<PetEntry> onPetSelected)
	{
		_onPetSelected = onPetSelected;
	}

	public void SetOnAvatarSelected(Action<AvatarSelection> onAvatarSelected)
	{
		_onAvatarSelected = onAvatarSelected;
	}

	public void SetOnSleeveSelected(Action<string> onSleeveSelected)
	{
		_onSleeveSelected = onSleeveSelected;
	}

	public void SetOnDefaultCosmeticSelected(Action<CosmeticType> onDefaultCosmeticSelected)
	{
		_onDefaultCosmeticSelected = onDefaultCosmeticSelected;
	}

	public void SetOnEmoteSelected(Action<List<string>> onEmoteSelected)
	{
		_onEmoteSelected = onEmoteSelected;
	}

	private void SetupSleeveDisplayItem(Transform displayItemGridTransform)
	{
		string prefabPathFromALT = GetPrefabPathFromALT(DisplayCosmeticsTypes.Sleeve.ToString(), _assetLookupSystem);
		if (!string.IsNullOrEmpty(prefabPathFromALT))
		{
			DisplayItemSleeve displayItemSleeve = AssetLoader.Instantiate<DisplayItemSleeve>(prefabPathFromALT, displayItemGridTransform);
			InitSleeve(displayItemSleeve);
		}
	}

	private void SetupAvatarDisplayItem(Transform displayItemGridTransform)
	{
		string prefabPathFromALT = GetPrefabPathFromALT(DisplayCosmeticsTypes.Avatar.ToString(), _assetLookupSystem);
		if (!string.IsNullOrEmpty(prefabPathFromALT))
		{
			DisplayItemAvatar displayItemAvatar = AssetLoader.Instantiate<DisplayItemAvatar>(prefabPathFromALT, displayItemGridTransform);
			InitAvatar(displayItemAvatar);
		}
	}

	private void SetupPetDisplayItem(Transform displayItemGridTransform)
	{
		string prefabPathFromALT = GetPrefabPathFromALT(DisplayCosmeticsTypes.Pet.ToString(), _assetLookupSystem);
		if (!string.IsNullOrEmpty(prefabPathFromALT))
		{
			DisplayItemPet displayItemPet = AssetLoader.Instantiate<DisplayItemPet>(prefabPathFromALT, displayItemGridTransform);
			InitPet(displayItemPet);
		}
	}

	private void SetupEmoteDisplayItem(Transform displayItemGridTransform)
	{
		string prefabPathFromALT = GetPrefabPathFromALT(DisplayCosmeticsTypes.Emote.ToString(), _assetLookupSystem);
		if (!string.IsNullOrEmpty(prefabPathFromALT))
		{
			DisplayItemEmote displayItemEmote = AssetLoader.Instantiate<DisplayItemEmote>(prefabPathFromALT, displayItemGridTransform);
			InitEmote(displayItemEmote);
		}
	}

	private void SetupTitleDisplayItem(Transform displayItemGridTransform)
	{
		string prefabPathFromALT = GetPrefabPathFromALT(DisplayCosmeticsTypes.Title.ToString(), _assetLookupSystem);
		if (!string.IsNullOrEmpty(prefabPathFromALT))
		{
			DisplayItemTitle displayItemTitle = AssetLoader.Instantiate<DisplayItemTitle>(prefabPathFromALT, displayItemGridTransform);
			InitTitle(displayItemTitle);
		}
	}

	private string GetPrefabPathFromALT(string lookUpString, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.LookupString = lookUpString;
		string text = (assetLookupSystem.TreeLoader.LoadTree<DisplayItemCosmeticPrefab>()?.GetPayload(assetLookupSystem.Blackboard))?.PrefabPath;
		if (text == null)
		{
			SimpleLog.LogError("Could not find prefab for lookup: " + lookUpString);
			return "";
		}
		return text;
	}

	private void SetOpenCloseCallbacks(DisplayItemCosmeticBase displayItem)
	{
		displayItem.SetOnCloseCallback(OnCloseSelector);
		displayItem.SetOnOpenCallback(OnOpenSelector);
		displayItem.SetOnStoreCallback(OnStoreOpen);
	}

	private void OnOpenSelector()
	{
		_onOpen?.Invoke();
	}

	private void OnCloseSelector()
	{
		_onClose?.Invoke();
	}

	private void OnStoreOpen(Action storeRedirect)
	{
		if (_storeAction != null)
		{
			_storeAction(storeRedirect);
		}
		else
		{
			storeRedirect();
		}
	}

	private void OnPetSelected(PetEntry selectedPet)
	{
		_onPetSelected?.Invoke(selectedPet);
	}

	private void OnAvatarSelected(AvatarSelection selectedAvatar)
	{
		_onAvatarSelected?.Invoke(selectedAvatar);
	}

	private void OnSleeveSelected(string sleeve)
	{
		_onSleeveSelected?.Invoke(sleeve);
	}

	private void OnEmoteSelected(List<string> emotes)
	{
		_onEmoteSelected?.Invoke(emotes);
	}
}
