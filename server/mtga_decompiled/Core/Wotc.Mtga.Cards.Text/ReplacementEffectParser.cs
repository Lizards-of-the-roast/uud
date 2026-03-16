using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Cards.Text;

public class ReplacementEffectParser : ITextEntryParser
{
	private readonly ICardTitleProvider _cardTitleProvider;

	private readonly IObjectPool _objPool;

	private readonly Func<MtgGameState> _getCurrentGameState;

	public ReplacementEffectParser(ICardTitleProvider cardTitleProvider, IObjectPool objPool, Func<MtgGameState> getCurrentGameState)
	{
		_cardTitleProvider = cardTitleProvider ?? NullCardTitleProvider.Default;
		_objPool = objPool ?? new NullObjectPool();
		_getCurrentGameState = getCurrentGameState ?? ((Func<MtgGameState>)(() => (MtgGameState)null));
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		MtgGameState gameState = _getCurrentGameState();
		if (gameState == null)
		{
			yield break;
		}
		foreach (uint item in DamageRedirectionIds(card.Instance, gameState))
		{
			MtgCardInstance cardById = gameState.GetCardById(item);
			if (cardById != null)
			{
				yield return new BasicTextEntry(string.Format(colorSettings.SupplementalFormat, _cardTitleProvider.GetCardTitle(cardById.GrpId, formatted: true, overrideLang)));
			}
		}
	}

	private IEnumerable<uint> DamageRedirectionIds(MtgCardInstance card, MtgGameState gameState)
	{
		if (!gameState.ReplacementEffects.TryGetValue(card.InstanceId, out var value) && !gameState.ReplacementEffects.TryGetValue(card.ParentId, out value))
		{
			yield break;
		}
		HashSet<uint> redirectionIds = _objPool.PopObject<HashSet<uint>>();
		foreach (ReplacementEffectData item in value)
		{
			if (item.DamageRedirectionId.HasValue)
			{
				redirectionIds.Add(item.DamageRedirectionId.Value);
			}
		}
		foreach (uint item2 in redirectionIds)
		{
			yield return item2;
		}
		redirectionIds.Clear();
		_objPool.PushObject(redirectionIds);
	}
}
