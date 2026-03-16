using TMPro;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class View_UserPrompt : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _promptLabel;

	[SerializeField]
	private CanvasGroup _promptPanel;

	[Header("Approximately the time it will take to reach the target. A smaller value will reach the target faster")]
	[SerializeField]
	private float _fadeInDuration = 0.3f;

	[SerializeField]
	private float _fadeOutDuration = 0.1f;

	private bool _textSet;

	private bool _isVisible = true;

	private float _fadeVelocity;

	private void Awake()
	{
		_promptPanel.alpha = 0f;
	}

	private void Update()
	{
		float num = ((_textSet && _isVisible) ? 1 : 0);
		if (_promptPanel.alpha != num)
		{
			float smoothTime = (_isVisible ? _fadeInDuration : _fadeOutDuration);
			_promptPanel.alpha = Mathf.SmoothDamp(_promptPanel.alpha, num, ref _fadeVelocity, smoothTime);
		}
	}

	public void SetPromptText(string text)
	{
		_fadeVelocity = 0f;
		_promptLabel.SetText(text);
		_textSet = !string.IsNullOrEmpty(text);
	}

	public void SetVisibility(bool visible)
	{
		_isVisible = visible;
	}
}
