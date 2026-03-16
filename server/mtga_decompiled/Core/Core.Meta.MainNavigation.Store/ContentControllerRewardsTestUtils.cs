using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Wizards.Models;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Meta.MainNavigation.Store;

public static class ContentControllerRewardsTestUtils
{
	public static void TEST_ShowRewards(ContentControllerRewards ccr, string updateReportJson = "")
	{
		ClientInventoryUpdateReportItem? clientInventoryUpdateReportItem2;
		if (!(updateReportJson != ""))
		{
			ClientInventoryUpdateReportItem clientInventoryUpdateReportItem = new ClientInventoryUpdateReportItem();
			clientInventoryUpdateReportItem.context = new InventoryUpdateContext
			{
				source = InventoryUpdateSource.MercantilePurchase,
				sourceId = "fake"
			};
			clientInventoryUpdateReportItem.xpGained = 0;
			clientInventoryUpdateReportItem.delta = new InventoryDelta
			{
				gemsDelta = 0,
				boosterDelta = Array.Empty<BoosterStack>(),
				cardsAdded = Array.Empty<int>(),
				vanityItemsAdded = new string[3] { "emotes.Phrase_ZNR_ObeyCooperate", "emotes.Phrase_ZNR_IonaNoLonger", "emotes.Phrase_ZNR_RebirthDeath" },
				goldDelta = 500,
				artSkinsAdded = Array.Empty<ArtSkin>(),
				customTokenDelta = new CustomTokenDeltaInfo[2]
				{
					new CustomTokenDeltaInfo
					{
						id = "DraftToken",
						delta = 3
					},
					new CustomTokenDeltaInfo
					{
						id = "SealedToken",
						delta = 3
					}
				}
			};
			clientInventoryUpdateReportItem.aetherizedCards = new List<AetherizedCardInformation>();
			clientInventoryUpdateReportItem2 = clientInventoryUpdateReportItem;
		}
		else
		{
			clientInventoryUpdateReportItem2 = JsonConvert.DeserializeObject<ClientInventoryUpdateReportItem>(updateReportJson);
		}
		ClientInventoryUpdateReportItem t = clientInventoryUpdateReportItem2;
		ccr.AddAndDisplayRewardsCoroutine(t, "TEST REWARDS", "TEST CLAIM");
	}

	public static EventPayoutData TEST_CreateEventPayoutData()
	{
		EventPayoutData obj = new EventPayoutData
		{
			PublicEventName = "Play",
			delta = TEST_CreateInventoryUpdate()
		};
		obj.delta.First().delta.goldDelta = 1000;
		obj.delta.First().delta.gemsDelta = 200;
		return obj;
	}

	public static SeasonPayoutData TEST_CreateSeasonPayoutData()
	{
		SeasonPayoutData seasonPayoutData = new SeasonPayoutData
		{
			oldConstructedOrdinal = 1,
			oldLimitedOrdinal = 2,
			currentSeasonOrdinal = 3,
			constructedReset = new RankProgress
			{
				playerId = "1234",
				seasonOrdinal = 1,
				newClass = RankingClassType.Bronze,
				oldClass = RankingClassType.Bronze,
				newLevel = 1,
				oldLevel = 1,
				oldStep = 1,
				newStep = 0,
				wasLossProtected = false,
				rankUpdateType = "season"
			},
			limitedReset = new RankProgress
			{
				playerId = "1234",
				seasonOrdinal = 1,
				newClass = RankingClassType.Silver,
				oldClass = RankingClassType.Gold,
				newLevel = 1,
				oldLevel = 1,
				oldStep = 1,
				newStep = 0,
				wasLossProtected = false,
				rankUpdateType = "season"
			},
			constructedDelta = TEST_CreateInventoryUpdate(),
			limitedDelta = TEST_CreateInventoryUpdate()
		};
		seasonPayoutData.constructedDelta.First().delta.goldDelta = 50;
		seasonPayoutData.constructedDelta.First().delta.gemsDelta = 50;
		seasonPayoutData.constructedDelta.First().delta.wcRareDelta = 2;
		seasonPayoutData.limitedDelta.First().delta.goldDelta = 50;
		seasonPayoutData.limitedDelta.First().delta.customTokenDelta = new CustomTokenDeltaInfo[1]
		{
			new CustomTokenDeltaInfo
			{
				id = "SealedToken",
				delta = 1
			}
		};
		return seasonPayoutData;
	}

	private static List<ClientInventoryUpdateReportItem> TEST_CreateInventoryUpdate()
	{
		ClientInventoryUpdateReportItem clientInventoryUpdateReportItem = new ClientInventoryUpdateReportItem();
		clientInventoryUpdateReportItem.delta = new InventoryDelta
		{
			boosterDelta = new BoosterStack[1]
			{
				new BoosterStack
				{
					collationId = 100020,
					count = 4
				}
			},
			cardsAdded = new int[4] { 73208, 81079, 77158, 81074 }
		};
		clientInventoryUpdateReportItem.aetherizedCards = new List<AetherizedCardInformation>();
		ClientInventoryUpdateReportItem item = clientInventoryUpdateReportItem;
		return new List<ClientInventoryUpdateReportItem> { item };
	}
}
