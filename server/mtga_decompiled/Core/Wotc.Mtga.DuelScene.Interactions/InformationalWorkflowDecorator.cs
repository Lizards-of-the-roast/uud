using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public class InformationalWorkflowDecorator : IWorkflowTranslator
{
	private readonly IWorkflowTranslator _nestedTranslation;

	private readonly IWorkflowTranslation<BaseUserRequest> _informationalTranslation;

	public InformationalWorkflowDecorator(IWorkflowTranslation<BaseUserRequest> informationalTranslation, IWorkflowTranslator nestedTranslation)
	{
		_informationalTranslation = informationalTranslation ?? NullWorkflowTranslation<BaseUserRequest>.Default;
		_nestedTranslation = nestedTranslation;
	}

	public WorkflowBase Translate(BaseUserRequest req)
	{
		if (!req.Informational)
		{
			return _nestedTranslation.Translate(req);
		}
		return _informationalTranslation.Translate(req);
	}
}
