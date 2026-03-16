using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Configuration;
using Wotc.Mtga;

public class WrapperEnvironmentLauncher : MonoBehaviour
{
	public string LaunchScene = "Bootstrap";

	public GameObject ReadyObject;

	public Dropdown EnvironmentDropdown;

	public InputField EnvironmentInput;

	public Dropdown DataSourceDropdown;

	public InputField DataSourceInput;

	public InputField LaunchInput;

	public Button LaunchButton;

	private List<string> _environments;

	private string _currentEnvironment;

	private List<string> _dataSources;

	private string _currentDataSource;

	private bool _suppress;

	private void Start()
	{
		ReadyObject.SetActive(value: true);
		LaunchInput.text = (string.IsNullOrWhiteSpace(LaunchScene) ? "Bootstrap" : LaunchScene);
		LaunchButton.onClick.AddListener(OnLaunchButton);
		_environments = GetEnvironments();
		_environments.Add("<custom>");
		EnvironmentDropdown.options = _environments.ConvertAll((string e) => new Dropdown.OptionData(e));
		_currentEnvironment = MDNPlayerPrefs.PreviousFDServer;
		int num = _environments.IndexOf(_currentEnvironment);
		if (num < 0)
		{
			_currentEnvironment = _environments[0];
			num = 0;
		}
		EnvironmentDropdown.value = num;
		EnvironmentInput.text = _currentEnvironment;
		EnvironmentDropdown.onValueChanged.AddListener(OnEnvironmentDropdown);
		EnvironmentInput.onValueChanged.AddListener(OnEnvironmentInput);
		_dataSources = GetDataSources();
		_dataSources.Add("<custom>");
		DataSourceDropdown.options = _dataSources.ConvertAll((string e) => new Dropdown.OptionData(e));
		_currentDataSource = DataSourceUtilities.GetCurrentDataSource();
		int num2 = _dataSources.IndexOf(_currentDataSource);
		if (num2 < 0)
		{
			_currentDataSource = _dataSources[0];
		}
		DataSourceDropdown.value = num2;
		DataSourceInput.text = _currentDataSource;
		DataSourceDropdown.onValueChanged.AddListener(OnDataSourceDropdown);
		DataSourceInput.onValueChanged.AddListener(OnDataSourceInput);
	}

	private void OnLaunchButton()
	{
		MDNPlayerPrefs.PreviousFDServer = _currentEnvironment;
		PlayerPrefs.SetString("EditorDataSource", _currentDataSource);
		Scenes.LoadScene(LaunchInput.text);
	}

	private List<string> GetEnvironments()
	{
		Task<ServicesConfiguration> task = new DefaultConfigurationLoader().LoadServicesConfiguration();
		task.Wait();
		List<string> list = task.Result?.ActiveEnvironmentNames?.ToList() ?? new List<string>(1);
		if (list.Count == 0)
		{
			Debug.LogWarning("No environments defined; defaulting to 'QADev'.");
			list.Add("QADev");
		}
		return list;
	}

	private void OnEnvironmentDropdown(int value)
	{
		if (!_suppress)
		{
			_currentEnvironment = _environments[value];
			_suppress = true;
			EnvironmentInput.text = _currentEnvironment;
			_suppress = false;
		}
	}

	private void OnEnvironmentInput(string value)
	{
		if (!_suppress)
		{
			_currentEnvironment = value;
			int num = _environments.IndexOf(_currentEnvironment);
			if (num < 0)
			{
				num = _environments.Count - 1;
			}
			_suppress = true;
			EnvironmentDropdown.value = num;
			_suppress = false;
		}
	}

	private List<string> GetDataSources()
	{
		List<string> list = new List<string>();
		DirectoryInfo[] directories = new DirectoryInfo("BuildDataSources").GetDirectories();
		foreach (DirectoryInfo directoryInfo in directories)
		{
			if (directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Length != 0)
			{
				list.Add(directoryInfo.Name);
			}
		}
		return list;
	}

	private void OnDataSourceDropdown(int value)
	{
		if (!_suppress)
		{
			_currentDataSource = _dataSources[value];
			_suppress = true;
			DataSourceInput.text = _currentDataSource;
			_suppress = false;
		}
	}

	private void OnDataSourceInput(string value)
	{
		if (!_suppress)
		{
			_currentDataSource = value;
			int num = _dataSources.IndexOf(_currentDataSource);
			if (num < 0)
			{
				num = _dataSources.Count - 1;
			}
			_suppress = true;
			DataSourceDropdown.value = num;
			_suppress = false;
		}
	}
}
