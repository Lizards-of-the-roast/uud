using Wotc.Mtga.Loc;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_Language : AutoPlayAction
{
	private string _language;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_language = AutoPlayAction.FromParameter(in parameters, index + 1);
	}

	protected override void OnExecute()
	{
		Languages.CurrentLanguage = _language;
		Complete("Language switched to: " + _language);
	}
}
