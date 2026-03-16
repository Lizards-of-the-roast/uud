using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Core.Meta.Utilities;
using Core.Code.Input;
using Core.Code.Promises;
using Core.Meta.Utilities;
using MTGA.KeyboardManager;
using Microsoft.Win32;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using WAS;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Loc;
using cTMP;

namespace Wotc.Mtga.Login;

public class BirthLanguagePanel : Panel
{
	[SerializeField]
	private cTMP_Dropdown _month_dropDown;

	[SerializeField]
	private cTMP_Dropdown _day_dropDown;

	[SerializeField]
	private cTMP_Dropdown _year_dropDown;

	[SerializeField]
	private cTMP_Dropdown _country_dropDown;

	[SerializeField]
	private TMP_Dropdown _language_dropDown;

	[SerializeField]
	private cTMP_Dropdown _experience_dropDown;

	[SerializeField]
	private TextMeshProUGUI _feedbackText;

	[SerializeField]
	private GameObject _supportLink;

	[SerializeField]
	private bool _isAccountUpdate;

	private readonly Dictionary<string, int> _languageToIndex = new Dictionary<string, int>();

	private readonly Dictionary<int, string> _indexToLanguage = new Dictionary<int, string>();

	private Color _unselectedColor;

	private readonly List<string> _days = new List<string>();

	private int _firstYear;

	private readonly StringBuilder _sb = new StringBuilder();

	private (cTMP_Dropdown Dropdown, string AnalyticsStep, bool localizeText)[] _allDropdowns;

	private (cTMP_Dropdown Dropdown, string AnalyticsStep, bool localizeText)[] AllDropdowns => _allDropdowns ?? (_allDropdowns = GenerateAllDropdowns());

	private static List<string> SortedCountryLocKeys => CountryLocToCodes.Countries.Keys.OrderBy((string key) => Languages.ActiveLocProvider.GetLocalizedText(key), StringComparer.CurrentCultureIgnoreCase).ToList();

	private static List<string> Months => new List<string>
	{
		"MainNav/Login/January", "MainNav/Login/February", "MainNav/Login/March", "MainNav/Login/April", "MainNav/Login/May", "MainNav/Login/June", "MainNav/Login/July", "MainNav/Login/August", "MainNav/Login/September", "MainNav/Login/October",
		"MainNav/Login/November", "MainNav/Login/December"
	};

	private static List<string> ExperienceLevels => new List<string> { "MainNav/Login/Experience_Novice", "MainNav/Login/Experience_Intermediate", "MainNav/Login/Experience_Advanced" };

	private (cTMP_Dropdown Dropdown, string AnalyticsStep, bool localizeText)[] GenerateAllDropdowns()
	{
		List<(cTMP_Dropdown, string, bool)> list = new List<(cTMP_Dropdown, string, bool)>();
		if ((bool)_month_dropDown)
		{
			list.Add((_month_dropDown, "blc_month_chosen", true));
		}
		if ((bool)_day_dropDown)
		{
			list.Add((_day_dropDown, "blc_day_chosen", false));
		}
		if ((bool)_year_dropDown)
		{
			list.Add((_year_dropDown, "blc_year_chosen", false));
		}
		if ((bool)_country_dropDown)
		{
			list.Add((_country_dropDown, "blc_country_chosen", true));
		}
		if ((bool)_experience_dropDown)
		{
			list.Add((_experience_dropDown, "blc_experience_chosen", true));
		}
		return list.ToArray();
	}

	public override void Initialize(LoginScene loginScene, IActionSystem actions, KeyboardManager keyboardManager, IBILogger biLogger)
	{
		_unselectedColor = _month_dropDown.captionText.color;
		for (int i = 1; i <= 31; i++)
		{
			_sb.Length = 0;
			_sb.Append(i);
			_days.Add(_sb.ToString());
		}
		_firstYear = DateTime.Now.Year;
		List<string> list = new List<string>();
		for (int num = _firstYear; num >= DateTime.Now.Year - 125; num--)
		{
			_sb.Length = 0;
			_sb.Append(num);
			list.Add(_sb.ToString());
		}
		_year_dropDown.AddOptions(list);
		_month_dropDown.AddOptions(Months);
		_country_dropDown.AddOptions(SortedCountryLocKeys);
		if ((bool)_experience_dropDown)
		{
			_experience_dropDown.AddOptions(ExperienceLevels);
		}
		_language_dropDown.AddOptions(SetUpLanguages());
		(cTMP_Dropdown, string, bool)[] allDropdowns = AllDropdowns;
		for (int j = 0; j < allDropdowns.Length; j++)
		{
			var (cTMP_Dropdown, analyticsStep, localizeText) = allDropdowns[j];
			cTMP_Dropdown.Init(actions);
			cTMP_Dropdown.onValueChanged.AddListener(_dropdownChanged(cTMP_Dropdown, analyticsStep));
			cTMP_Dropdown.LocalizeText = localizeText;
		}
		_supportLink.SetActive(value: false);
		base.Initialize(loginScene, actions, keyboardManager, biLogger);
	}

	public override void Show()
	{
		LoginFlowAnalytics.SendEvent_Registration("blc_begin", _isAccountUpdate);
		if ((bool)_experience_dropDown)
		{
			_experience_dropDown.gameObject.SetActive(value: true);
		}
		_feedbackText.gameObject.SetActive(value: false);
		_supportLink.gameObject.SetActive(value: false);
		(cTMP_Dropdown, string, bool)[] allDropdowns = AllDropdowns;
		for (int i = 0; i < allDropdowns.Length; i++)
		{
			allDropdowns[i].Item1.captionText.color = _unselectedColor;
		}
		_month_dropDown.value = -1;
		_month_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Birth_Month");
		_day_dropDown.ClearOptions();
		_day_dropDown.AddOptions(_days);
		_day_dropDown.value = -1;
		_day_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Day");
		_year_dropDown.value = -1;
		_year_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Year");
		_country_dropDown.value = -1;
		_country_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Country");
		if ((bool)_experience_dropDown)
		{
			_experience_dropDown.value = -1;
			_experience_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Experience_Question");
		}
		_languageToIndex.TryGetValue(GetInitialLanguage(), out var value);
		_language_dropDown.value = value;
		Languages.LanguageChangedSignal.Listeners += RefreshText;
		base.Show();
		EnableButton(enabled: true);
	}

	public override void Hide()
	{
		Languages.LanguageChangedSignal.Listeners -= RefreshText;
		base.Hide();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Languages.LanguageChangedSignal.Listeners -= RefreshText;
	}

	private void RefreshText()
	{
		_supportLink.SetActive(value: false);
		if (_month_dropDown.value == -1)
		{
			_month_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Birth_Month");
		}
		else
		{
			_month_dropDown.RefreshShownValue();
		}
		if (_day_dropDown.value == -1)
		{
			_day_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Day");
		}
		if (_year_dropDown.value == -1)
		{
			_year_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Year");
		}
		bool num = _country_dropDown.value == -1;
		string currentCountryLocKey = _country_dropDown.UnlocalizedValue();
		_country_dropDown.ClearOptions();
		_country_dropDown.AddOptions(SortedCountryLocKeys);
		if (!num && !string.IsNullOrEmpty(currentCountryLocKey))
		{
			int num2 = _country_dropDown.options.FindIndex((cTMP_Dropdown.OptionData od) => od.text == currentCountryLocKey);
			if (num2 != -1)
			{
				if (_country_dropDown.value == num2)
				{
					_country_dropDown.RefreshShownValue();
				}
				else
				{
					_country_dropDown.value = num2;
				}
				goto IL_016c;
			}
		}
		_country_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Country");
		goto IL_016c;
		IL_016c:
		if ((bool)_experience_dropDown)
		{
			if (_experience_dropDown.value == -1)
			{
				_experience_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Experience_Question");
			}
			else
			{
				_experience_dropDown.RefreshShownValue();
			}
		}
		_languageToIndex.TryGetValue(Languages.CurrentLanguage, out var value);
		_language_dropDown.value = value;
		_language_dropDown.ClearOptions();
		_language_dropDown.AddOptions(SetUpLanguages());
		_language_dropDown.SetValueWithoutNotify(value);
	}

	public void UpdateLanguage()
	{
		if (_indexToLanguage.TryGetValue(_language_dropDown.value, out var value))
		{
			MDNPlayerPrefs.PLAYERPREFS_ClientLanguage = value;
			Languages.CurrentLanguage = value;
		}
	}

	public void GenericHoverAudio()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public void PlayToggleAudio()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_toggle, base.gameObject);
	}

	public void UpdateNumberOfDays()
	{
		int count = 31;
		if (_month_dropDown.value >= 0)
		{
			count = DateTime.DaysInMonth((_year_dropDown.value >= 0) ? (_firstYear - _year_dropDown.value) : 2016, _month_dropDown.value + 1);
		}
		bool num = _day_dropDown.value >= 0;
		_day_dropDown.ClearOptions();
		_day_dropDown.AddOptions(_days.GetRange(0, count));
		if (!num)
		{
			_day_dropDown.value = -1;
			_day_dropDown.captionText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Day");
		}
		else if (_day_dropDown.value >= _day_dropDown.options.Count)
		{
			_day_dropDown.value = _day_dropDown.options.Count - 1;
		}
	}

	public void OnDropdownDown_Experience()
	{
		LoginFlowAnalytics.SendEvent_Registration("blc_experience_pressed", _isAccountUpdate);
	}

	private UnityAction<int> _dropdownChanged(cTMP_Dropdown dropdown, string analyticsStep)
	{
		return Closure;
		void Closure(int arg0)
		{
			LoginFlowAnalytics.SendEvent_Registration(analyticsStep, _isAccountUpdate);
			if (dropdown.value != -1 && dropdown.captionText.color != Color.white)
			{
				dropdown.captionText.color = Color.white;
			}
		}
	}

	private void RequiresAgeGate()
	{
		AgeGateUtils.GateUserFromLoginDueToAge();
		EnableButton(enabled: false);
		_loginScene.LoadPanel(PanelType.WelcomeGate);
		LoginUtils.ShowAgeGateRegistrationFailurePopup();
	}

	private IEnumerator AgeGateCheck_Coroutine(string playerCountry, DateTime birthday)
	{
		yield return _loginScene._accountClient.GetAgeGate(playerCountry, birthday.ToString("yyyy-MM-dd")).ThenOnMainThreadIfSuccess(delegate(AgeCheckForAgeGatingResponse result)
		{
			if (result.requiresAgeGate)
			{
				RequiresAgeGate();
			}
			else
			{
				_loginScene.Birthday = $"{birthday.Year}-{birthday.Month.ToString().PadLeft(2, '0')}-{birthday.Day.ToString().PadLeft(2, '0')}";
				_loginScene.SelectedCountry = playerCountry;
				if ((bool)_experience_dropDown)
				{
					MDNPlayerPrefs.PLAYERPREFS_Experience = ((PlayerExperience)_experience_dropDown.value/*cast due to .constrained prefix*/).ToString();
				}
				_loginScene.LoadPanel(PanelType.Register);
			}
		}).ThenOnMainThreadIfError(delegate(Error e)
		{
			AccountError error = WASUtils.ToAccountError(e);
			_loginScene.HandleAccountError(error, null, selectInputField: true);
			RequiresAgeGate();
		})
			.AsCoroutine();
	}

	private IEnumerator UpdateParentalConsent_Coroutine(string playerCountry, DateTime birthday)
	{
		yield return _loginScene._accountClient.UpdateParentalConsent(playerCountry, birthday.ToString("yyyy-MM-dd")).ThenOnMainThreadIfSuccess((Action<string>)delegate
		{
			_loginScene._accountClient.LogIn_Fast().ThenOnMainThreadIfError((Action<Error>)delegate
			{
				_loginScene.LoadNextPanelBasedOnLoginState();
			});
		}).ThenOnMainThreadIfError(delegate(Error e)
		{
			AccountError error = WASUtils.ToAccountError(e);
			_loginScene.HandleAccountError(error, null, selectInputField: true);
			_loginScene.LoadPanel(PanelType.WelcomeGate);
			LoginUtils.ShowUpdateParentalConsentFailurePopup();
		})
			.AsCoroutine();
	}

	public void OnButton_ContinueLanguage()
	{
		_supportLink.SetActive(value: false);
		if (_month_dropDown.value == -1 || _day_dropDown.value == -1 || _year_dropDown.value == -1)
		{
			_feedbackText.gameObject.SetActive(value: true);
			_feedbackText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Enter_Valid_Birthdate_Feedback");
			_supportLink.SetActive(value: true);
			return;
		}
		if (_country_dropDown.value == -1)
		{
			_feedbackText.gameObject.SetActive(value: true);
			_feedbackText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Select_Country_Feedback");
			_supportLink.SetActive(value: true);
			return;
		}
		MDNPlayerPrefs.PLAYERPREFS_HasSelectedInitialLanguage = true;
		LoginFlowAnalytics.SendEvent_Registration("blc_success", _isAccountUpdate);
		string playerCountry = CountryLocToCodes.Countries[_country_dropDown.UnlocalizedValue()];
		int year = _firstYear - _year_dropDown.value;
		int month = _month_dropDown.value + 1;
		int day = _day_dropDown.value + 1;
		DateTime birthday = new DateTime(year, month, day);
		IEnumerator routine = (_isAccountUpdate ? UpdateParentalConsent_Coroutine(playerCountry, birthday) : AgeGateCheck_Coroutine(playerCountry, birthday));
		StartCoroutine(routine);
	}

	private List<string> SetUpLanguages()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>(Languages.ExternalLanguages);
		for (int i = 0; i < list2.Count; i++)
		{
			string text = Languages.MTGAtoI2LangCode[list2[i]];
			string key = "MainNav/Settings/LanguageNative_" + text;
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText(key);
			_indexToLanguage[i] = list2[i];
			_languageToIndex[list2[i]] = i;
			list.Add(localizedText);
		}
		return list;
	}

	private string GetInitialLanguage()
	{
		string result = "en-US";
		if (PlayerPrefsExt.HasKey("ClientLanguage"))
		{
			result = MDNPlayerPrefs.PLAYERPREFS_ClientLanguage;
		}
		else
		{
			ReadInitialLanguageFromIniFile(ref result);
		}
		return result;
	}

	private static void ReadInitialLanguageFromIniFile(ref string result)
	{
		string text = string.Empty;
		string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..\\", "MTGAUpdater.ini"));
		if (new FileInfo(fullPath).Exists)
		{
			string[] array = File.ReadAllLines(fullPath);
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i].ToLower();
				if (text2.StartsWith("applicationname="))
				{
					text = text2.Replace("applicationname=", "");
					break;
				}
			}
		}
		string text3 = string.Empty;
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\\\WOW6432Node\\\\Wizards of the Coast\\\\" + text + "\\\\", writable: false);
				if (registryKey != null)
				{
					object value = registryKey.GetValue("ProductLanguage");
					if (value != null)
					{
						text3 = value as string;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("SharedContextView.GetInstallerLanguage -- unable to get registry key! {0}", ex.ToString());
			}
		}
		if (!string.IsNullOrEmpty(text3))
		{
			switch (text3)
			{
			case "1031":
				result = "German";
				break;
			case "1036":
				result = "French";
				break;
			case "1040":
				result = "Italian";
				break;
			case "1041":
				result = "Japanese";
				break;
			case "1042":
				result = "Korean";
				break;
			case "1046":
				result = "Portugese (Brazil)";
				break;
			case "3082":
				result = "Spanish";
				break;
			default:
				result = "English";
				break;
			}
		}
		else
		{
			result = "English";
		}
		result = Languages.Converter[result];
	}

	public void OnButton_Support()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/ReportABug/Support_URL"));
	}

	public void OnButton_AlreadyHaveAccount()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		_loginScene.LoadPanel(PanelType.LogIn);
	}

	public override void OnAccept()
	{
		OnButton_ContinueLanguage();
	}

	public override void OnNext()
	{
		EventSystem.current.SetSelectedGameObject(_month_dropDown.gameObject);
	}
}
