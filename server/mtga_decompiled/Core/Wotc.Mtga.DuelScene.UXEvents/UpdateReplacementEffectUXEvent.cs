using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class UpdateReplacementEffectUXEvent : ReplacementEffectUXEvent
{
	public UpdateReplacementEffectUXEvent(ReplacementEffectData data, MtgEntity entity, GameManager gameManager, IReplacementEffectController replacementEffectController)
		: base(data, entity, gameManager, replacementEffectController)
	{
	}

	public override void Execute()
	{
		_replacementController.UpdateReplacementEffect(base.Data);
		Complete();
	}
}
