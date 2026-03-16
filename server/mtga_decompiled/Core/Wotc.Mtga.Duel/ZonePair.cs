using System;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Duel;

public readonly struct ZonePair : IEquatable<ZonePair>
{
	public readonly ZoneType FromZone;

	public readonly CardHolderType FromHolder;

	public readonly GREPlayerNum FromOwner;

	public readonly ZoneType ToZone;

	public readonly CardHolderType ToHolder;

	public readonly GREPlayerNum ToOwner;

	public ZonePair(ZoneType fromZone, GREPlayerNum fromOwner, ZoneType toZone, GREPlayerNum toOwner)
	{
		FromZone = fromZone;
		FromHolder = fromZone.ToCardHolderType();
		FromOwner = fromOwner;
		ToZone = toZone;
		ToHolder = toZone.ToCardHolderType();
		ToOwner = toOwner;
	}

	public ZonePair(CardHolderType fromHolder, GREPlayerNum fromOwner, CardHolderType toHolder, GREPlayerNum toOwner)
	{
		FromZone = fromHolder.ToZoneType();
		FromHolder = fromHolder;
		FromOwner = fromOwner;
		ToZone = toHolder.ToZoneType();
		ToHolder = toHolder;
		ToOwner = toOwner;
	}

	public ZonePair(MtgZone from, MtgZone to)
	{
		ExtractDetails(from, out FromZone, out FromHolder, out FromOwner);
		ExtractDetails(to, out ToZone, out ToHolder, out ToOwner);
	}

	public ZonePair(ICardHolder from, ICardHolder to)
	{
		ExtractDetails(from, out FromZone, out FromHolder, out FromOwner);
		ExtractDetails(to, out ToZone, out ToHolder, out ToOwner);
	}

	private static void ExtractDetails(MtgZone zone, out ZoneType zoneType, out CardHolderType cardHolderType, out GREPlayerNum ownerNum)
	{
		if (zone != null)
		{
			zoneType = zone.Type;
			cardHolderType = zoneType.ToCardHolderType();
			ownerNum = zone.OwnerNum;
		}
		else
		{
			zoneType = ZoneType.None;
			cardHolderType = CardHolderType.Invalid;
			ownerNum = GREPlayerNum.Invalid;
		}
	}

	private static void ExtractDetails(ICardHolder cardHolder, out ZoneType zoneType, out CardHolderType cardHolderType, out GREPlayerNum ownerNum)
	{
		if (cardHolder is ZoneCardHolderBase zoneCardHolderBase)
		{
			zoneType = zoneCardHolderBase.GetZoneType;
			cardHolderType = zoneCardHolderBase.CardHolderType;
			ownerNum = zoneCardHolderBase.PlayerNum;
		}
		else if (cardHolder != null)
		{
			zoneType = cardHolder.CardHolderType.ToZoneType();
			cardHolderType = cardHolder.CardHolderType;
			ownerNum = cardHolder.PlayerNum;
		}
		else
		{
			zoneType = ZoneType.None;
			cardHolderType = CardHolderType.Invalid;
			ownerNum = GREPlayerNum.Invalid;
		}
	}

	public bool Equals(ZonePair other)
	{
		if (FromZone == other.FromZone && FromOwner == other.FromOwner && ToZone == other.ToZone)
		{
			return ToOwner == other.ToOwner;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ZonePair other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(((((uint)((int)FromZone * 397) ^ (uint)FromOwner) * 397) ^ (uint)ToZone) * 397) ^ (int)ToOwner;
	}

	public override string ToString()
	{
		return $"{FromOwner} {FromZone} ({FromHolder}) -> {ToOwner} {ToZone} ({ToHolder})";
	}
}
