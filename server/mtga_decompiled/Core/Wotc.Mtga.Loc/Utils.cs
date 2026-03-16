using System.Collections.Generic;
using GreClient.Rules;
using Wizards.Arena.Enums.System;
using Wizards.Arena.Promises;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Loc;

public static class Utils
{
	public const string ValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";

	public const string PreconKeyPrefix = "?=?Loc/";

	public static string ValidateBakeName(string text)
	{
		char[] array = text.ToCharArray();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_".IndexOf(array[i]) < 0)
			{
				array[i] = '_';
			}
		}
		string text2 = new string(array);
		if (!string.IsNullOrEmpty(text2) && char.IsDigit(text2[0]))
		{
			text2 = "_" + text2;
		}
		return text2;
	}

	public static string GetLocalizedDeckName(string name)
	{
		return GetLocalizedDeckName(name, Languages.ActiveLocProvider);
	}

	public static string GetLocalizedDeckName(string name, IClientLocProvider localizationManager)
	{
		if (name == null)
		{
			return "";
		}
		if (name.StartsWith("?=?Loc/"))
		{
			string key = name.Substring(7);
			string localizedText = localizationManager.GetLocalizedText(key);
			localizedText.Contains("Decks/Precon/Precon");
			return localizedText;
		}
		return name;
	}

	public static string ParameterizeInPlace(string translation, Dictionary<string, string> parameters)
	{
		if (translation != null)
		{
			foreach (string key in parameters.Keys)
			{
				translation = translation.Replace($"{{{key}}}", parameters[key]);
			}
			return translation;
		}
		return string.Empty;
	}

	public static void GetDeckSubmissionErrorMessages(Error error, out string errTitle, out string errText)
	{
		GetDeckSubmissionErrorMessages((ServerErrors)error.Code, out errTitle, out errText);
	}

	public static void GetDeckSubmissionErrorMessages(ServerErrors error, out string errTitle, out string errText)
	{
		if (error == ServerErrors.Deck_FormatOrOwnershipValidationError)
		{
			errTitle = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_InvalidDeck");
			errText = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Browsers/Submit_Deck_Invalid");
		}
		else
		{
			errTitle = Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title");
			errText = Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Submit_Error_Text");
		}
	}

	public static (string errTitle, string errText) GetChallengeErrorMessages(EDirectChallengeMismatch[] reasons)
	{
		IClientLocProvider activeLocProvider = Languages.ActiveLocProvider;
		string localizedText = activeLocProvider.GetLocalizedText("MainNav/PrivateGame/Error_MismatchedConfig_Title");
		string text = activeLocProvider.GetLocalizedText("MainNav/PrivateGame/Error_MismatchedConfig_Summary");
		for (int i = 0; i < reasons.Length; i++)
		{
			EDirectChallengeMismatch eDirectChallengeMismatch = reasons[i];
			switch (eDirectChallengeMismatch)
			{
			case EDirectChallengeMismatch.BestOf3Mismatch:
				text = text + "\n" + activeLocProvider.GetLocalizedText("MainNav/PrivateGame/Error_MismatchedConfig_Series");
				break;
			case EDirectChallengeMismatch.PlayFirstMismatch:
				text = text + "\n" + activeLocProvider.GetLocalizedText("MainNav/PrivateGame/Error_MistmatchedConfig_PlayFirstSelection");
				break;
			case EDirectChallengeMismatch.ModeMismatch:
				text = text + "\n" + activeLocProvider.GetLocalizedText("MainNav/PrivateGame/Error_MismatchedConfig_TournamentMode");
				break;
			default:
				text = text + "\n" + activeLocProvider.GetLocalizedText("MainNav/PrivateGame/Error_MismatchedConfig_Unknown");
				text = text + "\n" + eDirectChallengeMismatch;
				break;
			case EDirectChallengeMismatch.VariantMismatch:
				break;
			}
		}
		return (errTitle: localizedText, errText: text);
	}

	public static string GetRarityLoc(CardRarity rarity)
	{
		string key = "Enum/Rarity/" + rarity;
		return Languages.ActiveLocProvider.GetLocalizedText(key);
	}

	public static string GetCancelLocKey(AllowCancel allowCancel)
	{
		if (allowCancel == AllowCancel.Continue)
		{
			return "DuelScene/ClientPrompt/ClientPrompt_Button_Decline";
		}
		return "DuelScene/ClientPrompt/ClientPrompt_Button_Cancel";
	}

	public static List<string> GetParameters(string key)
	{
		List<string> list = new List<string>();
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText(key);
		if (localizedText.Contains('{') && localizedText.Contains('}'))
		{
			string[] array = localizedText.Split('{', '}');
			foreach (string text in array)
			{
				if (text.Length > 1 && !text.Contains(" ") && !text.Contains("\n"))
				{
					list.Add(text);
				}
			}
		}
		return list;
	}

	public static MTGALocalizedString GetNaiveLocalizedPluralString(int quantity, string singularKey, string pluralKey, string paramName)
	{
		string obj = ((quantity == 1) ? singularKey : pluralKey);
		Dictionary<string, string> parameters = new Dictionary<string, string> { 
		{
			paramName,
			quantity.ToString()
		} };
		MTGALocalizedString mTGALocalizedString = obj;
		mTGALocalizedString.Parameters = parameters;
		return mTGALocalizedString;
	}

	public static string GetLocalizedZoneKey(ZoneType zone, MtgPlayer owner)
	{
		switch (zone)
		{
		case ZoneType.Graveyard:
			if (!owner.IsLocalPlayer)
			{
				return "ZoneType_Opponent_Graveyard";
			}
			return "ZoneType_Your_Graveyard";
		case ZoneType.Hand:
			if (!owner.IsLocalPlayer)
			{
				return "ZoneType_Opponent_Hand";
			}
			return "ZoneType_Your_Hand";
		case ZoneType.Library:
			if (!owner.IsLocalPlayer)
			{
				return "ZoneType_Opponent_Library";
			}
			return "ZoneType_Your_Library";
		case ZoneType.Sideboard:
			if (!owner.IsLocalPlayer)
			{
				return "ZoneType_Opponent_Sideboard";
			}
			return "ZoneType_Your_Sideboard";
		default:
			return $"Enum/ZoneType/ZoneType_{zone}";
		}
	}

	public static string GetLocalizedPercentage(float percent, string locKey, string format = "F0")
	{
		return Languages.ActiveLocProvider.GetLocalizedText(locKey, ("percent", percent.ToString(format)));
	}

	public static string GetLocalizedPercentage(int currentItem, int totalItems, string locKey, string format = "F0")
	{
		return GetLocalizedPercentage((float)currentItem / (float)totalItems * 100f, locKey, format);
	}
}
