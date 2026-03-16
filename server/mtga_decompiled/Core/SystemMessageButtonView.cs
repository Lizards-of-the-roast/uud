using System;
using TMPro;
using UnityEngine;

public class SystemMessageButtonView : MonoBehaviour
{
	[SerializeField]
	private CustomButton _button;

	[SerializeField]
	private GameObject _externalLinkIcon;

	private static readonly int Disabled = Animator.StringToHash("Disabled");

	public void Init(SystemMessageManager.SystemMessageButtonData buttonData, Action<SystemMessageManager.SystemMessageButtonData> handleOnClick)
	{
		TextMeshProUGUI[] componentsInChildren = GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		componentsInChildren[0].text = buttonData.Text;
		if (buttonData.IsDisabled)
		{
			_button.Interactable = false;
			GetComponent<Animator>().SetBool(Disabled, value: true);
		}
		if (!string.IsNullOrEmpty(buttonData.AlertText))
		{
			componentsInChildren[1].text = buttonData.AlertText;
			componentsInChildren[1].gameObject.SetActive(value: true);
		}
		_button.OnClick.AddListener(delegate
		{
			handleOnClick(buttonData);
		});
		if (buttonData.IsExternalLink)
		{
			_externalLinkIcon.SetActive(value: true);
		}
		else
		{
			_externalLinkIcon.SetActive(value: false);
		}
		_button.OnMouseover.AddListener(onMouseOver);
	}

	public void SetInteractible(bool enabled)
	{
		_button.Interactable = enabled;
	}

	public void Click()
	{
		_button.Click();
	}

	private void onMouseOver()
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
		}
	}
}
