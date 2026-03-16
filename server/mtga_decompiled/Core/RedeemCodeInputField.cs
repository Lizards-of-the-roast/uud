using System.Collections;
using Core.Code.Promises;
using TMPro;
using UnityEngine;
using WAS;
using Wizards.Arena.Promises;
using Wotc.Mtga.Loc;

public class RedeemCodeInputField : MonoBehaviour
{
	private TMP_InputField InputField;

	private bool _currentlyRedeeming;

	private void Start()
	{
		InputField = GetComponentInChildren<TMP_InputField>();
		if ((bool)InputField)
		{
			InputField.onSubmit.AddListener(OnRedeem);
		}
	}

	private void OnEnable()
	{
		if ((bool)InputField)
		{
			InputField.text = "";
		}
	}

	private void OnDestroy()
	{
		if ((bool)InputField)
		{
			InputField.onSubmit.RemoveListener(OnRedeem);
		}
	}

	public void OnRedeem(string codeToRedeem)
	{
		if (!_currentlyRedeeming && !string.IsNullOrEmpty(codeToRedeem))
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
			StartCoroutine(RedeemCode(codeToRedeem).WithLoadingIndicator());
		}
	}

	private IEnumerator RedeemCode(string redeemedCode)
	{
		_currentlyRedeeming = true;
		string trimmedCode = redeemedCode.Trim();
		bool checkEntitlements = false;
		yield return WrapperController.Instance.AccountClient.RedeemCode(trimmedCode).IfSuccess(delegate
		{
			checkEntitlements = true;
			MainThreadDispatcher.Instance.Add(HandleRedeemCodeSuccess);
		}).ThenOnMainThreadIfError(delegate(Error e)
		{
			HandleRedeemCodeError(WASUtils.ToAccountError(e), trimmedCode);
		})
			.AsCoroutine();
		if (checkEntitlements)
		{
			yield return WrapperController.Instance.Store.GetEntitlements(shouldRetry: true).AsCoroutine();
		}
		_currentlyRedeeming = false;
	}

	private void HandleRedeemCodeSuccess()
	{
		InputField.text = "";
		SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Redeem_Code_Success_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Redeem_Code_Success_Text"));
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, AudioManager.Default);
	}

	private static void HandleRedeemCodeError(AccountError error, string trimmedCode)
	{
		string text = error.ErrorCode switch
		{
			400 => (!(error.Message == "OFFER EXPIRED")) ? Languages.ActiveLocProvider.GetLocalizedText("MainNav/CodeAlreadyRedeemed") : Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Redeem_Code_Failure_Text", ("codeId", trimmedCode)), 
			404 => Languages.ActiveLocProvider.GetLocalizedText("MainNav/CodeNotFound"), 
			_ => Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Redeem_Code_Failure_Text", ("codeId", trimmedCode)), 
		};
		SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Redeem_Code_Failure_Title"), text);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid, AudioManager.Default);
	}
}
