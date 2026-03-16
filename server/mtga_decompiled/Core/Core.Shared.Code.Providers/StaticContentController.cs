using System.Collections.Generic;
using Core.Code.Promises;
using Core.Meta.Shared;
using Core.Shared.Code.Network;
using MTGA.FrontDoorConnection;
using UnityEngine;
using Wizards.Arena.Models.Achievements;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.Event;
using Wizards.Unification.Models.FrontDoor;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Events;
using Wotc.Mtga.Providers;

namespace Core.Shared.Code.Providers;

public class StaticContentController
{
	private readonly IFrontDoorConnection _fdc;

	public readonly StaticContentCache StaticContentCache;

	public StaticContentController(IFrontDoorConnection fdc)
	{
		_fdc = fdc;
		_fdc.OnMsg_StaticContentNotification += RefreshStaticContentCache;
		StaticContentCache = new StaticContentCache();
	}

	~StaticContentController()
	{
		_fdc.OnMsg_StaticContentNotification -= RefreshStaticContentCache;
	}

	public static Promise<StaticContentResponse> GetStaticContent(IList<EStaticContent> requestedStaticContent, StaticContentController staticContentController, IFrontDoorConnection fdc, StaticContentProviders staticContentProviders)
	{
		return new StaticContentServiceWrapper(fdc).GetStaticContent(requestedStaticContent, staticContentController.StaticContentCache).IfSuccess(delegate(Promise<StaticContentResponse> successfulPromise)
		{
			InitProviders(successfulPromise.Result, staticContentController.StaticContentCache, staticContentProviders);
		});
	}

	private void RefreshStaticContentCache(StaticContentResponse response)
	{
		InitProviders(response, StaticContentCache, default(StaticContentProviders));
	}

	private static void InitProviders(StaticContentResponse response, StaticContentCache cache, StaticContentProviders providers)
	{
		if (response.AvailableCosmetics?.Object != null)
		{
			(providers.CosmeticsProvider ?? Pantry.Get<CosmeticsProvider>()).SetAvailableCosmetics(CosmeticsUtils.FromClient(response.AvailableCosmetics.Value.Object));
			cache.AvailableCosmeticsHash = response.AvailableCosmetics.Value.Hash;
		}
		if (response.SurveyConfigs?.Object != null)
		{
			(providers.SurveyConfigProvider ?? Pantry.Get<ISurveyConfigProvider>()).SetData(response.SurveyConfigs.Value.Object);
			cache.SurveyConfigsHash = response.SurveyConfigs.Value.Hash;
		}
		if (response.CardNicknames?.Object != null)
		{
			(providers.CardNicknamesProvider ?? Pantry.Get<ICardNicknamesProvider>()).SetData(response.CardNicknames.Value.Object);
			cache.CardNicknamesHash = response.CardNicknames.Value.Hash;
		}
		if (response.EmergencyBannedCardTitleIds?.Object != null)
		{
			(providers.EmergencyBansProvider ?? Pantry.Get<IEmergencyCardBansProvider>()).SetData(response.EmergencyBannedCardTitleIds.Value.Object);
			cache.EmergencyCardBansHash = response.EmergencyBannedCardTitleIds.Value.Hash;
		}
		if (response.AchievementsMetadata?.Object != null)
		{
			HashableObject<AchievementsMetadata> achievementsMetadata = response.AchievementsMetadata.Value;
			(providers.AchievementsDataProvider ?? Pantry.Get<IAchievementDataProvider>()).PostAchievementsMetaData(achievementsMetadata.Object);
			cache.AchievementsMetadataHash = achievementsMetadata.Hash;
			CampaignGraphManager graphManager = Pantry.Get<CampaignGraphManager>();
			graphManager.GetDefinitions().AsPromise().ThenOnMainThread(delegate(IReadOnlyDictionary<string, ClientGraphDefinition> graphDefs)
			{
				foreach (string achievementSet in achievementsMetadata.Object.AchievementSets)
				{
					if (graphDefs.TryGetValue(achievementSet, out var value))
					{
						graphManager.Update(value);
					}
				}
			});
		}
		if (response.QueueTips?.Object != null)
		{
			(providers.QueueTipProvider ?? Pantry.Get<IQueueTipProvider>()).SetData(response.QueueTips.Value.Object);
			cache.QueueTipsHash = response.QueueTips.Value.Hash;
		}
	}

	private static StaticContentReq CreateStaticContentReq(IList<EStaticContent> staticContentRequest, StaticContentCache staticContentCache)
	{
		StaticContentReq staticContentReq = new StaticContentReq();
		foreach (EStaticContent item in staticContentRequest)
		{
			switch (item)
			{
			case EStaticContent.AvailableCosmetics:
				staticContentReq.AvailableCosmeticsHash = staticContentCache.AvailableCosmeticsHash;
				break;
			case EStaticContent.SurveyConfigs:
				staticContentReq.SurveyConfigHash = staticContentCache.SurveyConfigsHash;
				break;
			case EStaticContent.CardNicknames:
				staticContentReq.CardNicknamesHash = staticContentCache.CardNicknamesHash;
				break;
			case EStaticContent.EmergencyCardBans:
				staticContentReq.EmergencyCardBansHash = staticContentCache.EmergencyCardBansHash;
				break;
			case EStaticContent.AchievementsMetadata:
				staticContentReq.AchievementsMetadataHash = staticContentCache.AchievementsMetadataHash;
				break;
			default:
				Debug.LogErrorFormat("Static Content Response Has No Matching Request: {0}", item);
				break;
			}
		}
		return staticContentReq;
	}
}
