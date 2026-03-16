using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class NullSelectionConfirmation : ISelectionConfirmation
{
	public static readonly ISelectionConfirmation Default = new NullSelectionConfirmation();

	public string GetConfirmationText(HighlightType highlightType, IEntityView entityView, SelectNRequest request)
	{
		return null;
	}
}
