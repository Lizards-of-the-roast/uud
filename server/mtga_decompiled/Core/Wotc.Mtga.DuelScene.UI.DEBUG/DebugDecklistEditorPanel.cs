using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using GreClient.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class DebugDecklistEditorPanel : MonoBehaviour
{
	public struct ContextProviders
	{
		public readonly ICardDatabaseAdapter CardDB;

		public readonly ICardDataProvider CardData;

		public readonly ICardTitleProvider CardTitle;

		public DeckConfigProvider DeckConfig { get; internal set; }

		public ContextProviders(DeckConfigProvider deckConfig, ICardDatabaseAdapter cardDB, ICardDataProvider cardData, ICardTitleProvider cardTitle)
		{
			DeckConfig = deckConfig;
			CardDB = cardDB;
			CardData = cardData;
			CardTitle = cardTitle;
		}
	}

	private const string RichTextRegexPattern = "<.*?>";

	private const string ReturnCharacterRegexPattern = "\r\n|\r|\n";

	private GameObject _parent;

	private ContextProviders _providers;

	private StringBuilder _sb;

	private bool _isInitialized;

	private bool _listHasHighlightedEntries;

	private ColorBlock _saveButtonColors;

	private float _timeDecklistLastDirtied;

	private float _timeDecklistValidationLastAttempted;

	private float _timeDecklistLastValidated;

	private DeckCollectionDeck _lastValidatedDeck;

	[SerializeField]
	[Tooltip("How long to wait after last text input before validating deck (to avoid doing lookups while user is actively typing in cards)")]
	[Range(0f, 5f)]
	private float _validationCooldown;

	[SerializeField]
	internal Button _closeButton;

	[SerializeField]
	internal Button _infoButton;

	[SerializeField]
	internal Button _saveButton;

	[SerializeField]
	private Color _dirtiedSaveButtonColor = Color.yellow;

	[SerializeField]
	[Range(0f, 1f)]
	private float _saveButtonOriginalColorWeight = 0.75f;

	[SerializeField]
	private Color _invalidSaveButtonColor = Color.red;

	[SerializeField]
	private Color _invalidCardNameColor = Color.red;

	[SerializeField]
	internal Button _revertButton;

	[SerializeField]
	internal Button _saveAsButton;

	[SerializeField]
	internal DeckConfigEditor _decklistSelector;

	[SerializeField]
	internal TMP_InputField _cardListInput;

	[SerializeField]
	internal TMP_InputField _styleCodeInput;

	[SerializeField]
	internal string _helpPageUrl = "https://wizardsofthecoast.atlassian.net/wiki/spaces/MDN/pages/643927636/Debug+Quick+Play";

	public ObservableValue<bool> HasUnsavedChanges = new ObservableValue<bool>();

	private DeckConfig SelectedDeck => _decklistSelector.CurrentView.SelectedDeck;

	public DeckConfigEditor ConfigEditor => _decklistSelector;

	private DeckConfigEditor.ViewModel DeckEditorConfigView => _decklistSelector.CurrentView;

	private bool DecklistIsDirty => _timeDecklistLastDirtied >= _timeDecklistValidationLastAttempted;

	private bool DecklistIsValid => _timeDecklistLastDirtied <= _timeDecklistLastValidated;

	private bool ShouldValidateDecklist
	{
		get
		{
			if (DecklistIsDirty)
			{
				return !InDecklistCooldown;
			}
			return false;
		}
	}

	private bool InDecklistCooldown => Time.realtimeSinceStartup - _timeDecklistLastDirtied < _validationCooldown;

	public bool IsOpen => base.gameObject.activeInHierarchy;

	public bool CanEditDecklist => !IsFallbackDeckName(SelectedDeck.Name);

	private string SanitizedDecklist => StripRichText(ReplaceBreakWithReturn(_cardListInput.text));

	public event Action Closed;

	public void Initialize(ContextProviders providers)
	{
		_providers = providers;
		_saveButtonColors = _saveButton.colors;
		_isInitialized = true;
		RefreshButtonInteractability();
	}

	private void Start()
	{
		_sb = new StringBuilder();
		_infoButton.onClick.AddListener(delegate
		{
			Application.OpenURL(_helpPageUrl);
		});
	}

	private bool HasRichText(string text)
	{
		return Regex.IsMatch(text, "<.*?>");
	}

	private string StripRichText(string text)
	{
		return Regex.Replace(text, "<.*?>", string.Empty);
	}

	private string ReplaceReturnWithBreak(string text)
	{
		return Regex.Replace(text, "\r\n|\r|\n", "<br>");
	}

	private string ReplaceBreakWithReturn(string text)
	{
		return text.Replace("<br>", Environment.NewLine);
	}

	private void Update()
	{
		if (ShouldValidateDecklist)
		{
			TryValidateDecklist();
			RefreshButtonInteractability();
		}
	}

	private void SetTextInput(string newText)
	{
		int caretPosition = _cardListInput.caretPosition;
		_cardListInput.SetTextWithoutNotify(ReplaceReturnWithBreak(newText));
		_cardListInput.Rebuild(CanvasUpdate.LatePreRender);
		_cardListInput.caretPosition = caretPosition;
		_cardListInput.Rebuild(CanvasUpdate.LatePreRender);
	}

	private List<string> HighlightFailures()
	{
		_sb.Clear();
		string[] array = Regex.Split(SanitizedDecklist, "\r\n|\r|\n");
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				_sb.AppendLine();
				continue;
			}
			bool flag = DeckCollection.IsValidLegacyDecklistLine(_providers.CardDB, text);
			string value = (flag ? (text ?? "") : ("<color=#" + ColorUtility.ToHtmlStringRGB(_invalidCardNameColor) + ">" + text + "</color>"));
			_sb.AppendLine(value);
			if (!flag)
			{
				list.Add(text);
			}
		}
		SetTextInput(_sb.ToString());
		_listHasHighlightedEntries = list.Count != 0;
		return list;
	}

	private void RemoveHighlightingFromInputPanel()
	{
		string textInput = StripRichText(ReplaceBreakWithReturn(_cardListInput.text));
		SetTextInput(textInput);
		_listHasHighlightedEntries = false;
	}

	private void OnEnable()
	{
		_saveButton.onClick.AddListener(OnSaveRequested);
		_revertButton.onClick.AddListener(OnRevertRequested);
		_closeButton.onClick.AddListener(Close);
		_cardListInput.onValueChanged.AddListener(OnDecklistModified);
	}

	private void OnDisable()
	{
		_saveButton.onClick.RemoveListener(OnSaveRequested);
		_revertButton.onClick.RemoveListener(OnRevertRequested);
		_saveAsButton.onClick.RemoveListener(OnSaveAsRequested);
		_closeButton.onClick.RemoveListener(Close);
		_cardListInput.onValueChanged.RemoveListener(OnDecklistModified);
	}

	private void OnDestroy()
	{
		_infoButton.onClick.RemoveAllListeners();
		_closeButton.onClick.RemoveAllListeners();
		_saveButton.onClick.RemoveAllListeners();
		_saveAsButton.onClick.RemoveAllListeners();
		_cardListInput.onValueChanged.RemoveAllListeners();
	}

	public void Open(DeckConfigEditor.ViewModel deck)
	{
		_decklistSelector.SetModel(deck);
		RefreshDecklist(_decklistSelector.CurrentView);
		_decklistSelector.ViewModelChanged += OnDecklistSelectorChange;
		base.gameObject.SetActive(value: true);
	}

	public void UpdateDeckConfigProvider(DeckConfigProvider deckConfigProvider)
	{
		_providers.DeckConfig = deckConfigProvider;
	}

	public bool TryValidateDecklist()
	{
		_timeDecklistValidationLastAttempted = Time.realtimeSinceStartup;
		if (!DeckCollection.CreateDeckFromText(_providers.CardDB, SanitizedDecklist, out var deck))
		{
			List<string> list = HighlightFailures();
			_sb.Clear();
			_sb.AppendLine("Unable to validate current decklist. These entries were invalid:");
			foreach (string item in list)
			{
				_sb.AppendLine(item);
			}
			return false;
		}
		_timeDecklistLastValidated = Time.realtimeSinceStartup;
		_lastValidatedDeck = deck;
		return true;
	}

	private void RefreshButtonInteractability()
	{
		if (_isInitialized)
		{
			bool flag = (bool)HasUnsavedChanges && (DecklistIsDirty || DecklistIsValid);
			ColorBlock colors = _saveButton.colors;
			if (flag && DecklistIsDirty)
			{
				colors.normalColor = Color.Lerp(_dirtiedSaveButtonColor, _saveButtonColors.normalColor, _saveButtonOriginalColorWeight);
				colors.selectedColor = Color.Lerp(_dirtiedSaveButtonColor, _saveButtonColors.selectedColor, _saveButtonOriginalColorWeight);
				colors.highlightedColor = Color.Lerp(_dirtiedSaveButtonColor, _saveButtonColors.highlightedColor, _saveButtonOriginalColorWeight);
				colors.pressedColor = Color.Lerp(_dirtiedSaveButtonColor, _saveButtonColors.highlightedColor, _saveButtonOriginalColorWeight);
			}
			else if (!flag && !DecklistIsValid)
			{
				colors.disabledColor = Color.Lerp(_invalidSaveButtonColor, _saveButtonColors.disabledColor, _saveButtonOriginalColorWeight);
			}
			else
			{
				colors = _saveButtonColors;
			}
			_saveButton.colors = colors;
			_saveButton.interactable = flag;
			_revertButton.interactable = HasUnsavedChanges;
			_closeButton.interactable = !HasUnsavedChanges;
		}
	}

	private void OnRevertRequested()
	{
		RefreshDecklist(_decklistSelector.CurrentView);
	}

	private void OnDecklistSelectorChange(DeckConfigEditor.ViewModel _, DeckConfigEditor.ViewModel currentView)
	{
		RefreshDecklist(currentView);
	}

	private void OnDecklistModified(string decklist)
	{
		_timeDecklistLastDirtied = Time.realtimeSinceStartup;
		if (!HasUnsavedChanges)
		{
			HasUnsavedChanges.Value = true;
		}
		if (_listHasHighlightedEntries)
		{
			RemoveHighlightingFromInputPanel();
		}
		RefreshButtonInteractability();
	}

	private void RefreshDecklist(DeckConfigEditor.ViewModel deckEditorView)
	{
		DeckConfig selectedDeck = deckEditorView.SelectedDeck;
		string text = (IsFallbackDeckName(selectedDeck.Name) ? selectedDeck.ConvertIdsToCardNames(_providers.CardData, _providers.CardTitle) : ReadDeckTextFile(deckEditorView.SelectedDirectory, selectedDeck.Name));
		_cardListInput.text = ReplaceReturnWithBreak(text);
		_cardListInput.interactable = CanEditDecklist;
		HasUnsavedChanges.Value = false;
		TryValidateDecklist();
		RefreshButtonInteractability();
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		this.Closed?.Invoke();
	}

	private string ReadDeckTextFile(string subDirectory, string deckName)
	{
		if (!_providers.DeckConfig.GetAllDecks().ContainsKey(subDirectory))
		{
			Debug.LogWarning("Attempting to read deck " + deckName + " from directory " + subDirectory + " and it is not in set of known deck configs. Something is wrong!");
		}
		FileInfo fileInfo = new FileInfo(Path.Combine(_providers.DeckConfig.RootDeckDirectory, subDirectory, deckName + ".txt"));
		if (!fileInfo.Exists)
		{
			SimpleLog.LogError("Can't load " + fileInfo.FullName + " because file doesn't exist.");
			return string.Empty;
		}
		return File.ReadAllText(fileInfo.FullName);
	}

	private static bool IsFallbackDeckName(string deckName)
	{
		if (!deckName.Equals("FallbackDeck"))
		{
			return deckName.Equals("FallbackDeck (w/ SB)");
		}
		return true;
	}

	private void OnSaveRequested()
	{
		DeckConfigEditor.ViewModel currentView = _decklistSelector.CurrentView;
		ref readonly string selectedDirectory = ref currentView.SelectedDirectory;
		DeckConfig selectedDeck = SelectedDeck;
		TrySaveDecklist(in selectedDirectory, in selectedDeck.Name, saveAs: false);
		RefreshButtonInteractability();
	}

	private void OnSaveAsRequested()
	{
		throw new NotImplementedException();
	}

	private bool TrySaveDecklist(in string subDirectory, in string deckName, bool saveAs)
	{
		if (DecklistIsDirty && !TryValidateDecklist())
		{
			return false;
		}
		FileInfo fileInfo = new FileInfo(Path.Join(_providers.DeckConfig.RootDeckDirectory, subDirectory, deckName + ".txt"));
		if (fileInfo.Exists == saveAs)
		{
			return false;
		}
		File.WriteAllText(fileInfo.FullName, SanitizedDecklist);
		if (saveAs)
		{
			throw new NotImplementedException();
		}
		HasUnsavedChanges.Value = false;
		DeckConfig modifiedDeck = DeckConfigProvider.ConstructDeckConfig(DeckEditorConfigView.SelectedDeck.Name, _lastValidatedDeck);
		Dictionary<string, IReadOnlyList<DeckConfig>> modifiedDeckOptions;
		bool result = DeckEditorConfigView.TryModifyDeckOptions(in modifiedDeck, out modifiedDeckOptions);
		_decklistSelector.SetModelAndNotify(DeckEditorConfigView.Modify(null, modifiedDeck, modifiedDeckOptions));
		return result;
	}
}
