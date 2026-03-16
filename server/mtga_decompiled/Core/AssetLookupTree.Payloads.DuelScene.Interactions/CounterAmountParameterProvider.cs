using GreClient.Rules;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class CounterAmountParameterProvider : IPromptParameterProvider
{
	public string CounterAmount;

	public string GetKey()
	{
		return "amount";
	}

	public bool TryGetValue(BaseUserRequest request, GameManager gameManager, out string paramValue)
	{
		paramValue = CounterAmount;
		return true;
	}
}
