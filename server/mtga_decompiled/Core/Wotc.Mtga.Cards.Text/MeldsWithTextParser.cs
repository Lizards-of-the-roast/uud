using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Text;

public class MeldsWithTextParser : ITextEntryParser
{
	private readonly IGreLocProvider _locProvider;

	public MeldsWithTextParser(IGreLocProvider locProvider)
	{
		_locProvider = locProvider ?? NullGreLocManager.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		if (CanGenerateTextForCard(card) && _locProvider.TryGetLocalizedText(out var text, card.ReminderTextId, overrideLang))
		{
			yield return new BasicTextEntry(string.Format(colorSettings.DefaultFormat, text));
		}
	}

	private static bool CanGenerateTextForCard(ICardDataAdapter card)
	{
		if (card.Instance != null)
		{
			if (card.Instance.IsCopy)
			{
				return false;
			}
			if (card.Instance.IsObjectCopy)
			{
				return false;
			}
			if (!card.Instance.IsCard())
			{
				return false;
			}
		}
		return card.LinkedFaceType == LinkedFace.MeldedPermanent;
	}
}
