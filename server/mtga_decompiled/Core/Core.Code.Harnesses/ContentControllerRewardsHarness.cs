using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Code.Input;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Store;
using Core.Meta.UI;
using Core.Shared.Code;
using Core.Shared.Code.ClientModels;
using MTGA.KeyboardManager;
using UnityEngine;
using UnityEngine.Serialization;
using Wizards.Arena.Enums.Store;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;
using Wotc.Mtga.Wrapper;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class ContentControllerRewardsHarness : Harness<ContentControllerRewards, HarnessDropdownUI>
{
	[Serializable]
	public class RewardsViewModel
	{
		public string Description;

		public int GemsToAdd;

		public int GoldToAdd;

		public List<BoosterStackViewModel> Boosters;

		public string[] VanityItems;

		public int[] CardsAdded;

		public int CommonWildcards;

		public int UncommonWildcards;

		public int RareWildcards;

		public int MythicWildcards;

		public List<ArtSkinViewModel> ArtSkins;

		public CustomTokenDeltaInfo[] Tokens;

		public override string ToString()
		{
			return Description;
		}
	}

	[Serializable]
	public class TokenDefinition
	{
		public string TokenId;

		public string PrefabName;

		public string ExpirationDate;

		public string LocKey;

		public string DescriptionLocKey;
	}

	[Serializable]
	public class BoosterStackViewModel
	{
		public CollationMapping CollationId;

		public int Count;
	}

	[Serializable]
	public class ArtSkinViewModel
	{
		public uint artId;

		public string ccv;
	}

	[Serializable]
	public class SeasonPayoutViewModel
	{
		public string Description;

		public int currentSeasonOrdinal;

		public int oldConstructedOrdinal;

		public int oldLimitedOrdinal;

		public RankProgress constructedReset;

		public RankProgress limitedReset;

		public RewardsViewModel constructedDelta;

		public RewardsViewModel limitedDelta;

		public InventoryInfoShared inventoryInfoWithNoDeltas;

		public override string ToString()
		{
			return Description;
		}
	}

	[FormerlySerializedAs("_viewModels")]
	[SerializeField]
	private RewardsViewModel[] _rewardsViewModels;

	[SerializeField]
	private SeasonPayoutViewModel[] _seasonPayoutViewModels;

	[SerializeField]
	private List<TokenDefinition> _tokenDefinitions;

	private GlobalCoroutineExecutor _coroutineExecutor;

	private List<object> _allViewModels;

	public override void Initialize(Transform root, Transform uiRoot)
	{
		base.Initialize(root, uiRoot);
		Pantry.Get<ICustomTokenProvider>().TokenDefinitions = _tokenDefinitions.Select((TokenDefinition x) => ToTokenDefinition(x)).ToDictionary((Client_CustomTokenDefinition x) => x.TokenId);
		Pantry.Get<IInventoryServiceWrapper>().Inventory = new ClientPlayerInventory
		{
			CustomTokens = _tokenDefinitions.ToDictionary((TokenDefinition x) => x.TokenId, (TokenDefinition y) => 0)
		};
		_coroutineExecutor = Pantry.Get<GlobalCoroutineExecutor>();
		_allViewModels = _rewardsViewModels.Cast<object>().Concat(_seasonPayoutViewModels).ToList();
		_UI.Init(HarnessUICore.CreateOptions(_allViewModels), OnValueChanged);
		TooltipTrigger.Inject(Pantry.Get<TooltipSystem>());
	}

	public override void DisplayViewForIndex(int index)
	{
		base.DisplayViewForIndex(index);
		_Instance.Init(Pantry.Get<SetMasteryDataProvider>(), Pantry.Get<AssetLookupManager>().AssetLookupSystem, Pantry.Get<ICardRolloverZoom>(), Pantry.Get<KeyboardManager>(), Pantry.Get<IActionSystem>(), Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>());
		SceneUITransforms sceneUITransforms = Pantry.Get<SceneUITransforms>();
		_Instance.transform.SetParent(sceneUITransforms.PopupsParent, worldPositionStays: false);
		HarnessUICore.SetValueForDropdown(_UI.Dropdown, _UI.DropdownIndex);
	}

	private void OnValueChanged(int newValue)
	{
		object obj = _allViewModels[newValue];
		if (obj is RewardsViewModel viewModel)
		{
			DisplayRewards(_Instance, viewModel);
		}
		else if (obj is SeasonPayoutViewModel viewModel2)
		{
			DisplaySeasonPayout(_Instance, viewModel2);
		}
	}

	private static void DisplayRewards(ContentControllerRewards view, RewardsViewModel viewModel)
	{
		ClientInventoryUpdateReportItem t = ToInventoryUpdateFromRewardsViewModel(viewModel);
		IClientLocProvider clientLocProvider = Pantry.Get<IClientLocProvider>();
		view.AddAndDisplayRewardsCoroutine(t, clientLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title_StoreAcquired"), clientLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimRedeemedButton"));
	}

	private static void DisplaySeasonPayout(ContentControllerRewards view, SeasonPayoutViewModel viewModel)
	{
		SeasonPayoutData endSeasonData = ToSeasonPayoutDataFromViewModel(viewModel);
		view.DisplayEndOfSeasonCoroutine(endSeasonData);
	}

	private static SeasonPayoutData ToSeasonPayoutDataFromViewModel(SeasonPayoutViewModel viewModel)
	{
		List<ClientInventoryUpdateReportItem> list = new List<ClientInventoryUpdateReportItem>();
		if (viewModel.limitedDelta != null)
		{
			list.Add(ToInventoryUpdateFromRewardsViewModel(viewModel.limitedDelta));
		}
		return new SeasonPayoutData
		{
			currentSeasonOrdinal = viewModel.currentSeasonOrdinal,
			oldConstructedOrdinal = viewModel.oldConstructedOrdinal,
			oldLimitedOrdinal = viewModel.oldLimitedOrdinal,
			constructedReset = viewModel.constructedReset,
			limitedReset = viewModel.limitedReset,
			constructedDelta = new List<ClientInventoryUpdateReportItem> { ToInventoryUpdateFromRewardsViewModel(viewModel.constructedDelta) },
			limitedDelta = list,
			inventoryInfoWithNoDeltas = null
		};
	}

	private static ClientInventoryUpdateReportItem ToInventoryUpdateFromRewardsViewModel(RewardsViewModel viewModel)
	{
		ClientInventoryUpdateReportItem clientInventoryUpdateReportItem = new ClientInventoryUpdateReportItem();
		clientInventoryUpdateReportItem.delta = new InventoryDelta
		{
			gemsDelta = viewModel.GemsToAdd,
			goldDelta = viewModel.GoldToAdd,
			boosterDelta = ToBoosterStack(viewModel.Boosters),
			cardsAdded = viewModel.CardsAdded,
			decksAdded = new Guid[0],
			vanityItemsAdded = viewModel.VanityItems,
			vanityItemsRemoved = new string[0],
			vaultProgressDelta = default(decimal),
			wcTrackPosition = 0,
			wcCommonDelta = viewModel.CommonWildcards,
			wcUncommonDelta = viewModel.UncommonWildcards,
			wcRareDelta = viewModel.RareWildcards,
			wcMythicDelta = viewModel.MythicWildcards,
			artSkinsAdded = ToArtSkins(viewModel.ArtSkins),
			artSkinsRemoved = new ArtSkin[0],
			voucherItemsDelta = new VoucherStack[0],
			tickets = new TicketStack[0],
			customTokenDelta = viewModel.Tokens
		};
		clientInventoryUpdateReportItem.aetherizedCards = new List<AetherizedCardInformation>();
		clientInventoryUpdateReportItem.xpGained = 0;
		clientInventoryUpdateReportItem.context = new InventoryUpdateContext
		{
			source = InventoryUpdateSource.MercantilePurchase,
			sourceId = "fake"
		};
		clientInventoryUpdateReportItem.parentcontext = null;
		return clientInventoryUpdateReportItem;
	}

	private static BoosterStack[] ToBoosterStack(List<BoosterStackViewModel> viewModelBoosters)
	{
		List<BoosterStack> list = new List<BoosterStack>();
		foreach (BoosterStackViewModel viewModelBooster in viewModelBoosters)
		{
			list.Add(new BoosterStack
			{
				collationId = (int)viewModelBooster.CollationId,
				count = viewModelBooster.Count
			});
		}
		return list.ToArray();
	}

	private static ArtSkin[] ToArtSkins(List<ArtSkinViewModel> viewModels)
	{
		return viewModels.Select((ArtSkinViewModel viewModel) => new ArtSkin
		{
			artId = viewModel.artId,
			ccv = viewModel.ccv
		}).ToArray();
	}

	private static Client_CustomTokenDefinition ToTokenDefinition(TokenDefinition tokenDefinition)
	{
		DateTime expirationDate = DateTime.MaxValue;
		if (!string.IsNullOrEmpty(tokenDefinition.ExpirationDate))
		{
			expirationDate = DateTime.Parse(tokenDefinition.ExpirationDate);
		}
		return new Client_CustomTokenDefinition
		{
			TokenId = tokenDefinition.TokenId,
			ExpirationDate = expirationDate,
			TokenType = ClientTokenType.Event,
			ThumbnailImageName = null,
			PrefabName = tokenDefinition.PrefabName,
			HeaderLocKey = tokenDefinition.LocKey,
			DescriptionLocKey = tokenDefinition.DescriptionLocKey,
			DisplayPriority = 0
		};
	}
}
