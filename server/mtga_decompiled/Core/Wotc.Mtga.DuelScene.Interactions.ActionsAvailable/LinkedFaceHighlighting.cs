using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using WorkflowVisuals;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public static class LinkedFaceHighlighting
{
	private static readonly IComparer<Action> _actionCostComparer = new ActionCostComparer();

	private static readonly IComparer<Action> _actionTypePriorityComparer = new ActionTypePriorityComparer();

	private static List<Action> _actionCache = new List<Action>();

	public static void SetHighlights(in Highlights highlights, IReadOnlyList<GreInteraction> interactions, MtgCardInstance cardInstance)
	{
		SetHighlights(in highlights, ConvertToActiveActions(interactions), cardInstance);
	}

	public static void SetHighlights(in Highlights highlights, IReadOnlyList<Action> actions, MtgCardInstance cardInstance)
	{
		MDFCHighlighting.SetHighlights(in highlights, actions, cardInstance);
	}

	private static List<Action> ConvertToActiveActions(IEnumerable<GreInteraction> interactions)
	{
		_actionCache.Clear();
		foreach (GreInteraction interaction in interactions)
		{
			if (interaction != null && interaction.IsActive)
			{
				Action greAction = interaction.GreAction;
				if (greAction != null)
				{
					_actionCache.Add(greAction);
				}
			}
		}
		_actionCache.Sort(_actionCostComparer);
		_actionCache.Sort(_actionTypePriorityComparer);
		return _actionCache;
	}
}
