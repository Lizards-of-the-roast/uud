using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Cards.Text;

public class LinkedInfoTitleTextParser : ITextEntryParser
{
	private readonly IGreLocProvider _locManager;

	public LinkedInfoTitleTextParser(IGreLocProvider locManager)
	{
		_locManager = locManager ?? NullGreLocManager.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		foreach (uint linkedInfoTitleLocId in card.LinkedInfoTitleLocIds)
		{
			yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, _locManager.GetLocalizedText(linkedInfoTitleLocId, overrideLang)));
		}
	}
}
