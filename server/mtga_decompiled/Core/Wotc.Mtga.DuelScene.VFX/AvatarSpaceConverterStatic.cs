using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class AvatarSpaceConverterStatic : ISpaceConverter
{
	private readonly IAvatarViewProvider _avatarViewProvider;

	private readonly GREPlayerNum _playerNum;

	public AvatarSpaceConverterStatic(IAvatarViewProvider avatarViewProvider, GREPlayerNum playerNum)
	{
		_avatarViewProvider = avatarViewProvider;
		_playerNum = playerNum;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		spaceConverterUtility.AddPlayerViewToSet(_avatarViewProvider, _playerNum, set);
	}
}
