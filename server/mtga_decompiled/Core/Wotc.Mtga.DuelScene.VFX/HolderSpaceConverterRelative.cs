using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class HolderSpaceConverterRelative : ISpaceConverter
{
	public enum RelativeContext
	{
		Local,
		Opponent
	}

	private readonly ICardHolderProvider _chc;

	private readonly RelativeContext _relativeContext;

	private readonly CardHolderType _holderType;

	public HolderSpaceConverterRelative(ICardHolderProvider chc, RelativeContext relativeContext, CardHolderType holderType)
	{
		_chc = chc;
		_relativeContext = relativeContext;
		_holderType = holderType;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
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
		spaceConverterUtility.AddZoneToSet(_chc, gREPlayerNum, _holderType, set);
	}
}
