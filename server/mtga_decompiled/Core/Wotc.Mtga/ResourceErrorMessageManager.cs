using System.Linq;
using Core.Shared.Code.Connection;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga;

public class ResourceErrorMessageManager
{
	private bool _isErrorShowing;

	public static ResourceErrorMessageManager Create()
	{
		return new ResourceErrorMessageManager();
	}

	public void ShowError(string errorMessage, string error, params (string, string)[] details)
	{
		if (!_isErrorShowing)
		{
			string text = ((details == null) ? string.Empty : string.Join("\n", details.ToList().ConvertAll(((string, string) x) => x.Item1 + " -> " + x.Item2)));
			Debug.Log("Showing resource load error: " + errorMessage + "\n" + error + "\n" + text);
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Asset_Error_Title");
			string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Asset_Error_Body");
			_isErrorShowing = true;
			FrontDoorConnectionManager frontDoorConnectionManager = Pantry.Get<FrontDoorConnectionManager>();
			SystemMessageManager.Instance.ShowOk(localizedText, localizedText2, delegate
			{
				SystemMessageManager.Instance.ClearMessageQueue();
				frontDoorConnectionManager.RestartGame("Resource Load Error");
				_isErrorShowing = false;
			}, null, SystemMessageManager.SystemMessagePriority.FatalError);
		}
	}
}
