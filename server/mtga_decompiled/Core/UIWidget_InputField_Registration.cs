using Core.Code.Input;
using TMPro;
using UnityEngine;

public class UIWidget_InputField_Registration : MonoBehaviour, INextActionHandler, IPreviousActionHandler
{
	[SerializeField]
	private TextMeshProUGUI _feedbackText;

	[SerializeField]
	private bool _resetInputFieldOnFeedbackTextChange = true;

	private IActionSystem _actionSystem;

	public TMP_InputField InputField => GetComponentInChildren<TMP_InputField>();

	public TextMeshProUGUI FeedbackText => _feedbackText;

	private void Awake()
	{
		_feedbackText.gameObject.SetActive(value: false);
	}

	public void Initialize(IActionSystem actionSystem)
	{
		_actionSystem = actionSystem;
		RemoveInputListeners();
		InputField.onSelect.AddListener(OnInputFieldSelected);
	}

	private void RemoveInputListeners()
	{
		InputField.onSelect.RemoveListener(OnInputFieldSelected);
		InputField.onDeselect.RemoveListener(OnInputFieldDeselected);
	}

	private void OnDestroy()
	{
		RemoveInputListeners();
	}

	public void SetFeedbackText(string feedbackText)
	{
		if (feedbackText != null)
		{
			_feedbackText.text = feedbackText.Trim();
		}
		_feedbackText.gameObject.SetActive(value: true);
		if (_resetInputFieldOnFeedbackTextChange)
		{
			InputField.text = string.Empty;
		}
	}

	public void SetFeedbackText(string feedbackText, bool resetInput)
	{
		if (feedbackText != null)
		{
			_feedbackText.text = feedbackText.Trim();
		}
		_feedbackText.gameObject.SetActive(value: true);
		if (resetInput)
		{
			InputField.text = string.Empty;
		}
	}

	public void ClearFeedbackText()
	{
		_feedbackText.text = string.Empty;
		_feedbackText.gameObject.SetActive(value: false);
	}

	private void OnInputFieldSelected(string text)
	{
		_actionSystem.PushFocus(this);
		InputField.onSelect.RemoveListener(OnInputFieldSelected);
		InputField.onDeselect.AddListener(OnInputFieldDeselected);
	}

	private void OnInputFieldDeselected(string text)
	{
		_actionSystem.PopFocus(this);
		InputField.onSelect.AddListener(OnInputFieldSelected);
		InputField.onDeselect.RemoveListener(OnInputFieldDeselected);
	}

	public void OnNext()
	{
		if (InputField.navigation.selectOnDown != null)
		{
			InputField.navigation.selectOnDown.Select();
		}
	}

	public void OnPrevious()
	{
		if (InputField.navigation.selectOnUp != null)
		{
			InputField.navigation.selectOnUp.Select();
		}
	}
}
