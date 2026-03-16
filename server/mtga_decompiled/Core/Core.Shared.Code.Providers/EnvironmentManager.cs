using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Code.ClientFeatureToggle;
using Wizards.Mtga;
using Wizards.Mtga.Configuration;
using Wizards.Mtga.Platforms;

namespace Core.Shared.Code.Providers;

public class EnvironmentManager
{
	private const string ProdDoorbellUrl = "https://doorbellprod.w2.mtgarena.com";

	private ServicesConfiguration _servicesConfiguration;

	public Action OnEnvironmentSet;

	private static readonly EnvironmentDescription HardcodedProd = new EnvironmentDescription
	{
		name = "Prod",
		fdHost = string.Empty,
		fdPort = 0,
		ecoUri = "https://api.platform.wizards.com/eco",
		accountSystemId = "N8QFG8NEBJ5T35FB",
		accountSystemSecret = "VMK1RE8YK6YR4EABJU91",
		accountSystemEnvironment = EnvironmentType.Prod,
		epicWASClientId = "2186e6b404a54e6fa062c4f37febb22d",
		epicWASClientSecret = "12fd208919fa46cdaebb797174a05c25",
		steamClientId = "AW2T7QJ7ZJEAXGOQUUYVEK5PYE",
		steamClientSecret = "AFSSE6DSIJGHRPCY3QICRK57BQ",
		xsollaTransactionType = "Production",
		doorbellUri = "https://doorbellprod.w2.mtgarena.com",
		HostPlatform = HostPlatform.AWS,
		bikeUri = "https://bike.w2.mtgarena.com/BusinessEvent/event"
	};

	public AssetsConfiguration AssetsConfiguration { get; private set; }

	private List<EnvironmentDescription> Environments { get; set; }

	public static EnvironmentManager Create()
	{
		return new EnvironmentManager();
	}

	private EnvironmentManager()
	{
		LoadConfigurations(PlatformContext.GetConfigurationLoader());
		Environments = _servicesConfiguration.ActiveEnvironments.Select(EnvironmentDescription.FromJObject).ToList();
	}

	public void InitializeEnvironment()
	{
		string previousEnvironment = MDNPlayerPrefs.PreviousFDServer;
		Pantry.CurrentEnvironment = Environments.Find((EnvironmentDescription x) => x.name == previousEnvironment) ?? ((Environments.Count > 0) ? Environments[0] : HardcodedProd);
	}

	private void LoadConfigurations(IConfigurationLoader loader)
	{
		Task<AssetsConfiguration> task = loader.LoadAssetsConfiguration();
		Task<ServicesConfiguration> task2 = loader.LoadServicesConfiguration();
		Task.WaitAll(task, task2);
		AssetsConfiguration = task.Result;
		_servicesConfiguration = task2.Result;
		if (loader is DefaultConfigurationLoader defaultConfigurationLoader)
		{
			OverridesConfiguration.Local.SetFeatureToggleValue("debug", defaultConfigurationLoader.AssetsConfigurationLoadedFromFile || defaultConfigurationLoader.ServicesConfigurationLoadedFromFile);
		}
	}

	public void InitializeEnvironmentSelector(Func<EnvironmentSelector> getEnvironmentSelector)
	{
		List<string> list = EnvironmentNames();
		if (list.Count > 1)
		{
			EnvironmentDescription environmentDescription = Pantry.CurrentEnvironment;
			if (environmentDescription == null)
			{
				environmentDescription = FindEnvironment("UseDoorbellClientVersionMap");
			}
			EnvironmentSelector environmentSelector = getEnvironmentSelector();
			if (environmentSelector != null)
			{
				environmentSelector.SetEnvironments(EnvironmentNames(), environmentDescription.name);
			}
			SetEnvironmentByName(environmentDescription.name, shouldDefault: false);
		}
		else if (list.Count == 1)
		{
			SetEnvironmentByName(list[0]);
		}
		else
		{
			SetEnvironmentByName("default-prod-environment-that-is-not-a-magic-string");
		}
	}

	public void SetEnvironmentByName(string envName, bool shouldDefault = true)
	{
		EnvironmentDescription environmentDescription = FindEnvironment(envName, shouldDefault: false);
		if (shouldDefault && environmentDescription == null)
		{
			environmentDescription = FindEnvironment(envName);
		}
		if (environmentDescription != null)
		{
			MDNPlayerPrefs.PreviousFDServer = environmentDescription.name;
			OnEnvironmentSet?.Invoke();
		}
	}

	public static string GetDoorbellUri(EnvironmentDescription env)
	{
		string text = env.doorbellUri;
		if (string.IsNullOrWhiteSpace(text))
		{
			return "";
		}
		if (!text.EndsWith('/'))
		{
			text += "/";
		}
		string ringDoorbellRoute = GetRingDoorbellRoute();
		return text + ringDoorbellRoute;
	}

	private static string GetRingDoorbellRoute()
	{
		if (!Pantry.Get<ClientFeatureToggleDataProvider>().GetToggleValueById("RingDoorbellV2"))
		{
			return "api/ring";
		}
		return "api/v2/ring";
	}

	public EnvironmentDescription FindEnvironment(string name, bool shouldDefault = true)
	{
		EnvironmentDescription environmentDescription = Environments.Find((EnvironmentDescription x) => x.name == name);
		if (environmentDescription == null && shouldDefault)
		{
			environmentDescription = ((Environments.Count <= 0) ? HardcodedProd : Environments[0]);
		}
		return environmentDescription;
	}

	public List<string> EnvironmentNames()
	{
		List<string> list = new List<string>(Environments.Count);
		foreach (EnvironmentDescription environment in Environments)
		{
			list.Add(environment.name);
		}
		return list;
	}

	public List<string> GetDebugEnvironmentNames()
	{
		return (from x in Environments
			where x.HostPlatform == HostPlatform.AWS
			where !string.IsNullOrEmpty(x.mdHost)
			where x.mdPort > 0
			select x.name).ToList();
	}
}
