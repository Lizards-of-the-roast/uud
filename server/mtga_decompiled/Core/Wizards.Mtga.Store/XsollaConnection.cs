using System;
using UnityEngine.UI;

namespace Wizards.Mtga.Store;

public abstract class XsollaConnection : IDisposable
{
	protected const string SANDBOX_EXTERNAL_UI = "https://sandbox-secure.xsolla.com/paystation4/?access_token=";

	protected const string EXTERNAL_UI = "https://secure.xsolla.com/paystation4/?access_token=";

	protected TransactionType _transactionType;

	public event Action OnPurchaseCompleted;

	public XsollaConnection(TransactionType transactionType)
	{
		_transactionType = transactionType;
		Button closeButton = LoadPurchasePromptPrefab().CloseButton;
		closeButton.onClick.RemoveAllListeners();
		closeButton.onClick.AddListener(OnUnityCloseButtonClicked);
	}

	public virtual void Dispose()
	{
	}

	protected virtual void OnUnityCloseButtonClicked()
	{
		this.OnPurchaseCompleted?.Invoke();
	}

	public virtual void OpenBrowser(string xsollaToken, string redirect)
	{
	}

	protected virtual XsollaPurchasePrompt LoadPurchasePromptPrefab()
	{
		return null;
	}

	protected string CreateURL(string xsollaToken, string redirect)
	{
		if (string.IsNullOrEmpty(redirect))
		{
			return ((_transactionType == TransactionType.Sandbox) ? "https://sandbox-secure.xsolla.com/paystation4/?access_token=" : "https://secure.xsolla.com/paystation4/?access_token=") + xsollaToken;
		}
		return redirect;
	}
}
