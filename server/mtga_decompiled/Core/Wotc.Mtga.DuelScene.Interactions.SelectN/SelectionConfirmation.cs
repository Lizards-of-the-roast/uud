using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public static class SelectionConfirmation
{
	public static ISelectionConfirmation CreateDefault(IContext context)
	{
		return CreateDefault(context.Get<ICardDatabaseAdapter>() ?? NullCardDatabaseAdapter.Default, context.Get<IGameStateProvider>() ?? NullGameStateProvider.Default, context.Get<IResolutionEffectProvider>() ?? NullResolutionEffectProvider.Default, context.Get<IEntityNameProvider<uint>>() ?? NullIdNameProvider.Default);
	}

	private static ISelectionConfirmation CreateDefault(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IResolutionEffectProvider resolutionEffectProvider, IEntityNameProvider<uint> entityNameProvider)
	{
		return new SelectionConfirmationAggregate(new CurseSelectionWarning(gameStateProvider, cardDatabase.ClientLocProvider), new ExploitSelectionWarning(gameStateProvider, cardDatabase), new ColdLocalPlayerSelectionWarning(cardDatabase.ClientLocProvider), new AuraColdSelectionWarning(gameStateProvider, entityNameProvider, cardDatabase.ClientLocProvider), new BirthingRitualSacrificeSelectionWarning(gameStateProvider, resolutionEffectProvider, cardDatabase.ClientLocProvider));
	}
}
