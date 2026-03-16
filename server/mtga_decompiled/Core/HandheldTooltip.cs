using System.Collections;
using TMPro;
using UnityEngine;

public class HandheldTooltip : MonoBehaviour
{
	[SerializeField]
	private TooltipTrigger TooltipTriggerToBeReplaced;

	[SerializeField]
	private Animator HandheldTooltipAnimator;

	[SerializeField]
	private float ToggleDurationInSeconds = 2.5f;

	[SerializeField]
	private TextMeshProUGUI TextBox;

	private void Awake()
	{
		UpdateText();
	}

	public void OnEnable()
	{
		UpdateText();
		StartCoroutine(TogglePopup());
	}

	private IEnumerator TogglePopup()
	{
		yield return new WaitForSeconds(ToggleDurationInSeconds);
		HandheldTooltipAnimator.SetTrigger("TurnOff");
	}

	private void UpdateText()
	{
		if ((bool)TooltipTriggerToBeReplaced)
		{
			TextBox.text = TooltipTriggerToBeReplaced.GetTooltipText();
		}
	}
}
