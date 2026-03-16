using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class AvatarSpaceConverterTarget : ISpaceConverter
{
	private readonly IAvatarViewProvider _avatarViewProvider;

	public AvatarSpaceConverterTarget(IAvatarViewProvider avatarViewProvider)
	{
		_avatarViewProvider = avatarViewProvider ?? NullAvatarViewProvider.Default;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
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
			spaceConverterUtility.AddPlayerViewToSet(_avatarViewProvider, playerNum, set);
		}
	}
}
