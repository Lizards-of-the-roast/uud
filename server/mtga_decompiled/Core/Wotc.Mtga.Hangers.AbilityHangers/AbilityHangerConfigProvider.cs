using System;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public class AbilityHangerConfigProvider : AbilityHangerBaseConfigProvider
{
	private readonly IEntityNameProvider<uint> _entityNameProvider;

	public AbilityHangerConfigProvider(AssetLookupSystem assetLookupSystem, ICardDatabaseAdapter cardDatabase, IClientLocProvider locManager, IObjectPool genericPool, IEntityNameProvider<uint> entityNameProvider)
		: this(new ALTQueryProvider(assetLookupSystem, genericPool), cardDatabase.CardDataProvider, cardDatabase.CardTitleProvider, cardDatabase.GreLocProvider, locManager, genericPool, entityNameProvider)
	{
	}

	public AbilityHangerConfigProvider(IHangerLookupProvider queryProvider, ICardDataProvider cardProvider, ICardTitleProvider cardTitleProvider, IGreLocProvider greLocProvider, IClientLocProvider locManager, IObjectPool genericPool, IEntityNameProvider<uint> entityNameProvider)
		: base(queryProvider, cardProvider, cardTitleProvider, greLocProvider, locManager, genericPool)
	{
		_entityNameProvider = entityNameProvider;
	}

	protected override void HangerTextParameters(ICardDataAdapter cardData, AbilityBadgeData badgeData, ref string headerText, ref string bodyText, out bool convertSymbols)
	{
		base.HangerTextParameters(cardData, badgeData, ref headerText, ref bodyText, out convertSymbols);
		foreach (AbilityWordData item in cardData.ActiveAbilityWords ?? Array.Empty<AbilityWordData>())
		{
			if (item.AbilityWord == "StartingPlayerId")
			{
				uint entity = uint.Parse(item.AdditionalDetail);
				string name = _entityNameProvider.GetName(entity);
				bodyText = bodyText.Replace("{StartingPlayerName}", name);
				convertSymbols = false;
			}
		}
	}
}
