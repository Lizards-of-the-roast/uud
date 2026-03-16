using System;
using UnityEngine;

namespace Core.Meta.MainNavigation.BoosterChamber;

[Serializable]
public class TimingCurvesByRarity
{
	public AnimationCurve CardMovementTiming;

	public AnimationCurve CardMovementEase;

	public AnimationCurve CardFlipTiming;

	public AnimationCurve CardFlipEase;
}
