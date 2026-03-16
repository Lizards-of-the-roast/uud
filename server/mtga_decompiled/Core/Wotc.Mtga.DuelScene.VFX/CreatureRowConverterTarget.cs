using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.VFX;

public class CreatureRowConverterTarget : BattlefieldSpaceConverter
{
	public CreatureRowConverterTarget(ICardHolderProvider chc)
		: base(chc)
	{
	}

	public override void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		if (!(contextEntity is MtgCardInstance mtgCardInstance))
		{
			return;
		}
		foreach (MtgEntity target in mtgCardInstance.Targets)
		{
			GREPlayerNum playerNum = GREPlayerNum.Invalid;
			if (!(target is MtgCardInstance mtgCardInstance2))
			{
				if (target is MtgPlayer mtgPlayer)
				{
					playerNum = mtgPlayer.ClientPlayerEnum;
				}
			}
			else
			{
				playerNum = mtgCardInstance2.Controller?.ClientPlayerEnum ?? GREPlayerNum.Invalid;
			}
			set.Add(base._battlefield.GetRegionTransformForCardType(CardType.Creature, playerNum));
		}
	}
}
