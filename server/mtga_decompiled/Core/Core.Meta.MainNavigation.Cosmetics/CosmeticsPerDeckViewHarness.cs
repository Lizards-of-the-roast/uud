using System;
using System.Collections;
using System.Collections.Generic;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Code.Harnesses;
using Pooling;
using UnityEngine;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.MDN.DeckManager;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Network.ServiceWrappers.Utils;
using Wotc.Mtga.Providers;
using com.wizards.harness;

namespace Core.Meta.MainNavigation.Cosmetics;

public class CosmeticsPerDeckViewHarness : Harness<CosmeticsPerDeckView, HarnessDropdownUI>
{
	[Serializable]
	public class CosmeticsPerDeckViewModel
	{
		public bool useProfileOnly;

		public string Description;

		public string currentSleeve;

		public string currentAvatar;

		public string currentPet;

		public List<string> currentEmotes;

		public string accountSleeve;

		public string accountAvatar;

		public string accountPet;

		public List<string> accountEmotes;

		public List<HarnessAvatarEntry> AvatarEntries;

		public List<HarnessPetEntry> PetEntries;

		public List<HarnessCardBackSelectorDisplayData> CardBackSelectorDisplayData;

		public List<HarnessEmoteData> EmoteEntries;

		public override string ToString()
		{
			return Description;
		}
	}

	[Serializable]
	public class HarnessEmoteData
	{
		public string Id;

		public string Category;

		public Wizards.Arena.Enums.Cosmetic.EmotePage page;

		public bool isDefault;

		public EmoteData ToEmoteData()
		{
			return new EmoteData(Id, new ClientEmoteEntry(Id, page, isDefault, Category));
		}
	}

	[Serializable]
	public class HarnessCardBackSelectorDisplayData
	{
		public string Name;

		public bool Collected;

		public EStoreSection StoreSection;

		public CardBackSelectorDisplayData ToCardBackSelector()
		{
			return new CardBackSelectorDisplayData(Name, Collected, StoreSection);
		}
	}

	[Serializable]
	public class HarnessAvatarEntry
	{
		public string Id = "";

		public CatalogType Type;

		public Wizards.Arena.Enums.Cosmetic.AcquisitionFlags Source;
	}

	[Serializable]
	public class HarnessPetEntry : PetEntry
	{
		public bool isOwned;
	}

	private class EmptyEmoteDataProvider : IEmoteDataProvider
	{
		private List<EmoteData> EmoteDatas = new List<EmoteData>();

		public void SetEmoteData(List<EmoteData> emoteDatas)
		{
			EmoteDatas = emoteDatas;
		}

		public EmoteData GetEmoteData(string id)
		{
			for (int i = 0; i < EmoteDatas.Count; i++)
			{
				if (EmoteDatas[i].Id == id)
				{
					return EmoteDatas[i];
				}
			}
			return null;
		}

		public void Update(IReadOnlyCollection<ClientEmoteEntry> emoteEntries)
		{
		}

		public IReadOnlyCollection<EmoteData> GetAllEmoteData()
		{
			return EmoteDatas;
		}

		public IReadOnlyCollection<EmoteData> GetDefaultEmoteData()
		{
			return new List<EmoteData>
			{
				EmoteDatas[0],
				EmoteDatas[1],
				EmoteDatas[2],
				EmoteDatas[3],
				EmoteDatas[4]
			};
		}
	}

	private class EmptyDecksManager : IDeckSleeveProvider
	{
		public string GetDefaultSleeve()
		{
			return "CardBack_Basic_AllAccess_ELD";
		}

		public IEnumerator Coroutine_UpdateAllDecksWithDefaultSleeve()
		{
			yield return null;
		}
	}

	public class EmptyCosmeticsServiceWrapper : ICosmeticsServiceWrapper
	{
		public Promise<CosmeticsClient> GetPlayerCosmetics()
		{
			return new SimplePromise<CosmeticsClient>(new CosmeticsClient());
		}

		public Promise<PreferredCosmetics> GetPlayerPreferredCosmetics()
		{
			return new SimplePromise<PreferredCosmetics>(new PreferredCosmetics());
		}

		public Promise<PreferredCosmetics> SetPetSelection(string name, string variant)
		{
			return new SimplePromise<PreferredCosmetics>(new PreferredCosmetics());
		}

		public Promise<PreferredCosmetics> SetAvatarSelection(string name)
		{
			return new SimplePromise<PreferredCosmetics>(new PreferredCosmetics());
		}

		public Promise<PreferredCosmetics> SetCardbackSelection(string name)
		{
			return new SimplePromise<PreferredCosmetics>(new PreferredCosmetics());
		}

		public Promise<PreferredCosmetics> SetEmotesSelection(List<string> emotes)
		{
			return new SimplePromise<PreferredCosmetics>(new PreferredCosmetics());
		}

		public Promise<PreferredCosmetics> SetTitleSelection(string titleId)
		{
			return new SimplePromise<PreferredCosmetics>(new PreferredCosmetics());
		}
	}

	[SerializeField]
	private CosmeticsPerDeckViewModel[] _viewModels;

	[SerializeField]
	private CardRolloverZoomBase _zoomBase;

	private HarnessCardDatabaseLoader _harnessCardDatabaseLoader;

	private EmptyEmoteDataProvider _emoteDataProvider = new EmptyEmoteDataProvider();

	public override void Initialize(Transform root, Transform uiRoot)
	{
		base.Initialize(root, uiRoot);
		_UI.Init(HarnessUICore.CreateOptions(_viewModels), OnValueChanged);
		TooltipTrigger.Inject(Pantry.Get<TooltipSystem>());
	}

	public override void DisplayViewForIndex(int index)
	{
		base.DisplayViewForIndex(index);
		HarnessUICore.SetValueForDropdown(_UI.Dropdown, _UI.DropdownIndex);
	}

	private void OnValueChanged(int newValue)
	{
		_Instance.gameObject.SetActive(value: false);
		CosmeticsPerDeckViewModel viewModel = _viewModels[newValue];
		ShowView(viewModel);
	}

	private void ShowView(CosmeticsPerDeckViewModel viewModel)
	{
		AssetLookupManager assetLookupManager = Pantry.Get<AssetLookupManager>();
		_Instance.gameObject.SetActive(value: true);
		IUnityObjectPool unityObjectPool = Pantry.Get<IUnityObjectPool>();
		IObjectPool genericPool = Pantry.Get<IObjectPool>();
		CardArtTextureLoader artTextureLoader = new CardArtTextureLoader();
		ResourceErrorMessageManager resourceErrorMessageManager = Pantry.Get<ResourceErrorMessageManager>();
		IClientLocProvider clientLocProvider = Pantry.Get<IClientLocProvider>();
		CardDatabase cardDatabase = Pantry.Get<CardDatabase>();
		CardColorCaches cardColorCaches = Pantry.Get<CardColorCaches>();
		IArtCropProvider cardArtCropDatabase = ArtCropDatabaseUtils.LoadBestProvider(NullBILogger.Default);
		CardMaterialBuilder cardMaterialBuilder = new CardMaterialBuilder(assetLookupManager.AssetLookupSystem, artTextureLoader, cardArtCropDatabase, cardColorCaches);
		CardViewBuilder cardViewBuilder = new CardViewBuilder(cardDatabase, unityObjectPool, genericPool, cardMaterialBuilder, assetLookupManager.AssetLookupSystem, clientLocProvider, null, new BILogger(), resourceErrorMessageManager, cardColorCaches);
		CosmeticsProvider cosmeticsProvider = new CosmeticsProvider(new EmptyCosmeticsServiceWrapper());
		CosmeticsClient cosmeticsClient = new CosmeticsClient();
		PetCatalog petCatalog = buildPetCatalog(viewModel.PetEntries, cosmeticsClient);
		cosmeticsClient.Emotes.Add(new CosmeticEmoteEntry
		{
			Id = "Sticker_ZNR_AngryHedron"
		});
		cosmeticsClient.Emotes.Add(new CosmeticEmoteEntry
		{
			Id = "Phrase_ZNR_ServesYou"
		});
		cosmeticsClient.Emotes.Add(new CosmeticEmoteEntry
		{
			Id = "Phrase_ZNR_InfernoSpark"
		});
		_emoteDataProvider.SetEmoteData(ConvertHarnessEmoteData(viewModel.EmoteEntries));
		cosmeticsProvider.SetAvailableCosmetics(cosmeticsClient);
		cosmeticsProvider.SetPlayerOwnedCosmetics(cosmeticsClient);
		petCatalog.CreateSortedPetGroups();
		Client_Deck client_Deck = new Client_Deck();
		Client_DeckSummary client_DeckSummary = new Client_DeckSummary();
		client_DeckSummary.CardBack = viewModel.currentSleeve;
		client_DeckSummary.Avatar = viewModel.currentAvatar;
		client_DeckSummary.Pet = viewModel.currentPet;
		client_DeckSummary.Emotes = viewModel.currentEmotes;
		client_Deck.UpdateWith(client_DeckSummary);
		if (viewModel.useProfileOnly)
		{
			client_Deck = null;
		}
		ClientVanitySelectionsV3 clientVanitySelectionsV = new ClientVanitySelectionsV3();
		clientVanitySelectionsV.avatarSelection = viewModel.accountAvatar;
		string[] array = viewModel.accountPet.Split('.');
		clientVanitySelectionsV.petSelection = new ClientPetSelection
		{
			name = array[0],
			variant = array[1]
		};
		clientVanitySelectionsV.cardBackSelection = viewModel.accountSleeve;
		clientVanitySelectionsV.emoteSelections = viewModel.accountEmotes;
		BILogger logger = new BILogger();
		_Instance.Init(client_Deck, clientVanitySelectionsV, clientLocProvider, cosmeticsProvider, buildAvatarCatalog(viewModel.AvatarEntries), petCatalog, assetLookupManager.AssetLookupSystem, new EmptyDecksManager(), _zoomBase, logger, cardDatabase, cardViewBuilder, ConvertList(viewModel.CardBackSelectorDisplayData), _emoteDataProvider, unityObjectPool);
	}

	private List<EmoteData> ConvertHarnessEmoteData(List<HarnessEmoteData> emoteEntrie)
	{
		List<EmoteData> result = new List<EmoteData>();
		emoteEntrie.ForEach(delegate(HarnessEmoteData x)
		{
			result.Add(x.ToEmoteData());
		});
		return result;
	}

	private List<CardBackSelectorDisplayData> ConvertList(List<HarnessCardBackSelectorDisplayData> cardSleeveData)
	{
		List<CardBackSelectorDisplayData> result = new List<CardBackSelectorDisplayData>();
		cardSleeveData.ForEach(delegate(HarnessCardBackSelectorDisplayData x)
		{
			result.Add(x.ToCardBackSelector());
		});
		return result;
	}

	private PetCatalog buildPetCatalog(List<HarnessPetEntry> petEntries, CosmeticsClient cosmeticsClient)
	{
		PetCatalog petCatalog = new PetCatalog();
		petEntries.ForEach(delegate(HarnessPetEntry x)
		{
			if (x.Id == null && x.Name != null)
			{
				if (x.isOwned)
				{
					cosmeticsClient.Pets.Add(new CosmeticPetEntry
					{
						Id = x.Id,
						Name = x.Name
					});
				}
				x.StoreItem = new StoreItem
				{
					StoreSection = EStoreSection.Pets
				};
				petCatalog.Add(x.Id, x);
			}
		});
		return petCatalog;
	}

	private AvatarCatalog buildAvatarCatalog(List<HarnessAvatarEntry> avatarEntries)
	{
		AvatarCatalog avatarCatalog = new AvatarCatalog();
		foreach (HarnessAvatarEntry avatarEntry in avatarEntries)
		{
			avatarCatalog.Add(avatarEntry.Id, new AvatarEntry
			{
				Id = avatarEntry.Id,
				Source = avatarEntry.Source,
				Type = avatarEntry.Type
			});
		}
		return avatarCatalog;
	}
}
