using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads;
using AssetLookupTree.Payloads.Avatar;
using UnityEngine;
using Wizards.Arena.Enums.Cosmetic;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Loc;

public static class ProfileUtilities
{
	private const string AVATAR_LOC_PREFIX = "MainNav/Profile/Avatars/";

	private const string AVATAR_ID_PREFIX = "Avatar_Basic_";

	public static string GetRandomAvatarId()
	{
		string text = "Avatar_Basic_AjaniGoldmane";
		if (WrapperController.Instance?.Store.AvatarCatalog == null)
		{
			Debug.LogWarning("Request for random avatar failed. Avatar Catalog is not yet available.");
			return text;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AvatarEntry> item in WrapperController.Instance.Store.AvatarCatalog)
		{
			if (item.Value.Source.HasFlag(AcquisitionFlags.DefaultLoginGrant))
			{
				list.Add(item.Key);
			}
		}
		if (list.Count == 0)
		{
			Debug.LogError("No Default Avatars specified.  Using " + text);
			return text;
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		return list[index];
	}

	public static string GetAvatarBustImagePath(AssetLookupSystem assetLookupSystem, string avatarId)
	{
		return PathForAvatarId<BustPayload>(assetLookupSystem, avatarId);
	}

	public static string GetAvatarThumbImagePath(AssetLookupSystem assetLookupSystem, string avatarId)
	{
		return PathForAvatarId<ThumbnailPayload>(assetLookupSystem, avatarId);
	}

	public static string GetAvatarFullImagePath(AssetLookupSystem assetLookupSystem, string avatarId)
	{
		return PathForAvatarId<FullBodyPayload>(assetLookupSystem, avatarId);
	}

	public static string GetAvatarStoreImagePath(AssetLookupSystem assetLookupSystem, string avatarId)
	{
		string text = PathForAvatarId<StorePayload>(assetLookupSystem, avatarId);
		if (string.IsNullOrEmpty(text))
		{
			SimpleLog.LogError("[Store] Missing store avatar for " + avatarId);
		}
		return text;
	}

	public static string GetAvatarLocKey(string avatarId)
	{
		return "MainNav/Profile/Avatars/" + avatarId + "_Name";
	}

	public static string GetAvatarBio(string avatarId)
	{
		return "MainNav/Profile/Avatars/" + avatarId + "_Bio";
	}

	public static AudioEvent GetAvatarVO(AssetLookupSystem assetLookupSystem, string avatarId)
	{
		BlackboardForAvatarId(assetLookupSystem, avatarId);
		VoiceSFX payload = GetPayload<VoiceSFX>(assetLookupSystem);
		if (payload != null && payload.SfxData != null && payload.SfxData.AudioEvents.Count > 0)
		{
			int count = payload.SfxData.AudioEvents.Count;
			int index = UnityEngine.Random.Range(0, count);
			return payload.SfxData.AudioEvents[index];
		}
		return null;
	}

	private static string PathForAvatarId<T>(AssetLookupSystem assetLookupSystem, string avatarId) where T : SpritePayload
	{
		BlackboardForAvatarId(assetLookupSystem, avatarId);
		return PathForSpritePayload(GetPayload<T>(assetLookupSystem));
	}

	private static T GetPayload<T>(AssetLookupSystem assetLookupSystem) where T : class, IPayload
	{
		T payload = assetLookupSystem.TreeLoader.LoadTree<T>().GetPayload(assetLookupSystem.Blackboard);
		assetLookupSystem.Blackboard.Clear();
		return payload;
	}

	private static void BlackboardForAvatarId(AssetLookupSystem assetLookupSystem, string avatarId)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.CosmeticAvatarId = avatarId;
	}

	private static string PathForSpritePayload(SpritePayload payload)
	{
		return payload?.Reference.RelativePath;
	}

	public static string GetMasteryEndingWarningMessage(TimeSpan timeLeft, IClientLocProvider _localization)
	{
		if (timeLeft.TotalDays > 2.0)
		{
			return _localization.GetLocalizedText("MainNav/BattlePass/MasteryEnds_Days", ("days", timeLeft.Days.ToString()));
		}
		if (timeLeft.TotalHours > 8.0)
		{
			return _localization.GetLocalizedText("MainNav/BattlePass/MasteryEnds_Hours", ("hours", ((int)timeLeft.TotalHours).ToString()));
		}
		if ((int)timeLeft.TotalHours == 1)
		{
			if (timeLeft.Minutes != 1)
			{
				return _localization.GetLocalizedText("MainNav/BattlePass/MasteryEnds_HourAndMinutes", ("minutes", timeLeft.Minutes.ToString()));
			}
			return _localization.GetLocalizedText("MainNav/BattlePass/MasteryEnds_HourAndMinute");
		}
		if (timeLeft.TotalHours > 1.0)
		{
			if (timeLeft.Minutes != 1)
			{
				return _localization.GetLocalizedText("MainNav/BattlePass/MasteryEnds_HoursAndMinutes", ("hours", timeLeft.Hours.ToString()), ("minutes", timeLeft.Minutes.ToString()));
			}
			return _localization.GetLocalizedText("MainNav/BattlePass/MasteryEnds_HoursAndMinute", ("hours", timeLeft.Hours.ToString()));
		}
		if (timeLeft.Minutes > 1)
		{
			return _localization.GetLocalizedText("MainNav/BattlePass/MasteryEnds_Minutes", ("minutes", timeLeft.Minutes.ToString()));
		}
		if (timeLeft.Minutes == 1)
		{
			return _localization.GetLocalizedText("MainNav/BattlePass/MasteryEnds_Minute");
		}
		return _localization.GetLocalizedText("MainNav/BattlePass/MasteryEnds_LessThanMinute");
	}
}
