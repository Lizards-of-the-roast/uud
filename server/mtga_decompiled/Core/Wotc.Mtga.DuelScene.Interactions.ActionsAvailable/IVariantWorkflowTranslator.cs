using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public interface IVariantWorkflowTranslator
{
	WorkflowVariant Translate(IEntityView entityView, List<GreInteraction> interactions);
}
