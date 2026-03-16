using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.VFX;

public class CreatureRowConverterStatic : BattlefieldSpaceConverter
{
	private readonly GREPlayerNum _playerNum;

	public CreatureRowConverterStatic(ICardHolderProvider cardHolderProvider, GREPlayerNum playerNum)
		: base(cardHolderProvider)
	{
		_playerNum = playerNum;
	}

	public override void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		set.Add(base._battlefield.GetRegionTransformForCardType(CardType.Creature, _playerNum));
	}
}
