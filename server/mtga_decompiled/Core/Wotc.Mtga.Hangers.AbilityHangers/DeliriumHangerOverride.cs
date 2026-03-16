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

public class DeliriumHangerOverride : IAbilityHangerTextOverride
{
	private const string ABILITY_WORD_DELIRIUM = "Delirium";

	private readonly IGreLocProvider _greLocProvider;

	private readonly IClientLocProvider _clientLocProvider;

	public DeliriumHangerOverride(IGreLocProvider greLocProvider, IClientLocProvider clientLocProvider)
	{
		_greLocProvider = greLocProvider;
		_clientLocProvider = clientLocProvider;
	}

	public bool CanUse(ICardDataAdapter model, AbilityPrintingData ability, IReadOnlyCollection<string> layers)
	{
		if (ability.AbilityWord == AbilityWord.Delirium)
		{
			return layers.Contains("Delirium");
		}
		return false;
	}

	public (string Header, string BodyText) GetText(ICardDataAdapter cardData)
	{
		int num = 0;
		List<int> list = null;
		string text = "";
		foreach (AbilityWordData activeAbilityWord in cardData.ActiveAbilityWords)
		{
			if (activeAbilityWord.AbilityWord == "Delirium")
			{
				num = activeAbilityWord.CardTypes?.Count ?? 0;
				list = activeAbilityWord.CardTypes;
				break;
			}
		}
		string item = string.Format(_clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/Delirium/Title_Count"), num);
		string empty = string.Empty;
		empty = num switch
		{
			0 => _clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/Delirium/Body_Empty"), 
			1 => _clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/Delirium/Body_OneType"), 
			_ => _clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/Delirium/Body_MultipleTypes"), 
		};
		for (int i = 0; i < num; i++)
		{
			string localizedTextForEnumValue = _greLocProvider.GetLocalizedTextForEnumValue((CardType)list[i]);
			text += localizedTextForEnumValue;
			if (i < num - 1)
			{
				text += ", ";
			}
		}
		if (num > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(empty);
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("<#FF9C01>");
			stringBuilder.Append(text);
			stringBuilder.Append("</color>");
			empty = stringBuilder.ToString();
		}
		return (Header: item, BodyText: empty);
	}
}
