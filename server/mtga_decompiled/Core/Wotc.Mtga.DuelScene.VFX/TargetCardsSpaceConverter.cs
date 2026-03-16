using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class TargetCardsSpaceConverter : TargetSpaceConverter
{
	public TargetCardsSpaceConverter(IEntityViewProvider entityViewProvider)
		: base(entityViewProvider)
	{
	}

	protected override void OnTarget(MtgEntity targetEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		if (_entityViewProvider.TryGetCardView(targetEntity.InstanceId, out var cardView))
		{
			spaceConverterUtility.AddCardViewToSet(cardView, set);
		}
	}
}
