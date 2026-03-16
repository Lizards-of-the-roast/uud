using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class CounterTypeParameterProvider : IPromptParameterProvider
{
	public CounterType CounterType;

	public string GetKey()
	{
		return "type";
	}

	public bool TryGetValue(BaseUserRequest request, GameManager gameManager, out string paramValue)
	{
		paramValue = gameManager.CardDatabase.GreLocProvider.GetLocalizedTextForEnumValue("CounterType", (int)CounterType);
		return true;
	}
}
