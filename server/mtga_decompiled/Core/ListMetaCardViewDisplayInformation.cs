using System;
using GreClient.CardData;

public class ListMetaCardViewDisplayInformation
{
	public CardPrintingData Card;

	public CardPrintingData VisualCard;

	public string SkinCode;

	public uint Quantity;

	public bool Banned;

	public bool Invalid;

	public bool Unowned;

	public bool Deprioritized;

	public bool Suggested;

	public bool CosmeticOwned;

	public AbilityHangerData[] ContextualHangers;

	public override bool Equals(object obj)
	{
		if (obj is ListMetaCardViewDisplayInformation listMetaCardViewDisplayInformation)
		{
			if (Card != null && Card == listMetaCardViewDisplayInformation.Card && VisualCard == listMetaCardViewDisplayInformation.VisualCard && SkinCode == listMetaCardViewDisplayInformation.SkinCode && Quantity == listMetaCardViewDisplayInformation.Quantity && Banned == listMetaCardViewDisplayInformation.Banned && Invalid == listMetaCardViewDisplayInformation.Invalid && Unowned == listMetaCardViewDisplayInformation.Unowned && Deprioritized == listMetaCardViewDisplayInformation.Deprioritized && Suggested == listMetaCardViewDisplayInformation.Suggested && CosmeticOwned == listMetaCardViewDisplayInformation.CosmeticOwned)
			{
				return ContextualHangers == listMetaCardViewDisplayInformation.ContextualHangers;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(Card);
		hashCode.Add(VisualCard);
		hashCode.Add(SkinCode);
		hashCode.Add(Quantity);
		hashCode.Add(Banned);
		hashCode.Add(Invalid);
		hashCode.Add(Unowned);
		hashCode.Add(Deprioritized);
		hashCode.Add(Suggested);
		hashCode.Add(CosmeticOwned);
		hashCode.Add(ContextualHangers);
		return hashCode.ToHashCode();
	}

	public bool IsInstanceOf(ListMetaCardViewDisplayInformation other)
	{
		if (Card != null && Card == other.Card && VisualCard == other.VisualCard && SkinCode == other.SkinCode && Banned == other.Banned && Invalid == other.Invalid && Unowned == other.Unowned && Deprioritized == other.Deprioritized && Suggested == other.Suggested && CosmeticOwned == other.CosmeticOwned)
		{
			return ContextualHangers == other.ContextualHangers;
		}
		return false;
	}

	public ListMetaCardViewDisplayInformation Clone()
	{
		return new ListMetaCardViewDisplayInformation
		{
			Card = Card,
			VisualCard = VisualCard,
			SkinCode = SkinCode,
			Quantity = Quantity,
			Banned = Banned,
			Invalid = Invalid,
			Unowned = Unowned,
			Deprioritized = Deprioritized,
			Suggested = Suggested,
			CosmeticOwned = CosmeticOwned,
			ContextualHangers = ContextualHangers
		};
	}
}
