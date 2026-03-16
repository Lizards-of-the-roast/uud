namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_SetPlayerPref : AutoPlayAction
{
	protected string _prefKey;

	protected string _prefValue;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_prefKey = AutoPlayAction.FromParameter(in parameters, index + 1);
		_prefValue = AutoPlayAction.FromParameter(in parameters, index + 2) ?? string.Empty;
	}

	protected override void OnExecute()
	{
		int result;
		float result2;
		bool result3;
		if (string.IsNullOrWhiteSpace(_prefValue))
		{
			PlayerPrefsExt.DeleteKey(_prefKey);
			PlayerPrefsExt.Save();
			Complete("Cleared Player Pref: " + _prefKey);
		}
		else if (int.TryParse(_prefValue, out result))
		{
			PlayerPrefsExt.SetInt(_prefKey, result);
			PlayerPrefsExt.Save();
			Complete($"Set Player Pref Int: {_prefKey} to {result}");
		}
		else if (float.TryParse(_prefValue, out result2))
		{
			PlayerPrefsExt.SetFloat(_prefKey, result2);
			PlayerPrefsExt.Save();
			Complete($"Set Player Pref Float: {_prefKey} to {result2}");
		}
		else if (bool.TryParse(_prefValue, out result3))
		{
			PlayerPrefsExt.SetBool(_prefKey, result3);
			PlayerPrefsExt.Save();
			Complete($"Set Player Pref Bool: {_prefKey} to {result3}");
		}
		else
		{
			PlayerPrefsExt.SetString(_prefKey, _prefValue);
			PlayerPrefsExt.Save();
			Complete("Set Player Pref String: " + _prefKey + " to " + _prefValue);
		}
	}
}
