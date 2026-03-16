using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class AutoSubmitActionCalculatorAggregate : IAutoSubmitActionCalculator
{
	private readonly IAutoSubmitActionCalculator[] _elements;

	public AutoSubmitActionCalculatorAggregate(params IAutoSubmitActionCalculator[] elements)
	{
		_elements = elements ?? Array.Empty<IAutoSubmitActionCalculator>();
	}

	public GreInteraction GetAutoSubmitAction(IEnumerable<GreInteraction> actions)
	{
		IAutoSubmitActionCalculator[] elements = _elements;
		for (int i = 0; i < elements.Length; i++)
		{
			GreInteraction autoSubmitAction = elements[i].GetAutoSubmitAction(actions);
			if (autoSubmitAction != null)
			{
				return autoSubmitAction;
			}
		}
		return null;
	}
}
