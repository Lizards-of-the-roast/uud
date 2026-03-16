using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Prompt;

public class Prompt_Id : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)(bb.Prompt?.PromptId ?? 0);
		return bb.Prompt != null;
	}
}
