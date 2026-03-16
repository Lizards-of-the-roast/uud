using System.Collections.Generic;
using AssetLookupTree.Payloads.General;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class PromptPayload : LocKey
{
	public readonly List<IPromptParameterProvider> ParameterProviders = new List<IPromptParameterProvider>();
}
