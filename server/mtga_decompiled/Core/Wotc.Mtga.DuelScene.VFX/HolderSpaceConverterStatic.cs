using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class HolderSpaceConverterStatic : ISpaceConverter
{
	private readonly ICardHolderProvider _chc;

	private readonly GREPlayerNum _playerNum;

	private readonly CardHolderType _holderType;

	public HolderSpaceConverterStatic(ICardHolderProvider chc, GREPlayerNum playerNum, CardHolderType holderType)
	{
		_chc = chc;
		_playerNum = playerNum;
		_holderType = holderType;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		spaceConverterUtility.AddZoneToSet(_chc, _playerNum, _holderType, set);
	}
}
