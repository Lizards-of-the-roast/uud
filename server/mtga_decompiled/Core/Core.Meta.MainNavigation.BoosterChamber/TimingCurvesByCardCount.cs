using System;
using UnityEngine;

namespace Core.Meta.MainNavigation.BoosterChamber;

[Serializable]
public class TimingCurvesByCardCount
{
	public int CardCount;

	public AnimationCurve CardSpawnTimingCurve;

	public TimingCurvesByRarity defaultTimingCurves;
}
