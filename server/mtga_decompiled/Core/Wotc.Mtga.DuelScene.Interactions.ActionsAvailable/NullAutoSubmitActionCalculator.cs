using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class NullAutoSubmitActionCalculator : IAutoSubmitActionCalculator
{
	public static readonly IAutoSubmitActionCalculator Default = new NullAutoSubmitActionCalculator();

	public GreInteraction GetAutoSubmitAction(IEnumerable<GreInteraction> actions)
	{
		return null;
	}
}
