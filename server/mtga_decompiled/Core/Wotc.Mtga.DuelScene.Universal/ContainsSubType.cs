using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Universal;

[Serializable]
public class ContainsSubType : GroupValidator
{
	[SerializeField]
	private SubType _subType;

	protected override bool Evaluate(ValidatorBlackboard blackboard)
	{
		IEnumerable<SubType> subtypes = blackboard.CardData.Subtypes;
		IEnumerable<SubType> source = subtypes ?? Enumerable.Empty<SubType>();
		if (_subType == SubType.None)
		{
			if (source.Count() != 0)
			{
				return source.Contains(SubType.None);
			}
			return true;
		}
		return source.Contains(_subType);
	}

	public override string ToString()
	{
		return (ExpectedValue ? "Is" : "Not") + ": " + EnumExtensions.EnumCleanName(_subType);
	}
}
