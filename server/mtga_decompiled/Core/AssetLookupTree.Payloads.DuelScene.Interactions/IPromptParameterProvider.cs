using GreClient.Rules;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public interface IPromptParameterProvider
{
	string GetKey();

	bool TryGetValue(BaseUserRequest request, GameManager gameManager, out string paramValue);
}
