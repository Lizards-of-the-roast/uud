using System.Collections.Generic;
using WorkflowVisuals;

namespace Wotc.Mtga.DuelScene;

public class NullHighlightController : IHighlightController
{
	public static readonly IHighlightController Default = new NullHighlightController();

	public void SetDirty()
	{
	}

	public void UpdateHighlights(IEnumerable<DuelScene_CDC> allCards, IEnumerable<DuelScene_AvatarView> allAvatars)
	{
	}

	public void SetForceHighlightsOff(bool forceHighlightsOff)
	{
	}

	public void SetBrowserHighlights(Dictionary<DuelScene_CDC, HighlightType> highlights, bool ignoreWorkflowHighlights = true)
	{
	}

	public void SetWorkflowHighlights(Highlights workflowHighlights)
	{
	}

	public void SetUserHighlights(Dictionary<uint, HighlightType> highlights)
	{
	}
}
