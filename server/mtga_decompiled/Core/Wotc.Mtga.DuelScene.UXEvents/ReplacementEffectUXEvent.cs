using AssetLookupTree;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.VFX;

namespace Wotc.Mtga.DuelScene.UXEvents;

public abstract class ReplacementEffectUXEvent : UXEvent
{
	protected AssetLookupSystem _assetLookupSystem;

	protected EntityViewManager _viewManager;

	protected IReplacementEffectController _replacementController;

	protected IVfxProvider _vfxProvider;

	public ReplacementEffectData Data { get; private set; }

	public MtgEntity Entity { get; private set; }

	public ReplacementEffectUXEvent(ReplacementEffectData data, MtgEntity entity, GameManager gameManager, IReplacementEffectController replacementEffectController)
	{
		Data = data;
		Entity = entity;
		_assetLookupSystem = gameManager.AssetLookupSystem;
		_viewManager = gameManager.ViewManager;
		_vfxProvider = gameManager.VfxProvider;
		_replacementController = replacementEffectController ?? NullReplacementEffectController.Default;
	}
}
