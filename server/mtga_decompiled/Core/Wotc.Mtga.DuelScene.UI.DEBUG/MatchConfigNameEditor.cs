using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class MatchConfigNameEditor : MonoBehaviour
{
	private static char[] _invalidFileNameCharacters = new char[10] { '.', '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

	[SerializeField]
	private TMP_InputField _nameInputField;

	[SerializeField]
	private Button _updateNameButton;

	private string _prvInput;

	private string _viewModel;

	public event Action<string, string> ViewModelUpdated;

	private void Awake()
	{
		_nameInputField.onValueChanged.AddListener(OnNameModified);
		_updateNameButton.onClick.AddListener(UpdateName);
	}

	private void OnDestroy()
	{
		_nameInputField.onValueChanged.RemoveListener(OnNameModified);
		_updateNameButton.onClick.RemoveListener(UpdateName);
	}

	public void SetViewModel(string viewModel)
	{
		_prvInput = viewModel;
		_viewModel = viewModel;
		_nameInputField.SetTextWithoutNotify(_viewModel);
	}

	private void OnNameModified(string value)
	{
		char[] invalidFileNameCharacters = _invalidFileNameCharacters;
		foreach (char value2 in invalidFileNameCharacters)
		{
			if (value.Contains(value2))
			{
				_nameInputField.SetTextWithoutNotify(_prvInput);
				return;
			}
		}
		_prvInput = value;
	}

	private void UpdateName()
	{
		string text = _nameInputField.text;
		if (!(text == _viewModel))
		{
			string viewModel = _viewModel;
			_viewModel = text;
			this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
		}
	}
}
