using System.Collections.Generic;
using WorkflowVisuals;

namespace Wotc.Mtga.DuelScene;

public interface IDimmingController
{
	void SetDirty();

	void SetBrowserDimming(Dictionary<DuelScene_CDC, bool> dimming, bool defaultState);

	void SetWorkflowDimming(Dimming workflowDimming);

	void UpdateDimming(IEnumerable<DuelScene_CDC> cardViews);
}
