using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class TargetSpaceConverter : ISpaceConverter
{
	protected readonly IEntityViewProvider _entityViewProvider;

	public TargetSpaceConverter(IEntityViewProvider entityViewProvider)
	{
		_entityViewProvider = entityViewProvider ?? NullEntityViewProvider.Default;
	}

	public virtual void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		if (!(contextEntity is MtgCardInstance mtgCardInstance))
		{
			return;
		}
		foreach (MtgEntity target in mtgCardInstance.Targets)
		{
			OnTarget(target, set, spaceConverterUtility);
		}
	}

	protected virtual void OnTarget(MtgEntity targetEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		DuelScene_AvatarView avatar;
		if (_entityViewProvider.TryGetCardView(targetEntity.InstanceId, out var cardView))
		{
			spaceConverterUtility.AddCardViewToSet(cardView, set);
		}
		else if (_entityViewProvider.TryGetAvatarById(targetEntity.InstanceId, out avatar))
		{
			set.Add(avatar.EffectsRoot);
		}
	}
}
