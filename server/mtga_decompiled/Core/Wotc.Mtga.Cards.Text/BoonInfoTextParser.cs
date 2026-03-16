using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Text;

public class BoonInfoTextParser : ITextEntryParser
{
	private readonly IClientLocProvider _locManager;

	public BoonInfoTextParser(IClientLocProvider locManager)
	{
		_locManager = locManager ?? NullLocProvider.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang)
	{
		if (card.ObjectType == GameObjectType.Boon)
		{
			uint valueOrDefault = card.Instance.BoonTriggersRemaining.GetValueOrDefault();
			if (card.Instance.BoonTriggersInitial.GetValueOrDefault() > 1)
			{
				string localizedTextForLanguage = _locManager.GetLocalizedTextForLanguage("DuelScene/RuleText/BoonTriggersRemaining", overrideLang, ("count", valueOrDefault.ToString()));
				yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, localizedTextForLanguage));
			}
		}
	}
}
