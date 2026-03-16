using System.Collections.Generic;
using WorkflowVisuals;

namespace Wotc.Mtga.DuelScene;

public class NullDimmingController : IDimmingController
{
	public static readonly IDimmingController Default = new NullDimmingController();

	public void SetDirty()
	{
	}

	public void SetBrowserDimming(Dictionary<DuelScene_CDC, bool> dimming, bool defaultState)
	{
	}

	public void SetWorkflowDimming(Dimming workflowDimming)
	{
	}

	public void UpdateDimming(IEnumerable<DuelScene_CDC> cardViews)
	{
	}
}
