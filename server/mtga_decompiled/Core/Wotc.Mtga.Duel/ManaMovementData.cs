using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Duel;

public readonly struct ManaMovementData
{
	public enum ResourceSourceType
	{
		Card,
		Pool,
		Stack
	}

	public enum ResourceSinkType
	{
		Card,
		Pool,
		Stack
	}

	public readonly MtgEntity Source;

	public readonly MtgEntity Sink;

	public readonly MtgMana Mana;

	public bool IsValid => Mana != null;

	public ResourceSourceType SourceType
	{
		get
		{
			if (!(Source is MtgCardInstance mtgCardInstance))
			{
				return ResourceSourceType.Pool;
			}
			MtgZone zone = mtgCardInstance.Zone;
			if (zone == null || zone.Type != ZoneType.Stack)
			{
				return ResourceSourceType.Card;
			}
			return ResourceSourceType.Stack;
		}
	}

	public ResourceSinkType SinkType
	{
		get
		{
			if (!(Sink is MtgCardInstance mtgCardInstance))
			{
				return ResourceSinkType.Pool;
			}
			MtgZone zone = mtgCardInstance.Zone;
			if (zone == null || zone.Type != ZoneType.Stack)
			{
				return ResourceSinkType.Card;
			}
			return ResourceSinkType.Stack;
		}
	}

	public bool IsFueling => Sink is MtgCardInstance;

	public ManaColor Color => Mana?.Color ?? ManaColor.None;

	public uint SubstitutionGrpId => Mana?.SubsitutionGrpId ?? 0;

	public ManaMovementData(MtgEntity source, MtgEntity sink, MtgMana mana)
	{
		Source = source;
		Sink = sink;
		Mana = mana;
	}
}
