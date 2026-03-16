using AssetLookupTree;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectTargets;

public class SelectTargetsWorkflow_NPE : SelectTargetsWorkflow
{
	private readonly NPEDirector _director;

	public SelectTargetsWorkflow_NPE(SelectTargetsRequest request, IObjectPool objectPool, ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IGameplaySettingsProvider gameplaySettingsProvider, ICardHolderProvider cardHolderProvider, IBrowserManager browserManager, IBrowserHeaderTextProvider headerTextProvider, ICardViewProvider cardViewProvider, IPromptTextProvider promptTextProvider, IAutoTargetingSolution autoTargetingSolution, AssetLookupSystem assetLookupSystem, NPEDirector director)
		: base(request, objectPool, cardDatabase, gameStateProvider, gameplaySettingsProvider, cardHolderProvider, browserManager, headerTextProvider, cardViewProvider, promptTextProvider, autoTargetingSolution, assetLookupSystem)
	{
		_director = director;
	}

	protected override void ApplyTheSelection(Target target)
	{
		if (!_director.WarnForBadTarget(target))
		{
			base.ApplyTheSelection(target);
		}
	}
}
