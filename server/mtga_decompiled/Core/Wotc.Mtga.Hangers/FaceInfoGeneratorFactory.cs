using System;
using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;

namespace Wotc.Mtga.Hangers;

public static class FaceInfoGeneratorFactory
{
	public static class DuelScene
	{
		public static class Handheld
		{
			public static IFaceInfoGenerator HoverGenerator(ICardDatabaseAdapter cardDatabase, IHighlightProvider highlightProvider, AssetLookupSystem assetLookupSystem, DeckFormat currentEventFormat, IObjectPool genericPool)
			{
				return new SortedFaceInfoGenerator(_comparer, new ParentFaceInfoGenerator(new FacedownInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new CopyFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new PrototypeFaceInfoGenerator(cardDatabase.AbilityDataProvider, cardDatabase.ClientLocProvider, genericPool), new DFCFaceInfoGenerator(cardDatabase), new HighlightedFaceInfoGenerator(new MDFCFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider), highlightProvider), new NamedCardFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider, currentEventFormat), new DungeonFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new RingTemptsYouFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new SpecializeFromFaceInfoGenerator(cardDatabase.ClientLocProvider), new TokenFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider, assetLookupSystem), new ConjureFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider), new SpecializesToFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider), new MeldFaceInfoGenerator(cardDatabase.AbilityDataProvider, cardDatabase.ClientLocProvider, genericPool)));
			}
		}

		public static IFaceInfoGenerator HoverGenerator(ICardDatabaseAdapter cardDatabase, IHighlightProvider highlightProvider, DeckFormat currentEventFormat, IObjectPool genericPool)
		{
			return new CountLimitedFaceInfoGenerator(1u, new SortedFaceInfoGenerator(_comparer, new ParentFaceInfoGenerator(new FacedownInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new CopyFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new PrototypeFaceInfoGenerator(cardDatabase.AbilityDataProvider, cardDatabase.ClientLocProvider, genericPool), new DFCFaceInfoGenerator(cardDatabase), new HighlightedFaceInfoGenerator(new MDFCFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider), highlightProvider), new NamedCardFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider, currentEventFormat))));
		}

		public static IFaceInfoGenerator ExamineGenerator(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, DeckFormat currentEventFormat, IObjectPool genericPool)
		{
			return new SortedFaceInfoGenerator(_comparer, new ParentFaceInfoGenerator(new SpecializesIntoGenerator(cardDatabase.ClientLocProvider, genericPool), new FacedownInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new CopyFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new PrototypeFaceInfoGenerator(cardDatabase.AbilityDataProvider, cardDatabase.ClientLocProvider, genericPool), new DFCFaceInfoGenerator(cardDatabase), new MDFCFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider), new NamedCardFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider, currentEventFormat), new DungeonFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new RingTemptsYouFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new DayNightFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new SpecializeFromFaceInfoGenerator(cardDatabase.ClientLocProvider), new TokenFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider, assetLookupSystem), new ConjureFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider), new MeldFaceInfoGenerator(cardDatabase.AbilityDataProvider, cardDatabase.ClientLocProvider, genericPool)));
		}

		public static IFaceInfoGenerator LeftBattlefieldGenerator(ICardDatabaseAdapter cardDatabase, Func<MtgGameState> getCurrentGameState, IEqualityComparer<ICardDataAdapter> equalityComparer)
		{
			return new ParentFaceInfoGenerator(new RemoveDuplicatesFaceInfoGenerator(new ParentFaceInfoGenerator(new TriggeredByFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider, getCurrentGameState), new LimboParentFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider, getCurrentGameState)), equalityComparer), new AdditionalCostFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider, getCurrentGameState));
		}
	}

	private const uint HOVER_COUNT_LIMIT = 1u;

	private static IComparer<FaceHanger.FaceCardInfo> _comparer = new FaceHangerComparer();

	public static IFaceInfoGenerator HoverGenerator(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, IObjectPool genericPool)
	{
		return new SortedFaceInfoGenerator(_comparer, new ParentFaceInfoGenerator(new DFCFaceInfoGenerator(cardDatabase), new MDFCFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider), new DungeonFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new DayNightFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new RingTemptsYouFaceInfoGenerator(cardDatabase.CardDataProvider, cardDatabase.ClientLocProvider), new MeldFaceInfoGenerator(cardDatabase.AbilityDataProvider, cardDatabase.ClientLocProvider, genericPool), new SpecializesIntoGenerator(cardDatabase.ClientLocProvider, genericPool), new TokenFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider, assetLookupSystem), new ConjureFaceInfoGenerator(cardDatabase, cardDatabase.ClientLocProvider)));
	}
}
