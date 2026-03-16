namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_SetUserPref : AutoPlayAction_SetPlayerPref
{
	protected override void OnExecute()
	{
		string text = _autoplayManager.AccountClient?.AccountInformation?.PersonaID;
		_prefKey = text + "." + _prefKey;
		base.OnExecute();
	}
}
