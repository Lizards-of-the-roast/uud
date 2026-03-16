using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class AvatarSpaceConverterRelative : ISpaceConverter
{
	public enum RelativeContext
	{
		Local,
		Opponent
	}

	private readonly IAvatarViewProvider _avatarProvider;

	private readonly RelativeContext _relativeContext;

	public AvatarSpaceConverterRelative(IAvatarViewProvider avatarProvider, RelativeContext relativeContext)
	{
		_avatarProvider = avatarProvider ?? NullAvatarViewProvider.Default;
		_relativeContext = relativeContext;
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
		spaceConverterUtility.AddPlayerViewToSet(_avatarProvider, gREPlayerNum, set);
	}
}
