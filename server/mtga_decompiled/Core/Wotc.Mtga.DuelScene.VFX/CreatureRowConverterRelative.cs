using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.VFX;

public class CreatureRowConverterRelative : BattlefieldSpaceConverter
{
	public enum RelativeContext
	{
		Local,
		Opponent
	}

	private readonly RelativeContext _relativeContext;

	public CreatureRowConverterRelative(ICardHolderProvider chc, RelativeContext relativeContext)
		: base(chc)
	{
		_relativeContext = relativeContext;
	}

	public override void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		GREPlayerNum gREPlayerNum = GREPlayerNum.Invalid;
		if (!(contextEntity is MtgCardInstance mtgCardInstance))
		{
			if (contextEntity is MtgPlayer mtgPlayer)
			{
				gREPlayerNum = mtgPlayer.ClientPlayerEnum;
			}
		}
		else
		{
			gREPlayerNum = mtgCardInstance.Controller?.ClientPlayerEnum ?? GREPlayerNum.Invalid;
		}
		if (gREPlayerNum != GREPlayerNum.Invalid && _relativeContext == RelativeContext.Opponent)
		{
			switch (gREPlayerNum)
			{
			case GREPlayerNum.LocalPlayer:
				gREPlayerNum = GREPlayerNum.Opponent;
				break;
			case GREPlayerNum.Opponent:
				gREPlayerNum = GREPlayerNum.LocalPlayer;
				break;
			}
		}
		set.Add(base._battlefield.GetRegionTransformForCardType(CardType.Creature, gREPlayerNum));
	}
}
