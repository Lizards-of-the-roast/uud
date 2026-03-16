using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Universal;

[Serializable]
public class ContainsCardType : GroupValidator
{
	[SerializeField]
	private CardType _cardType;

	protected override bool Evaluate(ValidatorBlackboard blackboard)
	{
		IEnumerable<CardType> cardTypes = blackboard.CardData.CardTypes;
		IEnumerable<CardType> source = cardTypes ?? Enumerable.Empty<CardType>();
		if (_cardType == CardType.None)
		{
			if (source.Count() != 0)
			{
				return source.Contains(CardType.None);
			}
			return true;
		}
		return source.Contains(_cardType);
	}

	public override string ToString()
	{
		return (ExpectedValue ? "Is" : "Not") + ": " + EnumExtensions.EnumCleanName(_cardType);
	}
}
