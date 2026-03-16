using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public static class ModalHighlighting
{
	public const uint ABILITY_ID_ASCENDANT_SPIRIT = 75081u;

	public static HighlightType HighlightForGreInteraction(GreInteraction interaction, MtgCardInstance instance)
	{
		MtgCardInstance instance2 = InstanceForInteractionHighlight(interaction, instance);
		if (interaction != null && interaction.IsActive)
		{
			return Highlighting.HighlightForAction(interaction.GreAction, instance2);
		}
		return HighlightType.None;
	}

	private static MtgCardInstance InstanceForInteractionHighlight(GreInteraction interaction, MtgCardInstance instance)
	{
		if (interaction == null || instance == null)
		{
			return null;
		}
		return InstanceForActionHighlight(interaction.GreAction, instance);
	}

	private static MtgCardInstance InstanceForActionHighlight(Action action, MtgCardInstance instance)
	{
		if (action == null || instance == null)
		{
			return null;
		}
		if (action.IsMDFCAction() && instance.TryGetBackInstance(out var backInstance))
		{
			return backInstance;
		}
		return instance;
	}
}
