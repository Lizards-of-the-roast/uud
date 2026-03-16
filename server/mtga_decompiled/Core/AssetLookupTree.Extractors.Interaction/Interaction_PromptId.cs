using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Interaction;

public class Interaction_PromptId : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb?.Interaction?.Prompt == null)
		{
			return false;
		}
		value = (int)bb.Interaction.Prompt.PromptId;
		return true;
	}
}
