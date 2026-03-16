using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_Include : AutoPlayAction
{
	private AutoPlayScript _includedScript;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		string text = AutoPlayAction.FromParameter(in parameters, index + 1);
		Timeout = AutoPlayAction.FromParameter(in parameters, index + 2)?.IntoFloat() ?? Timeout;
		text = text.Replace("\\", "/");
		_includedScript = AutoPlayScriptFactory.CreateAutoPlayScript(text, LogAction, ComponentGetters, _autoplayManager);
		if (_includedScript == null)
		{
			Fail("Cannot find include script: " + text);
		}
	}

	protected override void OnUpdate()
	{
		if (ProcessNestedScript(_includedScript))
		{
			Complete("Successfully executed " + _includedScript.Name);
		}
	}
}
