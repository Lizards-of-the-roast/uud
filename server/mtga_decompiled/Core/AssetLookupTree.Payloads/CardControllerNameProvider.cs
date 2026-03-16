using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Payloads;

public class CardControllerNameProvider : ILocParameterProvider
{
	public string GetKey()
	{
		return "playerName";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.IdNameProvider != null && filledBB.CardData != null && filledBB.CardData.Controller != null)
		{
			paramValue = filledBB.IdNameProvider.GetName(filledBB.CardData.Controller.InstanceId);
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
