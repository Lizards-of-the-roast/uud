using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Cards.Text;

public class MutationsParser : ITextEntryParser
{
	private readonly ICardTitleProvider _cardTitleProvider;

	private readonly IClientLocProvider _clientLocManager;

	private readonly List<string> _mutationChildrenNamesCache = new List<string>(10);

	public MutationsParser(ICardTitleProvider cardTitleProvider, IClientLocProvider clientLocManager)
	{
		_cardTitleProvider = cardTitleProvider;
		_clientLocManager = clientLocManager;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		MtgCardInstance instance = card.Instance;
		if (instance == null || instance.MutationChildren.Count <= 0)
		{
			yield break;
		}
		_mutationChildrenNamesCache.Clear();
		foreach (MtgCardInstance mutationChild in instance.MutationChildren)
		{
			_mutationChildrenNamesCache.Add(_cardTitleProvider.GetCardTitle(mutationChild.GrpId));
		}
		if (instance.OverlayGrpId.HasValue)
		{
			_mutationChildrenNamesCache.Remove(_cardTitleProvider.GetCardTitle(instance.OverlayGrpId.Value));
			_mutationChildrenNamesCache.Insert(0, _cardTitleProvider.GetCardTitle(instance.BaseGrpId));
		}
		yield return new BasicTextEntry(string.Format(colorSettings.DefaultFormat, _clientLocManager.GetLocalizedTextForLanguage("Card/Textbox/MutationsList", overrideLang, ("mutationList", string.Join("; ", _mutationChildrenNamesCache)))));
		_mutationChildrenNamesCache.Clear();
	}
}
