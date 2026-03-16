using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public interface ISelectionConfirmation
{
	string GetConfirmationText(HighlightType highlightType, IEntityView entityView, SelectNRequest request);
}
