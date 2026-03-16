using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;

public class View_ChooseXInterface : MonoBehaviour, IChooseXInterface
{
	private const string ANIMATOR_BOOL_DISABLED = "Disabled";

	[SerializeField]
	private GameObject _root;

	[SerializeField]
	private Button _upArrowButton;

	[SerializeField]
	private Button _upFiveArrowButton;

	[SerializeField]
	private Button _downArrowButton;

	[SerializeField]
	private Button _downFiveArrowButton;

	[SerializeField]
	private TextMeshProUGUI _buttonLabel;

	[SerializeField]
	private Button _confirmationButton;

	[SerializeField]
	private Animator _confirmationAnimator;

	[SerializeField]
	private GameObject _mainButton;

	[SerializeField]
	private GameObject _secondaryButton;

	public event Action Submit;

	public event Action<int> ValueModified;

	private void Awake()
	{
		_upArrowButton.onClick.AddListener(delegate
		{
			this.ValueModified?.Invoke(1);
		});
		_upFiveArrowButton.onClick.AddListener(delegate
		{
			this.ValueModified?.Invoke(5);
		});
		_downArrowButton.onClick.AddListener(delegate
		{
			this.ValueModified?.Invoke(-1);
		});
		_downFiveArrowButton.onClick.AddListener(delegate
		{
			this.ValueModified?.Invoke(-5);
		});
		_confirmationButton.onClick.AddListener(delegate
		{
			this.Submit?.Invoke();
		});
	}

	public void Open()
	{
		_root.UpdateActive(active: true);
	}

	public void Close()
	{
		_root.UpdateActive(active: false);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, AudioManager.Default);
	}

	public void SetButtonText(string buttonText)
	{
		_buttonLabel.SetText(buttonText);
	}

	public void SetButtonStyle(ButtonStyle.StyleType styleType)
	{
		_mainButton.UpdateActive(styleType == ButtonStyle.StyleType.Main);
		_secondaryButton.UpdateActive(styleType == ButtonStyle.StyleType.Secondary);
	}

	public void SetVisualState(NumericInputVisualState state)
	{
		_upFiveArrowButton.interactable = state.HasFlag(NumericInputVisualState.IncrementManyEnabled);
		_upFiveArrowButton.gameObject.UpdateActive(state.HasFlag(NumericInputVisualState.IncrementManyEnabled));
		_downFiveArrowButton.interactable = state.HasFlag(NumericInputVisualState.DecrementManyEnabled);
		_downFiveArrowButton.gameObject.UpdateActive(state.HasFlag(NumericInputVisualState.DecrementManyEnabled));
		_upArrowButton.interactable = state.HasFlag(NumericInputVisualState.IncrementEnabled);
		_downArrowButton.interactable = state.HasFlag(NumericInputVisualState.DecrementEnabled);
		if (_confirmationAnimator != null)
		{
			_confirmationAnimator.SetBool("Disabled", !state.HasFlag(NumericInputVisualState.CanSubmit));
		}
	}

	private void OnDestroy()
	{
		this.Submit = null;
		this.ValueModified = null;
		_upArrowButton.onClick.RemoveAllListeners();
		_upFiveArrowButton.onClick.RemoveAllListeners();
		_downArrowButton.onClick.RemoveAllListeners();
		_downFiveArrowButton.onClick.RemoveAllListeners();
		_confirmationButton.onClick.RemoveAllListeners();
	}
}
