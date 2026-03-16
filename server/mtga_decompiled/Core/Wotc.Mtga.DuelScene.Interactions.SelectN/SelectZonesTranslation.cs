using AssetLookupTree;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class SelectZonesTranslation : IWorkflowTranslation<SelectNRequest>
{
	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IEntityViewProvider _entityViewProvider;

	private readonly IClientLocProvider _clientLocProvider;

	private readonly IBrowserController _browserController;

	public SelectZonesTranslation(AssetLookupSystem assetLookupSystem, IContext context)
	{
		_assetLookupSystem = assetLookupSystem;
		_gameStateProvider = context.Get<IGameStateProvider>();
		_entityViewProvider = context.Get<IEntityViewProvider>();
		_clientLocProvider = context.Get<IClientLocProvider>();
		_browserController = context.Get<IBrowserController>();
	}

	public WorkflowBase Translate(SelectNRequest req)
	{
		if (IsSingleZoneSelection(req))
		{
			return new SelectZoneWorkflow(req, _entityViewProvider, _assetLookupSystem, _gameStateProvider);
		}
		return new SelectZonesWorkflow(req, _gameStateProvider, _clientLocProvider, _browserController);
	}

	private static bool IsSingleZoneSelection(SelectNRequest req)
	{
		if (req.MinSel == 1 && req.MaxSel == 1 && req.Ids.Count == 2)
		{
			return !req.CanCancel;
		}
		return false;
	}
}
