using System.Threading.Tasks;
using HasbroGo.Accounts.Models.Requests;
using HasbroGo.Accounts.Models.Responses;
using HasbroGo.Errors;
using TMPro;
using UnityEngine;

namespace HasbroGo;

public class RegistrationMenu : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI results;

	[SerializeField]
	private TMP_InputField email;

	[SerializeField]
	private TMP_InputField birthday;

	[SerializeField]
	private TMP_InputField country;

	private bool RegistrationInProgress { get; set; }

	public void RegisterAccount()
	{
		if (RegistrationInProgress)
		{
			return;
		}
		results.text = "Registration Started";
		if (HasbroGoSDKManager.Instance.HasbroGoSdk == null || HasbroGoSDKManager.Instance.HasbroGoSdk.AccountsService == null)
		{
			return;
		}
		RegistrationInProgress = true;
		RegisterWASRequest registerWASRequest = new RegisterWASRequest
		{
			Email = email.text,
			Password = "Password",
			FirstName = "FirstName",
			LastName = "LastName",
			DisplayName = "DisplayName",
			DateOfBirth = birthday.text,
			CountryCode = country.text,
			AcceptedTC = true,
			EmailOptIn = true,
			DataShareOptIn = true,
			TargetedAnalyticsOptOut = false,
			DryRun = false
		};
		Task<Result<RegisterWASResponse, Error>> resultWAS = HasbroGoSDKManager.Instance.HasbroGoSdk.AccountsService.RegisterWAS(registerWASRequest);
		resultWAS.ContinueWith(delegate
		{
			RegistrationInProgress = false;
			if (resultWAS.Result.IsOk)
			{
				results.text = resultWAS.Result.Value.DisplayName;
			}
			else
			{
				results.text = resultWAS.Result.Error.Message;
			}
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}
}
