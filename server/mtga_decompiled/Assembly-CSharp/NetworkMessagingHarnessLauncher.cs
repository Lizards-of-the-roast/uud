using System.Collections;
using Core.Shared.Code.Connection;
using Core.Shared.Code.Providers;
using Core.Shared.Code.Utilities;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

public class NetworkMessagingHarnessLauncher : MonoBehaviour
{
	private FrontDoorConnectionAWS _frontDoorConnection;

	private void Awake()
	{
		LoggingUtils.Initialize();
		Pantry.Get<EnvironmentManager>().InitializeEnvironment();
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		IAccountClient accountClient = Pantry.Get<IAccountClient>();
		accountClient.SetCredentials(currentEnvironment);
		FrontDoorConnectionManager frontDoorConnectionManager = Pantry.Get<FrontDoorConnectionManager>();
		frontDoorConnectionManager.SetEnvironment(currentEnvironment, accountClient);
		_frontDoorConnection = Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS;
		StartCoroutine(TryLoginAndConnectToFrontDoor(frontDoorConnectionManager));
	}

	public void Update()
	{
		_frontDoorConnection?.ProcessMessages();
	}

	private static IEnumerator TryLoginAndConnectToFrontDoor(FrontDoorConnectionManager fdConnectionManager)
	{
		ConnectionManager connectionManager = Pantry.Get<ConnectionManager>();
		new InitializeConnectionCommand().ExecuteFromPantry();
		yield return connectionManager.RingDoorbell();
		AutoLoginState autoLoginState = AutoLoginState.None;
		yield return fdConnectionManager.TryFastLogIn(delegate(AutoLoginState x)
		{
			autoLoginState = x;
		});
		if (autoLoginState != AutoLoginState.Connected)
		{
			Debug.LogWarning("Failed to auto log in. Try doing so manually.");
		}
		else
		{
			Debug.Log("Successfully autologged in!");
		}
	}
}
