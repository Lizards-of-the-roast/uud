using AssetLookupTree;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.VFX;

namespace Wotc.Mtga.DuelScene.UXEvents;

public abstract class DesignationUXEventBase : UXEvent
{
	public readonly DesignationData Designation;

	protected readonly IDesignationController _designationController;

	protected readonly AssetLookupSystem _assetLookupSystem;

	protected readonly IVfxProvider _vfxProvider;

	public DesignationUXEventBase(DesignationData designation, IDesignationController designationController, GameManager gameManager)
	{
		Designation = designation;
		_designationController = designationController ?? NullDesignationController.Default;
		_assetLookupSystem = gameManager.AssetLookupSystem;
		_vfxProvider = gameManager.VfxProvider;
	}
}
