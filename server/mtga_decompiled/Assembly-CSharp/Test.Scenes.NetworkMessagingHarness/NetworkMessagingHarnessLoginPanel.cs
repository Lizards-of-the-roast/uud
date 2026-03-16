using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Test.Scenes.NetworkMessagingHarness;

public class NetworkMessagingHarnessLoginPanel : MonoBehaviour
{
	[SerializeField]
	public Image LoginStatusIcon;

	[SerializeField]
	public Text LoginStatusText;

	[SerializeField]
	public GameObject LoginFormParent;

	[SerializeField]
	public InputField UsernameField;

	[SerializeField]
	public InputField PasswordField;

	private FDConnectionState? _lastKnownConnectionState;

	public void Start()
	{
		FDConnectionState connectionState = Pantry.Get<IFrontDoorConnectionServiceWrapper>().ConnectionState;
		SetLoginStatusUI(connectionState);
		_lastKnownConnectionState = connectionState;
	}

	public void Update()
	{
		FDConnectionState connectionState = Pantry.Get<IFrontDoorConnectionServiceWrapper>().ConnectionState;
		if (connectionState != _lastKnownConnectionState)
		{
			SetLoginStatusUI(connectionState);
			_lastKnownConnectionState = connectionState;
		}
	}

	public void SetLoginStatusUI(FDConnectionState connectionState)
	{
		switch (connectionState)
		{
		case FDConnectionState.Connected:
			LoginStatusIcon.color = new Color(0f, 200f, 0f);
			LoginStatusText.text = "Logged In";
			break;
		case FDConnectionState.Authenticating:
			LoginStatusIcon.color = new Color(222f, 230f, 85f);
			LoginStatusText.text = "Authenticating";
			break;
		case FDConnectionState.Connecting:
			LoginStatusIcon.color = new Color(242f, 188f, 94f);
			LoginStatusText.text = "Connecting";
			break;
		default:
			LoginStatusIcon.color = new Color(200f, 0f, 0f);
			LoginStatusText.text = "Logged Out";
			break;
		}
		LoginFormParent.UpdateActive(connectionState != FDConnectionState.Connected);
	}

	public void OnLoginButtonClicked()
	{
		string text = UsernameField.text;
		string text2 = PasswordField.text;
		StartCoroutine(AttemptLoginCoroutine(text, text2));
	}

	private IEnumerator AttemptLoginCoroutine(string username, string password)
	{
		IAccountClient accountClient = Pantry.Get<IAccountClient>();
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		IFrontDoorConnectionServiceWrapper connectionServiceWrapper = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
		yield return accountClient.LogIn_Credentials(username, password).Then(delegate(Promise<AccountInformation> acctInfo)
		{
			if (!acctInfo.Successful)
			{
				Debug.LogError("Failed to log in with these credentials");
			}
			else
			{
				FDCConnectionParams parameters = new FDCConnectionParams
				{
					Host = currentEnvironment.fdHost,
					Port = currentEnvironment.fdPort,
					SessionTicket = acctInfo.Result.Credentials.Jwt,
					IsDebugAccount = (acctInfo.Result.HasRole_Debugging() || Debug.isDebugBuild),
					AcceptsPolicy = () => true
				};
				connectionServiceWrapper.Connect(parameters);
				Debug.Log("Logged in and connected properly!");
			}
		}).AsCoroutine();
	}
}
