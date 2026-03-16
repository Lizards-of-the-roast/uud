using System;
using System.Collections.Generic;
using WorkflowVisuals;

namespace Wotc.Mtga.DuelScene;

public class NullHighlightManager : IHighlightManager, IHighlightProvider, IHighlightController
{
	public static readonly IHighlightManager Default = new NullHighlightManager();

	private static readonly IHighlightProvider _provider = NullHighlightProvider.Default;

	private static readonly IHighlightController _controller = NullHighlightController.Default;

	public event Action HighlightsUpdated
	{
		add
		{
		}
		remove
		{
		}
	}

	public HighlightType GetHighlightForId(uint id)
	{
		return _provider.GetHighlightForId(id);
	}

	public void SetDirty()
	{
		_controller.SetDirty();
	}

	public void UpdateHighlights(IEnumerable<DuelScene_CDC> allCards, IEnumerable<DuelScene_AvatarView> allAvatars)
	{
		_controller.UpdateHighlights(allCards, allAvatars);
	}

	public void SetForceHighlightsOff(bool forceHighlightsOff)
	{
		_controller.SetForceHighlightsOff(forceHighlightsOff);
	}

	public void SetBrowserHighlights(Dictionary<DuelScene_CDC, HighlightType> highlights, bool ignoreWorkflowHighlights = true)
	{
		_controller.SetBrowserHighlights(highlights, ignoreWorkflowHighlights);
	}

	public void SetWorkflowHighlights(Highlights workflowHighlights)
	{
		_controller.SetWorkflowHighlights(workflowHighlights);
	}

	public void SetUserHighlights(Dictionary<uint, HighlightType> highlights)
	{
		_controller.SetUserHighlights(highlights);
	}
}
