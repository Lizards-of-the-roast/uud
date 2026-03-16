using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.DuelScene.Interactions;
using GreClient.Rules;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class SelectZoneWorkflow : WorkflowBase<SelectNRequest>
{
	private readonly IEntityViewProvider _entityViewProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IGameStateProvider _gameStateProvider;

	public SelectZoneWorkflow(SelectNRequest req, IEntityViewProvider entityViewProvider, AssetLookupSystem assetLookupSystem, IGameStateProvider gameStateProvider)
		: base(req)
	{
		_entityViewProvider = entityViewProvider;
		_assetLookupSystem = assetLookupSystem;
		_gameStateProvider = gameStateProvider;
	}

	protected override void ApplyInteractionInternal()
	{
		SetButtons();
	}

	private void SelectZone(uint zoneId)
	{
		_request.SubmitSelection(zoneId);
	}

	protected override void SetButtons()
	{
		AssetLookupTree<SelectZoneWorkflowButtonInfoPayload> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<SelectZoneWorkflowButtonInfoPayload>();
		IBlackboard blackboard = _assetLookupSystem.Blackboard;
		blackboard.Clear();
		blackboard.SetCardDataExtensive(_entityViewProvider.GetCardView(_request.SourceId)?.Model);
		base.Buttons.Cleanup();
		foreach (uint id in _request.Ids)
		{
			MtgZone zone = (blackboard.ZoneSelection = _gameStateProvider.LatestGameState.Value.GetZoneById(id));
			SelectZoneWorkflowButtonInfoPayload payload = assetLookupTree.GetPayload(blackboard);
			PromptButtonData promptButtonData = new PromptButtonData
			{
				ButtonText = GetButtonText(zone, id),
				Style = ButtonStyle.StyleType.Secondary,
				ButtonCallback = delegate
				{
					SelectZone(id);
				}
			};
			if (payload != null && payload.IsMainButton)
			{
				promptButtonData.Style = ButtonStyle.StyleType.Main;
			}
			if (payload != null && payload.IsTopSelection)
			{
				base.Buttons.WorkflowButtons.Add(promptButtonData);
			}
			else
			{
				base.Buttons.WorkflowButtons.Insert(0, promptButtonData);
			}
		}
		blackboard.Clear();
		base.SetButtons();
	}

	private string GetButtonText(MtgZone zone, uint zoneId)
	{
		if (zone != null)
		{
			return Utils.GetLocalizedZoneKey(zone.Type, zone.Owner);
		}
		return $"Zone: {zoneId}";
	}
}
