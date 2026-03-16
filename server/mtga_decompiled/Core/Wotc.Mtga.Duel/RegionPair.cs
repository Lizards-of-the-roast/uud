using System;

namespace Wotc.Mtga.Duel;

public readonly struct RegionPair : IEquatable<RegionPair>
{
	public readonly BattlefieldRegionType FromRegion;

	public readonly GREPlayerNum FromOwner;

	public readonly BattlefieldRegionType ToRegion;

	public readonly GREPlayerNum ToOwner;

	public RegionPair(BattlefieldRegionType fromRegion, GREPlayerNum fromOwner, BattlefieldRegionType toRegion, GREPlayerNum toOwner)
	{
		FromRegion = fromRegion;
		FromOwner = fromOwner;
		ToRegion = toRegion;
		ToOwner = toOwner;
	}

	public bool Equals(RegionPair other)
	{
		if (FromRegion == other.FromRegion && FromOwner == other.FromOwner && ToRegion == other.ToRegion)
		{
			return ToOwner == other.ToOwner;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is RegionPair other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(((((uint)((int)FromRegion * 397) ^ (uint)FromOwner) * 397) ^ (uint)ToRegion) * 397) ^ (int)ToOwner;
	}

	public override string ToString()
	{
		return $"{FromOwner} {FromRegion} -> {ToOwner} {ToRegion}";
	}
}
