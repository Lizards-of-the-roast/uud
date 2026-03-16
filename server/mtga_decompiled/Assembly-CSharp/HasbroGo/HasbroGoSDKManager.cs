using System.Net.Http;
using HasbroGo.Config;
using HasbroGo.TestSupport;
using HasbroGoUnity;
using UnityEngine;

namespace HasbroGo;

public class HasbroGoSDKManager : MonoBehaviour
{
	public delegate void ShutDown();

	[SerializeField]
	public bool UseProductionEnvironment;

	[SerializeField]
	private ClientCredentials PreProductionCredentials;

	[SerializeField]
	private ClientCredentials ProductionCredentials;

	private Configuration _config;

	[SerializeField]
	private bool DoTestAccessRevoked;

	public static HasbroGoSDKManager Instance { get; private set; }

	public HasbroGoUnitySDK HasbroGoSdk { get; private set; }

	public event ShutDown OnShutdownEvent;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Object.Destroy(this);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad(this);
		string baseUrl = (UseProductionEnvironment ? Configuration.ProdBaseUrl : Configuration.DevBaseUrl);
		string baseWizMessageBusUrl = (UseProductionEnvironment ? Configuration.ProdBaseWizMessageBusUrl : Configuration.DevBaseWizMessageBusUrl);
		string clientID = (UseProductionEnvironment ? ProductionCredentials.ClientId : PreProductionCredentials.ClientId);
		string clientSecret = (UseProductionEnvironment ? ProductionCredentials.ClientSecret : PreProductionCredentials.ClientSecret);
		_config = new Configuration(baseUrl, baseWizMessageBusUrl, clientID, clientSecret, new PatronusLogger());
		_config.PersistentDataPath = Application.persistentDataPath;
		HasbroGoSdk = new HasbroGoUnitySDK(_config);
		HasbroGoSdk.AddService(Service.Accounts);
		HasbroGoSdk.AddService(Service.Wands);
		HasbroGoSdk.AddService(Service.Social);
		HasbroGoSdk.AddService(Service.Entitlements);
		Application.quitting += OnShutDown;
		if (DoTestAccessRevoked)
		{
			Invoke("DisableAuthentication", 20f);
		}
	}

	private void OnShutDown()
	{
		this.OnShutdownEvent();
		HasbroGoSdk.Shutdown();
	}

	private void DisableAuthentication()
	{
		BearerTokenExpiredHttpServiceStub handler = new BearerTokenExpiredHttpServiceStub();
		_config.HttpClient = new HttpClient(handler);
		HasbroGoSdk.AccountsService.GetProfile();
		_config.HttpClient = new HttpClient();
	}
}
