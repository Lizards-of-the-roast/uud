using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Prompt;

public class PromptParameter_Id : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.PromptParameterId;
		return true;
	}
}
