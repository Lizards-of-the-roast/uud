using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Payloads;

public class OptionalMessageNumeralProvider : ILocParameterProvider
{
	public string GetKey()
	{
		return "numeral";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.Request != null && filledBB.Request.Prompt != null && filledBB.GameState != null)
		{
			foreach (PromptParameter parameter in filledBB.Request.Prompt.Parameters)
			{
				if (!filledBB.GameState.TryGetCard((uint)parameter.NumberValue, out var _))
				{
					paramValue = parameter.NumberValue.ToString();
					break;
				}
			}
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
