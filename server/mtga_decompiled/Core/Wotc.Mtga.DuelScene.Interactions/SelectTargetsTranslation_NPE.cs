using AssetLookupTree;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions.SelectTargets;

namespace Wotc.Mtga.DuelScene.Interactions;

public class SelectTargetsTranslation_NPE : IWorkflowTranslation<SelectTargetsRequest>
{
	private readonly IObjectPool _objPool;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly ICardTitleProvider _cardTitleProvider;

	private readonly IAbilityDataProvider _abilityProvider;

	private readonly IGameplaySettingsProvider _gameplaySettingsProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IPromptTextProvider _promptTextProvider;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly IBrowserManager _browserManager;

	private readonly IBrowserHeaderTextProvider _headerTextProvider;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly NPEDirector _director;

	public SelectTargetsTranslation_NPE(IContext context, AssetLookupSystem assetLookupSystem, NPEDirector director)
		: this(context.Get<IObjectPool>(), context.Get<ICardDatabaseAdapter>(), context.Get<IGameStateProvider>(), context.Get<IGameplaySettingsProvider>(), context.Get<ICardHolderProvider>(), context.Get<IBrowserManager>(), context.Get<IBrowserHeaderTextProvider>(), context.Get<ICardViewProvider>(), context.Get<IPromptTextProvider>(), assetLookupSystem, director)
	{
	}

	private SelectTargetsTranslation_NPE(IObjectPool objectPool, ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IGameplaySettingsProvider gameplaySettingsProvider, ICardHolderProvider cardHolderProvider, IBrowserManager browserManager, IBrowserHeaderTextProvider headerTextProvider, ICardViewProvider cardViewProvider, IPromptTextProvider promptTextProvider, AssetLookupSystem assetLookupSystem, NPEDirector director)
	{
		_objPool = objectPool ?? NullObjectPool.Default;
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_gameplaySettingsProvider = gameplaySettingsProvider ?? NullGameplaySettingsProvider.Default;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_promptTextProvider = promptTextProvider ?? NullPromptTextProvider.Default;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_browserManager = browserManager ?? NullBrowserManager.Default;
		_headerTextProvider = headerTextProvider ?? NullBrowserHeaderTextProvider.Default;
		_assetLookupSystem = assetLookupSystem;
		_director = director;
	}

	public WorkflowBase Translate(SelectTargetsRequest req)
	{
		return new SelectTargetsWorkflow_NPE(req, _objPool, _cardDatabase, _gameStateProvider, _gameplaySettingsProvider, _cardHolderProvider, _browserManager, _headerTextProvider, _cardViewProvider, _promptTextProvider, NullAutoTargetingSolution.Default, _assetLookupSystem, _director);
	}
}
