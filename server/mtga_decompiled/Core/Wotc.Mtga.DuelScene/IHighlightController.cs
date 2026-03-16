using System.Collections.Generic;
using WorkflowVisuals;

namespace Wotc.Mtga.DuelScene;

public interface IHighlightController
{
	void SetDirty();

	void UpdateHighlights(IEnumerable<DuelScene_CDC> allCards, IEnumerable<DuelScene_AvatarView> allAvatars);

	void SetForceHighlightsOff(bool forceHighlightsOff);

	void SetBrowserHighlights(Dictionary<DuelScene_CDC, HighlightType> highlights, bool ignoreWorkflowHighlights = true);

	void SetWorkflowHighlights(Highlights workflowHighlights);

	void SetUserHighlights(Dictionary<uint, HighlightType> highlights);
}
