using Wizards.Mtga.Assets;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_LoadScene : AutoPlayAction
{
	private string _scene;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_scene = AutoPlayAction.FromParameter(in parameters, index + 1);
	}

	protected override void OnExecute()
	{
		Scenes.LoadScene(_scene);
		Complete("Loaded scene " + _scene);
	}
}
