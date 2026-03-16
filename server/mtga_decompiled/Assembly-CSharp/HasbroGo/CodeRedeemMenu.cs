using HasbroGo.Entitlements.Models.Requests;
using HasbroGo.Entitlements.Models.Results;
using HasbroGo.Errors;
using TMPro;
using UnityEngine;

namespace HasbroGo;

public class CodeRedeemMenu : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField codeEntry;

	[SerializeField]
	private TextMeshProUGUI resultsText;

	private void OnEnable()
	{
		resultsText.text = string.Empty;
	}

	public async void RedeemCode()
	{
		resultsText.text = string.Empty;
		HasbroGoSDK hasbroGoSdk = HasbroGoSDKManager.Instance.HasbroGoSdk;
		if (hasbroGoSdk == null || hasbroGoSdk.AccountsService == null)
		{
			resultsText.text = "Unable to find the SDK or Accounts Service.";
			return;
		}
		if (!hasbroGoSdk.AccountsService.IsLoggedIn())
		{
			resultsText.text = "User must be logged in before codes can be redeemed.";
			return;
		}
		if (string.IsNullOrEmpty(codeEntry.text))
		{
			resultsText.text = "Invalid redeem code";
			return;
		}
		RedeemCodeRequest redeemCodeRequest = new RedeemCodeRequest
		{
			Code = codeEntry.text
		};
		Result<RedeemCodeResult, Error> result = await hasbroGoSdk.EntitlementsService.RedeemCode(redeemCodeRequest);
		if (result.IsOk)
		{
			resultsText.text = result.Value.ToString();
		}
		else
		{
			resultsText.text = "Redeem Failure: " + result.Error.Message;
		}
	}
}
