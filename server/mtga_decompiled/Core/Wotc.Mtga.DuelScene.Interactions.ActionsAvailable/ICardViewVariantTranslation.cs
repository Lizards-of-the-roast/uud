using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public interface ICardViewVariantTranslation
{
	WorkflowVariant Translate(DuelScene_CDC cardView, List<GreInteraction> interactions);
}
