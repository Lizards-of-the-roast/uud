using AssetLookupTree;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

public static class SpecialHangerConfigProviders
{
	public static IHangerConfigProvider Create_DS(IContext context, IPathProvider<string> iconPathProvider, AssetLookupSystem assetLookupSystem)
	{
		IObjectPool objectPool = context.Get<IObjectPool>() ?? NullObjectPool.Default;
		ICardDatabaseAdapter cardDatabaseAdapter = context.Get<ICardDatabaseAdapter>() ?? NullCardDatabaseAdapter.Default;
		IClientLocProvider clientLocProvider = cardDatabaseAdapter.ClientLocProvider;
		IGameStateProvider gameStateProvider = context.Get<IGameStateProvider>() ?? NullGameStateProvider.Default;
		IEntityNameProvider<uint> entityNameProvider = context.Get<IEntityNameProvider<uint>>() ?? NullIdNameProvider.Default;
		return new HangerConfigProviderAggregate(new HappilyEverAfterConfigProvider(objectPool, clientLocProvider, gameStateProvider, iconPathProvider), new SunsetRevelryConfigProvider(clientLocProvider, gameStateProvider, iconPathProvider), new PrivateInfoConfigProvider(clientLocProvider), new PublicInfoConfigProvider(clientLocProvider), new PhasedOutConfigProvider(clientLocProvider), new OwnershipHangerConfigProvider(clientLocProvider, entityNameProvider, context.Get<ICardViewProvider>(), gameStateProvider), new ControllerHangerProvider(gameStateProvider, clientLocProvider, entityNameProvider), new BattleProtectorHangerProvider(entityNameProvider, clientLocProvider, gameStateProvider), new RoomStateHangerProvider(cardDatabaseAdapter, context.Get<IEntityNameProvider<MtgCardInstance>>()), new OnAnAdventureHangerProvider(clientLocProvider, iconPathProvider, entityNameProvider, cardDatabaseAdapter, assetLookupSystem), new ExiledUnderHangerConfigProvider(clientLocProvider, gameStateProvider, context.Get<IBrowserProvider>(), entityNameProvider), new EnteredZoneThisTurnConfigProvider(clientLocProvider), new BasicLandWithAbilitiesRemovedConfigProvider(clientLocProvider, gameStateProvider, cardDatabaseAdapter));
	}
}
