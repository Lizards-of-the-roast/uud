using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wizards.Mtga.Credits;

public class CreditsDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private Transform _textScrollView;

	[SerializeField]
	private CustomButton _backButton;

	[SerializeField]
	private CustomButton _universesBeyondButton;

	private string _ubSearchText;

	public event Action BackButtonClicked;

	private void Awake()
	{
		_backButton.OnClick.AddListener(OnBackClicked);
		_universesBeyondButton.OnClick.AddListener(ScrollToUbSection);
	}

	private void OnBackClicked()
	{
		this.BackButtonClicked?.Invoke();
	}

	public void SetCreditsText(string creditsText, string ubSearchText)
	{
		_text.text = creditsText;
		_ubSearchText = ubSearchText;
	}

	private void OnDestroy()
	{
		_backButton.OnClick.RemoveListener(OnBackClicked);
		_universesBeyondButton.OnClick.RemoveListener(ScrollToUbSection);
	}

	public void ScrollToUbSection()
	{
		List<string> list = new List<string>(_text.text.Split('\n'));
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, _universesBeyondButton.gameObject);
		int num = (from x in list.Select((string text, int index) => (text: text, index: index))
			where x.text.Contains(_ubSearchText)
			select x.index).FirstOrDefault();
		float num2 = (float)(_text.textInfo.lineCount - list.Count + num) / (float)_text.textInfo.lineCount;
		float verticalNormalizedPosition = 1f - num2;
		_textScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = verticalNormalizedPosition;
	}
}
