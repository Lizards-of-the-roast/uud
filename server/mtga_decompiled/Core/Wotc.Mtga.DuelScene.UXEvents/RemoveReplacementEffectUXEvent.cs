using AssetLookupTree;
using AssetLookupTree.Payloads.ReplacementEffect;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class RemoveReplacementEffectUXEvent : ReplacementEffectUXEvent
{
	public RemoveReplacementEffectUXEvent(ReplacementEffectData data, MtgEntity entity, GameManager gameManager, IReplacementEffectController replacementEffectController)
		: base(data, entity, gameManager, replacementEffectController)
	{
	}

	public override void Execute()
	{
		MtgEntity entity = base.Entity;
		if (!(entity is MtgPlayer mtgPlayer))
		{
			if (!(entity is MtgCardInstance mtgCardInstance))
			{
				if (entity == null)
				{
					_replacementController.RemoveReplacementEffect(base.Data);
				}
			}
			else
			{
				DuelScene_CDC cardView = _viewManager.GetCardView(mtgCardInstance.InstanceId);
				if ((object)cardView != null)
				{
					_assetLookupSystem.Blackboard.Clear();
					_assetLookupSystem.Blackboard.SetCardDataExtensive(cardView.Model);
					_assetLookupSystem.Blackboard.CardHolderType = cardView.CurrentCardHolder.CardHolderType;
					AssetLookupTree<EndVFX> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<EndVFX>();
					AssetLookupTree<EndSFX> assetLookupTree2 = _assetLookupSystem.TreeLoader.LoadTree<EndSFX>();
					EndVFX payload = assetLookupTree.GetPayload(_assetLookupSystem.Blackboard);
					EndSFX payload2 = assetLookupTree2.GetPayload(_assetLookupSystem.Blackboard);
					_assetLookupSystem.Blackboard.Clear();
					if (payload != null)
					{
						foreach (VfxData vfxData in payload.VfxDatas)
						{
							_vfxProvider.PlayVFX(vfxData, cardView.Model);
						}
					}
					if (payload2 != null)
					{
						AudioManager.PlayAudio(payload2.SfxData.AudioEvents, cardView.gameObject);
					}
				}
			}
		}
		else
		{
			_viewManager.GetAvatarById(mtgPlayer.InstanceId)?.OnReplacementRemoved();
		}
		Complete();
	}
}
