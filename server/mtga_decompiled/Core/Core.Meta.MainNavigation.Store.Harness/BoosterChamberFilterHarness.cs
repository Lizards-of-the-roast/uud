using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Code.Collations;
using Core.Code.Collections;
using Core.Code.Harnesses.OfflineHarnessServices;
using Core.Shared.Code.CardFilters;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wizards.Unification.Models.Player;
using Wotc.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Wrapper;
using com.wizards.harness;

namespace Core.Meta.MainNavigation.Store.Harness;

public class BoosterChamberFilterHarness : Harness<BoosterChamberController, HarnessDropdownUI>
{
	[Serializable]
	public class BoosterChamberHarnessViewModel
	{
		public string Description;

		public List<HarnessBoosterInfo> Boosters;

		public List<HarnessCardInfo> Cards;

		public override string ToString()
		{
			return Description;
		}
	}

	[Serializable]
	public class HarnessBoosterInfo
	{
		public int collationId;

		public int count;
	}

	[Serializable]
	public class HarnessCardInfo
	{
		public string SetCode;

		public uint CardGRPId;
	}

	private class MockInventoryServiceWrapper : IInventoryServiceWrapper
	{
		public InventoryInfoShared ExposedInventoryInfoShared;

		public Action OnInventoryUpdated { get; set; }

		public Action OnCardsUpdated { get; set; }

		public Action PublishEvents { get; set; }

		public ClientPlayerInventory Inventory { get; set; }

		public Dictionary<uint, int> Cards { get; set; }

		public Dictionary<uint, int> newCards { get; set; }

		public Dictionary<uint, int> CardsToTagNew { get; set; }

		public List<ClientInventoryUpdateReportItem> Updates { get; }

		public event Action<int> GemsChanged;

		public event Action<int> GoldChanged;

		public event Action<List<ClientBoosterInfo>> BoostersChanged;

		public void OnReconnect()
		{
			throw new NotImplementedException();
		}

		public void OnInventoryInfoUpdated_AWS(InventoryInfoShared obj)
		{
			throw new NotImplementedException();
		}

		public Promise<CardsAndCacheVersion> GetPlayerCards(int playerCardsVersion)
		{
			throw new NotImplementedException();
		}

		public Promise<InventoryInfo> RedeemWildcards(WildcardBulkRequest request)
		{
			throw new NotImplementedException();
		}

		public Promise<string> RedeemVoucher(string voucherId)
		{
			throw new NotImplementedException();
		}

		public Promise<InventoryInfoShared> CrackBooster(string boosterCollationId, int numBoostersToOpen)
		{
			return new SimplePromise<InventoryInfoShared>(ExposedInventoryInfoShared);
		}

		public Promise<bool> CompleteVault()
		{
			throw new NotImplementedException();
		}
	}

	[SerializeField]
	private BoosterChamberHarnessViewModel[] _viewModels;

	private BoosterChamberHarnessViewModel _viewModel;

	public override void Initialize(Transform root, Transform uiRoot)
	{
		base.Initialize(root, uiRoot);
		_UI.Init(HarnessUICore.CreateOptions(_viewModels), OnValueChanged);
	}

	public override void DisplayViewForIndex(int index)
	{
		base.DisplayViewForIndex(index);
		HarnessUICore.SetValueForDropdown(_UI.Dropdown, _UI.DropdownIndex);
	}

	private void OnValueChanged(int newValue)
	{
		_viewModel = _viewModels[newValue];
		Invoke("ShowView", 1f);
	}

	private void ShowView()
	{
		AssetLookupManager assetLookupManager = Pantry.Get<AssetLookupManager>();
		CardDatabase cardDatabase = Pantry.Get<CardDatabase>();
		InventoryManager inventoryManager = new InventoryManager(new MockInventoryServiceWrapper
		{
			Inventory = new ClientPlayerInventory
			{
				boosters = SetBoosters(_viewModel.Boosters)
			}
		}, Pantry.Get<IMercantileServiceWrapper>());
		HarnessInventoryServiceWrapper inventoryServiceWrapper = Pantry.Get<IInventoryServiceWrapper>() as HarnessInventoryServiceWrapper;
		SetRewardCards(_viewModel.Cards, inventoryServiceWrapper);
		BuildSetMetadataProvider(_viewModel.Boosters);
		AudioManager.InitializeAudio(assetLookupManager.AssetLookupSystem);
		_Instance.Instantiate(Pantry.Get<ICardRolloverZoom>(), BuildCardBuilderForHarness(assetLookupManager, cardDatabase), assetLookupManager.AssetLookupSystem, null, new BILogger(), cardDatabase, null, Pantry.Get<ISetMetadataProvider>(), inventoryManager, null, Pantry.Get<IUnityObjectPool>(), new GameObject().AddComponent<CustomButton>(), null);
		_Instance.OnBeginOpen();
	}

	private static void SetRewardCards(List<HarnessCardInfo> cards, HarnessInventoryServiceWrapper inventoryServiceWrapper)
	{
		CrackBoostersCardInfo[] array = new CrackBoostersCardInfo[cards.Count];
		for (int i = 0; i < cards.Count; i++)
		{
			array[i] = new CrackBoostersCardInfo
			{
				grpId = cards[i].CardGRPId,
				set = cards[i].SetCode
			};
		}
		inventoryServiceWrapper.ExposedInventoryInfoShared = new InventoryInfoShared
		{
			cardsOpened = array
		};
	}

	private static ClientSetCollation BuildClientSetCollation(HarnessBoosterInfo booster, ClientSetMetadata clientSetMetadata)
	{
		CollationMapping collationId = (CollationMapping)booster.collationId;
		string setCode = (clientSetMetadata.SetCode = collationId.ToString());
		return new ClientSetCollation
		{
			CollationCode = collationId,
			FlavorId = "",
			SetCode = setCode,
			CardFilterType = CardFilterType.None,
			Set = clientSetMetadata
		};
	}

	private static ISetMetadataProvider BuildSetMetadataProvider(List<HarnessBoosterInfo> boosters)
	{
		ClientSetMetadata clientSetMetadata = new ClientSetMetadata
		{
			Collations = new List<ClientSetCollation>()
		};
		foreach (IGrouping<int, HarnessBoosterInfo> item2 in from _ in boosters
			group _ by _.collationId)
		{
			item2.Deconstruct(out var _, out var grouped);
			ClientSetCollation item = BuildClientSetCollation(grouped.First(), clientSetMetadata);
			clientSetMetadata.Collations.Add(item);
		}
		ISetMetadataProvider setMetadataProvider = Pantry.Get<ISetMetadataProvider>();
		setMetadataProvider.LoadData(new SetMetadataCollection
		{
			SetDatas = new List<ClientSetMetadata> { clientSetMetadata },
			SetGroups = new List<ClientSetGroup>(),
			StoreSetGroups = new List<ClientStorePackGroup>()
		});
		return setMetadataProvider;
	}

	private static List<ClientBoosterInfo> SetBoosters(List<HarnessBoosterInfo> boosters)
	{
		List<ClientBoosterInfo> list = new List<ClientBoosterInfo>(boosters.Count);
		foreach (HarnessBoosterInfo booster in boosters)
		{
			list.Add(new ClientBoosterInfo
			{
				collationId = booster.collationId,
				count = booster.count
			});
		}
		return list;
	}

	private static CardViewBuilder BuildCardBuilderForHarness(AssetLookupManager alm, CardDatabase cardDatabase)
	{
		IUnityObjectPool unityPool = Pantry.Get<IUnityObjectPool>();
		IObjectPool genericPool = Pantry.Get<IObjectPool>();
		CardArtTextureLoader artTextureLoader = new CardArtTextureLoader();
		ResourceErrorMessageManager resourceErrorMessageManager = Pantry.Get<ResourceErrorMessageManager>();
		IClientLocProvider localizationManager = Pantry.Get<IClientLocProvider>();
		CardColorCaches cardColorCaches = Pantry.Get<CardColorCaches>();
		IArtCropProvider cardArtCropDatabase = ArtCropDatabaseUtils.LoadBestProvider(NullBILogger.Default);
		CardMaterialBuilder cardMaterialBuilder = new CardMaterialBuilder(alm.AssetLookupSystem, artTextureLoader, cardArtCropDatabase, cardColorCaches);
		return new CardViewBuilder(cardDatabase, unityPool, genericPool, cardMaterialBuilder, alm.AssetLookupSystem, localizationManager, null, new BILogger(), resourceErrorMessageManager, cardColorCaches);
	}
}
