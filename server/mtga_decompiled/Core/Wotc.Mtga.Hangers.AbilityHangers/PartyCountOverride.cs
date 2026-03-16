using System;
using System.Collections.Generic;
using System.Text;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public class PartyCountOverride : IAbilityHangerTextOverride
{
	private readonly IGreLocProvider _greLocProvider;

	private readonly IClientLocProvider _clientLocProvider;

	private int _count;

	public PartyCountOverride(IGreLocProvider greLocProvider, IClientLocProvider clientLocProvider)
	{
		_greLocProvider = greLocProvider;
		_clientLocProvider = clientLocProvider;
	}

	public bool CanUse(ICardDataAdapter cardData, AbilityPrintingData ability, IReadOnlyCollection<string> layers)
	{
		_count = 0;
		if (!ability.MiscellaneousTerms.Contains(MiscellaneousTerm.Party))
		{
			return false;
		}
		if (int.TryParse(cardData.ActiveAbilityWords.Find("PartySize", (AbilityWordData x, string t) => x.AbilityWord == t).AdditionalDetail, out _count))
		{
			return _count > 0;
		}
		return false;
	}

	public (string Header, string BodyText) GetText(ICardDataAdapter cardData)
	{
		string item = string.Format(_clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/Party_Title_Count"), _count);
		string text = _clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/Party");
		if (_count < 4 && hasUnclaimedPartyClasses(cardData, out var unclaimedClasses))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(text);
			stringBuilder.AppendLine(Environment.NewLine);
			stringBuilder.AppendLine(_clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/Party_Additional"));
			stringBuilder.Append("<#FF9C01>");
			for (int i = 0; i < unclaimedClasses.Length; i++)
			{
				int enumVal = unclaimedClasses[i];
				stringBuilder.Append(_greLocProvider.GetLocalizedTextForEnumValue((SubType)enumVal));
				if (i != unclaimedClasses.Length - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append("</color>");
			text = stringBuilder.ToString();
		}
		return (Header: item, BodyText: text);
		static bool hasUnclaimedPartyClasses(ICardDataAdapter cData, out int[] reference)
		{
			reference = null;
			if (cData.Controller == null)
			{
				return false;
			}
			AbilityWordData abilityWordData = cData.Controller.ActiveAbilityWords.Find((AbilityWordData x) => x.AbilityWord == "UnclaimedPartyClasses");
			if (abilityWordData.Values != null)
			{
				int[] values = abilityWordData.Values;
				if (values == null || values.Length != 0)
				{
					reference = abilityWordData.Values;
					return true;
				}
			}
			return false;
		}
	}
}
