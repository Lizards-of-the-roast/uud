using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Payloads;

public class DelayedTriggerAdditionalDetailsProvider : ILocParameterProvider
{
	public string LocParameterKey = "detail";

	public string Detail = "";

	public string GetKey()
	{
		return LocParameterKey;
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.CardData != null && filledBB.GameState?.DelayedTriggerAffectees != null)
		{
			paramValue = FindDelayedTriggerAdditionalDetails(filledBB);
		}
		return !string.IsNullOrEmpty(paramValue);
	}

	private string FindDelayedTriggerAdditionalDetails(IBlackboard filledBB)
	{
		foreach (DelayedTriggerData delayedTriggerAffectee in filledBB.GameState.DelayedTriggerAffectees)
		{
			foreach (KeyValuePairInfo detail in delayedTriggerAffectee.Details)
			{
				if (detail.Key == Detail)
				{
					string result = string.Empty;
					switch (detail.Type)
					{
					case KeyValuePairValueType.Bool:
						result = detail.ValueBool[0].ToString();
						break;
					case KeyValuePairValueType.Double:
						result = detail.ValueDouble[0].ToString();
						break;
					case KeyValuePairValueType.Float:
						result = detail.ValueFloat[0].ToString();
						break;
					case KeyValuePairValueType.Int32:
						result = detail.ValueInt32[0].ToString();
						break;
					case KeyValuePairValueType.Int64:
						result = detail.ValueInt64[0].ToString();
						break;
					case KeyValuePairValueType.String:
						result = detail.ValueString[0];
						break;
					case KeyValuePairValueType.Uint32:
						result = detail.ValueUint32[0].ToString();
						break;
					case KeyValuePairValueType.Uint64:
						result = detail.ValueUint64[0].ToString();
						break;
					}
					return result;
				}
			}
		}
		return string.Empty;
	}
}
