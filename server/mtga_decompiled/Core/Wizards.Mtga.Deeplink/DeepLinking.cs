using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using ProfileUI;
using UnityEngine;
using Wizards.MDN;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga.Assets;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.Deeplink;

public class DeepLinking
{
	public const string DEEPLINK_SCHEME = "unitydl://";

	public static string DEEPLINK_HOST = "play.mtgarena.com";

	public static string ADJUST_ULINK_HOST = "pex8.adj.st";

	private static string DATE_TIME_FORMAT = "yyyyMMddTHH:mm:ssZ";

	public static string DeferredUrl = null;

	public static NameValueCollection AttributionData = null;

	private static IBILogger _biLogger = null;

	public static void SetDeferredURL(string url)
	{
		if (!string.IsNullOrEmpty(url))
		{
			DeferredUrl = url;
			if (WrapperController.Instance != null)
			{
				TryNavigateViaUrl(DeferredUrl, WrapperController.Instance, _biLogger);
				DeferredUrl = null;
			}
		}
	}

	public static void SetDeferredAttributionData(NameValueCollection valueCollection)
	{
		if (valueCollection != null)
		{
			AttributionData = valueCollection;
		}
	}

	public static void LogBIQuerryParams(NameValueCollection valueCollection)
	{
	}

	public static NameValueCollection DetermineAttributionDataToUse(NameValueCollection deeplinkParsedData)
	{
		NameValueCollection nameValueCollection = AttributionData;
		if (nameValueCollection == null)
		{
			nameValueCollection = deeplinkParsedData;
		}
		if (deeplinkParsedData.Get("adj_campaign") != null || deeplinkParsedData.Get("adj_adGroup") != null)
		{
			nameValueCollection = deeplinkParsedData;
		}
		return nameValueCollection;
	}

	public static void SaveCurrentDeeplink()
	{
		string text = DateTime.Now.ToString(DATE_TIME_FORMAT);
		if (!string.IsNullOrEmpty(Application.absoluteURL))
		{
			MDNPlayerPrefs.DeeplinkURLForNextSession = text + " " + Application.absoluteURL;
		}
		else if (!string.IsNullOrEmpty(DeferredUrl))
		{
			MDNPlayerPrefs.DeeplinkURLForNextSession = text + " " + DeferredUrl;
		}
	}

	private static bool TryGetSavedDeeplink(ref string url)
	{
		string deeplinkURLForNextSession = MDNPlayerPrefs.DeeplinkURLForNextSession;
		MDNPlayerPrefs.DeeplinkURLForNextSession = "";
		if (!string.IsNullOrEmpty(deeplinkURLForNextSession))
		{
			string[] array = deeplinkURLForNextSession.Split(' ');
			DateTime dateTime = DateTime.ParseExact(array[0], DATE_TIME_FORMAT, CultureInfo.InvariantCulture);
			if (DateTime.Now - dateTime < new TimeSpan(24, 0, 0))
			{
				url = array[1];
				return true;
			}
		}
		return false;
	}

	public static bool NavigateViaDeepLink(WrapperController wrapper, IBILogger bILogger)
	{
		string url = null;
		if (!string.IsNullOrEmpty(Application.absoluteURL) | !TryGetSavedDeeplink(ref url))
		{
			url = Application.absoluteURL;
		}
		if (!string.IsNullOrEmpty(DeferredUrl))
		{
			url = DeferredUrl;
			DeferredUrl = null;
		}
		_biLogger = bILogger;
		return TryNavigateViaUrl(url, wrapper, bILogger);
	}

	public static bool TryNavigateViaUrl(string url, WrapperController wrapper, IBILogger bILogger)
	{
		if (!Uri.TryCreate(url, UriKind.Absolute, out var result))
		{
			if (!string.IsNullOrEmpty(url))
			{
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_CouldNotTravel"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_PageNotAccessible"));
				bILogger?.Send(ClientBusinessEventType.DeepLinkError, new DeepLinkError
				{
					EventTime = DateTime.UtcNow,
					targetURL = url,
					error = "URL string is invalid/cannot be parsed"
				});
			}
			return false;
		}
		if (result.Host != DEEPLINK_HOST && result.Host != ADJUST_ULINK_HOST)
		{
			Uri.TryCreate(result.Scheme + "://" + DEEPLINK_HOST + "/" + result.Host + result.PathAndQuery, UriKind.Absolute, out result);
		}
		string errorMessage = "";
		string targetURLTopLevel = ((result.Segments.Length > 1) ? result.Segments[1] : "home");
		NameValueCollection nameValueCollection = DetermineAttributionDataToUse(HttpUtility.ParseQueryString(result.Query));
		if (!TryNavigateProcessedUri(result, wrapper, bILogger, out errorMessage))
		{
			bILogger?.Send(ClientBusinessEventType.DeepLinkError, new DeepLinkError
			{
				EventTime = DateTime.UtcNow,
				targetURL = url,
				targetURLTopLevel = targetURLTopLevel,
				targetURLPath = result.AbsolutePath,
				targetURLQueries = result.Query,
				campaign = nameValueCollection.Get("adj_campaign"),
				adGroup = nameValueCollection.Get("adj_adGroup"),
				creative = nameValueCollection.Get("adj_creative"),
				label = nameValueCollection.Get("adj_label"),
				source = nameValueCollection.Get("adj_source"),
				error = errorMessage
			});
			LogBIQuerryParams(nameValueCollection);
			return false;
		}
		bILogger?.Send(ClientBusinessEventType.DeepLinkSuccess, new DeepLinkSuccess
		{
			EventTime = DateTime.UtcNow,
			targetURL = url,
			targetURLTopLevel = targetURLTopLevel,
			targetURLPath = result.AbsolutePath,
			targetURLQueries = result.Query,
			campaign = nameValueCollection.Get("adj_campaign"),
			adGroup = nameValueCollection.Get("adj_adGroup"),
			creative = nameValueCollection.Get("adj_creative"),
			label = nameValueCollection.Get("adj_label"),
			source = nameValueCollection.Get("adj_source")
		});
		LogBIQuerryParams(nameValueCollection);
		return true;
	}

	private static bool TryNavigateProcessedUri(Uri targetUri, WrapperController wrapper, IBILogger biLogger, out string errorMessage)
	{
		errorMessage = "";
		string text = ((targetUri.Segments.Length > 1) ? targetUri.Segments[1] : "home");
		NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(targetUri.Query);
		switch (text)
		{
		case "profile":
		case "profile/":
			return TryNavigateProfileScreen(new ReadOnlySpan<string>(targetUri.Segments, 1, targetUri.Segments.Length - 1), wrapper, out errorMessage);
		case "store":
		case "store/":
			if (!string.IsNullOrEmpty(targetUri.Query) && TryNavigateStoreItem(new ReadOnlySpan<string>(targetUri.Segments, 1, targetUri.Segments.Length - 1), nameValueCollection, wrapper))
			{
				return true;
			}
			return TryNavigateStore(new ReadOnlySpan<string>(targetUri.Segments, 1, targetUri.Segments.Length - 1), wrapper, out errorMessage);
		case "event":
		case "event/":
			return TryNavigateEvent(new ReadOnlySpan<string>(targetUri.Segments, 1, targetUri.Segments.Length - 1), wrapper, out errorMessage);
		case "packs":
		case "packs/":
		case "boosters":
		case "boosters/":
			return TryNavigatePacks(wrapper, out errorMessage);
		case "color_challenge":
		case "color_challenge/":
			wrapper.SceneLoader.GoToEventScreen(wrapper.EventManager.ColorMasteryEventContext);
			break;
		case "decks":
		case "decks/":
			wrapper.SceneLoader.GoToDeckManager();
			break;
		case "mastery":
		case "mastery/":
			wrapper.SceneLoader.GoToProgressionTrackScene(new ProgressionTrackPageContext(null, NavContentType.None, wrapper.SceneLoader.CurrentContentType), "Deep Link", forceReload: false, alwaysInit: true);
			break;
		case "learn_to_play":
		case "learn_to_play/":
			wrapper.SceneLoader.GoToLearnToPlay("Deep Link");
			break;
		case "home":
		case "home/":
			wrapper.SceneLoader.GoToLanding(new HomePageContext());
			break;
		case "cinematic":
		case "cinematic/":
		{
			string videoUrl = ((nameValueCollection.Count > 0) ? nameValueCollection.Get("videoUrl") : "");
			string videoPlayLookupMode = ((nameValueCollection.Count > 0) ? nameValueCollection.Get("videoPlayLookupMode") : "");
			string videoPlayAudioMode = ((nameValueCollection.Count > 0) ? nameValueCollection.Get("videoPlayAudioMode") : "");
			return TryNavigateCinematic(new ReadOnlySpan<string>(targetUri.Segments, 1, targetUri.Segments.Length - 1), wrapper, videoUrl, videoPlayLookupMode, videoPlayAudioMode, out errorMessage);
		}
		default:
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_CouldNotTravel"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_PageNotAccessible"));
			errorMessage = "Invalid Top Level '" + text + "'";
			return false;
		}
		return true;
	}

	public static void LogDeepLinkNotUsed(string url, string context, IBILogger bILogger)
	{
		if (!Uri.TryCreate(url, UriKind.Absolute, out var result))
		{
			bILogger?.Send(ClientBusinessEventType.DeepLinkError, new DeepLinkError
			{
				EventTime = DateTime.UtcNow,
				targetURL = url,
				error = context + " + URL string is invalid/cannot be parsed"
			});
			return;
		}
		NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(result.Query);
		bILogger?.Send(ClientBusinessEventType.DeepLinkNotUsed, new DeepLinkNotUsed
		{
			EventTime = DateTime.UtcNow,
			targetURL = url,
			targetURLTopLevel = ((result.Segments.Length > 1) ? result.Segments[1] : ""),
			targetURLPath = result.AbsolutePath,
			targetURLQueries = result.Query,
			campaign = nameValueCollection.Get("adj_campaign"),
			adGroup = nameValueCollection.Get("adj_adGroup"),
			creative = nameValueCollection.Get("adj_creative"),
			label = nameValueCollection.Get("adj_label"),
			source = nameValueCollection.Get("adj_source"),
			reason = context
		});
		LogBIQuerryParams(nameValueCollection);
	}

	public static bool TryNavigateProfileScreen(ReadOnlySpan<string> subpath, WrapperController wrapper, out string errorMessage)
	{
		if (wrapper.FrontDoorConnectionServiceWrapper.Killswitch.IsProfileSceneDisabled)
		{
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_CouldNotTravel"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_PageNotAccessible"));
			errorMessage = "Profile Scene Killswitch Enabled";
			return false;
		}
		if (subpath.Length > 1)
		{
			ProfileScreenModeEnum screenMode = subpath[1] switch
			{
				"season_rewards" => ProfileScreenModeEnum.SeasonRewards, 
				"season_rewards/" => ProfileScreenModeEnum.SeasonRewards, 
				"constructed" => ProfileScreenModeEnum.SeasonRewards, 
				"constructed/" => ProfileScreenModeEnum.SeasonRewards, 
				"limited" => ProfileScreenModeEnum.SeasonRewards, 
				"limited/" => ProfileScreenModeEnum.SeasonRewards, 
				"pet_select" => ProfileScreenModeEnum.PetSelect, 
				"pet_select/" => ProfileScreenModeEnum.PetSelect, 
				"emote_select" => ProfileScreenModeEnum.EmoteSelect, 
				"emote_select/" => ProfileScreenModeEnum.EmoteSelect, 
				"card_back_select" => ProfileScreenModeEnum.CardBackSelect, 
				"card_back_select/" => ProfileScreenModeEnum.CardBackSelect, 
				"avatar_select" => ProfileScreenModeEnum.AvatarSelect, 
				"avatar_select/" => ProfileScreenModeEnum.AvatarSelect, 
				_ => ProfileScreenModeEnum.ProfileDetails, 
			};
			string text = subpath[1];
			RankType rankType = ((text == "limited" || text == "limited/") ? RankType.Limited : RankType.Constructed);
			RankType rankType2 = rankType;
			wrapper.SceneLoader.GoToProfileScreen(SceneChangeInitiator.System, "Deep Link", screenMode, rankType2, forceReload: false, alwaysInit: true);
		}
		else
		{
			wrapper.SceneLoader.GoToProfileScreen(SceneChangeInitiator.System, "Deep Link", ProfileScreenModeEnum.Unknown, RankType.Unknown, forceReload: false, alwaysInit: true);
		}
		errorMessage = "";
		return true;
	}

	public static bool TryNavigateStore(ReadOnlySpan<string> subpath, WrapperController wrapper, out string errorMessage)
	{
		if (wrapper.FrontDoorConnectionServiceWrapper.Killswitch.IsStoreDisabled)
		{
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_CouldNotTravel"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_PageNotAccessible"));
			errorMessage = "Store Killswitch Enabled";
			return false;
		}
		StoreTabType storeTabType = ParseStoreTabType(subpath);
		if (storeTabType != StoreTabType.None)
		{
			wrapper.SceneLoader.GoToStore(storeTabType, "Deep Link", forceReload: false, alwaysInit: true);
		}
		else
		{
			wrapper.SceneLoader.GoToStore(StoreTabType.Featured, "Deep Link", forceReload: false, alwaysInit: true);
		}
		errorMessage = "";
		return true;
	}

	public static bool TryNavigateStoreItem(ReadOnlySpan<string> subpath, NameValueCollection queryDictionary, WrapperController wrapper)
	{
		string text = queryDictionary["item_id"];
		StoreTabType fallbackContext = ParseStoreTabType(subpath);
		if (!string.IsNullOrEmpty(text))
		{
			wrapper.SceneLoader.GoToStoreItem(text, fallbackContext, "Deep Link", forceReload: false, alwaysInit: true);
			return true;
		}
		return false;
	}

	public static bool TryNavigateEvent(ReadOnlySpan<string> subpath, WrapperController wrapper, out string errorMessage)
	{
		if (subpath.Length > 1)
		{
			string eventName = subpath[1];
			EventContext eventContext = EventContextForPath(wrapper.EventManager.EventContexts, eventName);
			if (eventContext != null)
			{
				wrapper.SceneLoader.GoToEventScreen(eventContext, reloadIfAlreadyLoaded: false, SceneLoader.NavMethod.Deeplink);
				errorMessage = "";
				return true;
			}
			errorMessage = "Event Subpath " + subpath[1] + " cannot be resolved";
		}
		else
		{
			errorMessage = "Event uri does not have subpath/event name";
		}
		SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_CouldNotTravel"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/EventPage/EventNotActive"));
		return false;
	}

	public static bool TryNavigateCinematic(ReadOnlySpan<string> subpath, WrapperController wrapper, string videoUrl, string videoPlayLookupMode, string videoPlayAudioMode, out string errorMessage)
	{
		if (subpath.Length > 1)
		{
			string text = subpath[1];
			if (Scenes.IsSceneAvailable(text))
			{
				wrapper.SceneLoader.GoToCinematic(text, videoUrl, videoPlayLookupMode, videoPlayAudioMode);
				errorMessage = "";
				return true;
			}
			errorMessage = "Cinematic name " + text + " is not available";
		}
		else
		{
			errorMessage = "Cinematic uri does not have subpath/event name";
		}
		SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_CouldNotTravel"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/EventPage/EventNotActive"));
		return false;
	}

	public static EventContext EventContextForPath(List<EventContext> eventContexts, string eventName)
	{
		return eventContexts.Find((EventContext c) => ContextMatchesName(c, eventName));
	}

	public static bool ContextMatchesName(EventContext context, string eventName)
	{
		string text = context.PlayerEvent.EventUXInfo.PublicEventName?.ToLower();
		string text2 = context.PlayerEvent.EventInfo.InternalEventName?.ToLower();
		eventName = eventName.ToLower();
		if (!(eventName == text))
		{
			return eventName == text2;
		}
		return true;
	}

	public static bool TryNavigatePacks(WrapperController wrapper, out string errorMessage)
	{
		if (wrapper.FrontDoorConnectionServiceWrapper.Killswitch.IsBoosterDisabled)
		{
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_CouldNotTravel"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Deeplink_PageNotAccessible"));
			errorMessage = "Booster Chamber killswitch enabled";
			return false;
		}
		wrapper.SceneLoader.GoToBoosterChamber("Deep Link");
		errorMessage = "";
		return true;
	}

	private static StoreTabType ParseStoreTabType(ReadOnlySpan<string> subpath)
	{
		StoreTabType result = StoreTabType.None;
		if (subpath.Length > 1)
		{
			result = subpath[1] switch
			{
				"avatars" => StoreTabType.Cosmetics, 
				"avatars/" => StoreTabType.Cosmetics, 
				"bundles" => StoreTabType.Bundles, 
				"bundles/" => StoreTabType.Bundles, 
				"card_styles" => StoreTabType.Cosmetics, 
				"card_styles/" => StoreTabType.Cosmetics, 
				"featured" => StoreTabType.Featured, 
				"featured/" => StoreTabType.Featured, 
				"gems" => StoreTabType.Gems, 
				"gems/" => StoreTabType.Gems, 
				"packs" => StoreTabType.Packs, 
				"packs/" => StoreTabType.Packs, 
				"pets" => StoreTabType.Cosmetics, 
				"pets/" => StoreTabType.Cosmetics, 
				"daily_deals" => StoreTabType.DailyDeals, 
				"daily_deals/" => StoreTabType.DailyDeals, 
				"sales" => StoreTabType.DailyDeals, 
				"sales/" => StoreTabType.DailyDeals, 
				"sleeves" => StoreTabType.Cosmetics, 
				"sleeves/" => StoreTabType.Cosmetics, 
				"decks" => StoreTabType.Decks, 
				"decks/" => StoreTabType.Decks, 
				"prizewall" => StoreTabType.PrizeWall, 
				"prizewall/" => StoreTabType.PrizeWall, 
				"prize_wall" => StoreTabType.PrizeWall, 
				"prize_wall/" => StoreTabType.PrizeWall, 
				_ => StoreTabType.None, 
			};
		}
		return result;
	}
}
