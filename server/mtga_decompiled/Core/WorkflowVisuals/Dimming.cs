using System.Collections.Generic;

namespace WorkflowVisuals;

public class Dimming
{
	public Dictionary<uint, bool> IdToIsDimmed = new Dictionary<uint, bool>();

	public bool WorkflowActive = true;

	public static void Merge(Dimming lhs, Dimming rhs)
	{
		foreach (uint key in rhs.IdToIsDimmed.Keys)
		{
			bool flag = rhs.IdToIsDimmed[key];
			bool value = false;
			if (lhs.IdToIsDimmed.TryGetValue(key, out value))
			{
				if (value != flag && value && !flag)
				{
					lhs.IdToIsDimmed[key] = flag;
				}
			}
			else
			{
				lhs.IdToIsDimmed.Add(key, flag);
			}
		}
		if (rhs.WorkflowActive && !lhs.WorkflowActive)
		{
			lhs.WorkflowActive = rhs.WorkflowActive;
		}
	}
}
