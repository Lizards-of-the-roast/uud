using HasbroGo.Accounts.Profile;
using HasbroGo.Errors;
using HasbroGo.Helpers;
using HasbroGo.Logging;
using TMPro;
using UnityEngine;

namespace HasbroGo;

public class ExamplesMainMenu : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private GameObject copyButtonGO;

	private readonly string logCategory = "ExamplesMainMenu";

	private HasbroGoSDK sdk => HasbroGoSDKManager.Instance.HasbroGoSdk;

	private async void Awake()
	{
		Result<ProfileResponse, Error> result = await sdk.AccountsService.GetProfile();
		if (result.IsOk)
		{
			displayNameText.text = result.Value.DisplayName;
			return;
		}
		LogHelper.Logger.Log(logCategory, LogLevel.Error, "Failed to get profile for logged in user.");
		displayNameText.gameObject.SetActive(value: false);
		copyButtonGO.SetActive(value: false);
	}

	public void CopyDisplayNameToClipboard()
	{
		GUIUtility.systemCopyBuffer = displayNameText.text;
		LogHelper.Logger.Log(logCategory, LogLevel.Log, "Display Name copied to clipboard.");
	}
}
