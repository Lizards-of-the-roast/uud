using AssetLookupTree;
using AssetLookupTree.Payloads.GameState.Designation;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class AddDesignationUXEvent : DesignationUXEventBase
{
	public AddDesignationUXEvent(DesignationData designation, IDesignationController designationController, GameManager gameManager)
		: base(designation, designationController, gameManager)
	{
	}

	public override void Execute()
	{
		_designationController.AddDesignation(Designation);
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.Designation = Designation.Type;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AddedVfx> loadedTree))
		{
			AddedVfx payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_vfxProvider.PlayVFX(payload.VfxData, null);
			}
		}
		Complete();
	}
}
