using System.Collections.Generic;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.CastingTimeOption;

public class CastTimeOptionHeaderProvider : ICastTimeOptionHeaderProvider
{
	private readonly IReadOnlyDictionary<CastingTimeOptionType, ICastTimeOptionHeaderProvider> _typeMap;

	public CastTimeOptionHeaderProvider(IContext context)
		: this(context.Get<ICardDatabaseAdapter>(), context.Get<IGameStateProvider>())
	{
	}

	private CastTimeOptionHeaderProvider(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider)
	{
		_typeMap = new Dictionary<CastingTimeOptionType, ICastTimeOptionHeaderProvider>
		{
			[CastingTimeOptionType.Done] = NullCastTimeOptionHeaderProvider.Default,
			[CastingTimeOptionType.AdditionalCost] = new AdditionalCostHeaderProvider(cardDatabase, gameStateProvider)
		};
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCastTimeOptionHeader(MtgCastTimeOption castTimeOption)
	{
		if (!_typeMap.TryGetValue(castTimeOption.Type, out var value))
		{
			return null;
		}
		return value.GetCastTimeOptionHeader(castTimeOption);
	}
}
