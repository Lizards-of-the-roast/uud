using System;
using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

[Serializable]
public struct NumCardsOfTypeInZoneConstraintContainer
{
	[Serializable]
	public class InnerListOfNumCardsOfTypeInZoneConstraints
	{
		public List<NumCardsOfTypeInZoneConstraint> constraints;
	}

	[Serializable]
	public class NumCardsOfTypeInZoneConstraint
	{
		public Comparison comparison;

		public uint number;

		public CardType cardType;

		public Zone zone;

		public Controller controller;
	}

	public List<InnerListOfNumCardsOfTypeInZoneConstraints> constraints;
}
