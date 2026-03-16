using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public interface IWorkflowTranslation<T> where T : BaseUserRequest
{
	WorkflowBase Translate(T req);
}
