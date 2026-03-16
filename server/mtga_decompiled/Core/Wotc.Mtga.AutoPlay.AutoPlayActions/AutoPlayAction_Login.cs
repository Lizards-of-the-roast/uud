using System.IO;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_Login : AutoPlayAction
{
	private AutoPlayScript _nestedScript;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		string text = AutoPlayAction.FromParameter(in parameters, index + 1);
		if (ParseDataFromCredFile(text, out var user, out var pass))
		{
			Log("Logging in as " + user + " withcredentials from file " + text);
			string[] fileContents = new string[3]
			{
				"InputText|Login_Email_Field|" + user,
				"InputText|Login_PW_Field|" + pass,
				"Click|Login_Login_Button"
			};
			_nestedScript = new AutoPlayScript(in fileContents, LogAction, ComponentGetters, _autoplayManager);
		}
	}

	private static bool ParseDataFromCredFile(string fileName, out string user, out string pass)
	{
		user = string.Empty;
		pass = string.Empty;
		if (string.IsNullOrEmpty(fileName))
		{
			return false;
		}
		string path = Path.Combine(new DirectoryInfo(AutoPlayManager.GetConfigRoot).ToString(), fileName);
		if (File.Exists(path))
		{
			string text = File.ReadAllText(path);
			if (!string.IsNullOrWhiteSpace(text))
			{
				string[] array = text.Split('\n');
				if (array.Length == 2)
				{
					if (array[0].Contains("user="))
					{
						user = array[0].Substring("user=".Length).Trim();
					}
					if (array[1].Contains("pass="))
					{
						pass = array[1].Substring("pass=".Length).Trim();
					}
					return true;
				}
			}
		}
		return false;
	}

	protected override void OnUpdate()
	{
		if (ProcessNestedScript(_nestedScript))
		{
			Complete("Successfully logged in");
		}
	}
}
