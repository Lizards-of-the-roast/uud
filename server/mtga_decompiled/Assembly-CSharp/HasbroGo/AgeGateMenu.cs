using System.Threading.Tasks;
using HasbroGo.Accounts;
using HasbroGo.Accounts.Models.Requests;
using HasbroGo.Errors;
using TMPro;
using UnityEngine;

namespace HasbroGo;

public class AgeGateMenu : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI results;

	[SerializeField]
	private TMP_InputField birthday;

	[SerializeField]
	private TMP_InputField country;

	private bool AgeGateCheckInProgress;

	public void AgeGateCheck()
	{
		if (AgeGateCheckInProgress)
		{
			return;
		}
		AgeGateCheckInProgress = true;
		AgeGateCheckRequest ageGateCheckRequest = new AgeGateCheckRequest
		{
			DateOfBirth = birthday.text,
			CountryCode = country.text
		};
		Task<Result<AgeGateCheckResult, Error>> resultWAS = HasbroGoSDKManager.Instance.HasbroGoSdk.AccountsService.AgeGateCheck(ageGateCheckRequest);
		resultWAS.ContinueWith(delegate
		{
			AgeGateCheckInProgress = false;
			if (resultWAS.Result.IsOk)
			{
				results.text = (resultWAS.Result.Value.RequiresAgeGate ? "True: Needs age gate" : "False: Doesn't need age gate");
			}
			else
			{
				results.text = resultWAS.Result.Error.Message;
			}
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}
}
