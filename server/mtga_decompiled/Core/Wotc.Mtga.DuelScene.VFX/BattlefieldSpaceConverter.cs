using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public abstract class BattlefieldSpaceConverter : ISpaceConverter
{
	private readonly ICardHolderProvider _chc;

	private IBattlefieldCardHolder _battlefieldCache;

	protected IBattlefieldCardHolder _battlefield => _battlefieldCache ?? (_battlefieldCache = _chc.GetCardHolder(GREPlayerNum.Invalid, CardHolderType.Battlefield) as IBattlefieldCardHolder);

	protected BattlefieldSpaceConverter(ICardHolderProvider chc)
	{
		_chc = chc ?? NullCardHolderProvider.Default;
	}

	public abstract void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility);
}
