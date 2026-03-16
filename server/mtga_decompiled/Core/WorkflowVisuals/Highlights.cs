using System.Collections.Generic;

namespace WorkflowVisuals;

public class Highlights
{
	public readonly Dictionary<uint, HighlightType> IdToHighlightType_Workflow = new Dictionary<uint, HighlightType>();

	public readonly Dictionary<uint, HighlightType> ManaIdToHighlightType = new Dictionary<uint, HighlightType>();

	public readonly Dictionary<uint, HighlightType> IdToHighlightType_User = new Dictionary<uint, HighlightType>();

	public readonly Dictionary<IEntityView, HighlightType> EntityHighlights = new Dictionary<IEntityView, HighlightType>();

	public void Clear()
	{
		IdToHighlightType_Workflow.Clear();
		ManaIdToHighlightType.Clear();
		IdToHighlightType_User.Clear();
		EntityHighlights.Clear();
	}

	public void Merge(Highlights other)
	{
		MergeInHighlights(IdToHighlightType_Workflow, other.IdToHighlightType_Workflow);
		MergeInHighlights(ManaIdToHighlightType, other.ManaIdToHighlightType);
		MergeInHighlights(IdToHighlightType_User, other.IdToHighlightType_User);
		MergeInHighlights(EntityHighlights, other.EntityHighlights);
	}

	private static void MergeInHighlights<T>(Dictionary<T, HighlightType> target, Dictionary<T, HighlightType> source)
	{
		foreach (KeyValuePair<T, HighlightType> item in source)
		{
			if (target.TryGetValue(item.Key, out var value))
			{
				if (value < item.Value)
				{
					target[item.Key] = item.Value;
				}
			}
			else
			{
				target.Add(item.Key, item.Value);
			}
		}
	}
}
