using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Wotc.Mtga.Extensions;

public class BoosterVaultView : MonoBehaviour
{
	[SerializeField]
	private CustomButton _chestButton;

	[SerializeField]
	private QuestProgressBar _progressBar;

	public TMP_Text TempProgressLabel;

	public TMP_Text TempProgressQuantity;

	private Coroutine _tempCoroutine;

	[SerializeField]
	public TooltipTrigger TooltipTriggerChest;

	[SerializeField]
	public TooltipTrigger TooltipTriggerBar;

	private float _percent;

	private bool _allowOpen;

	public UnityEvent OnClick => _chestButton.OnClick;

	public void SetActive(bool active)
	{
		base.gameObject.UpdateActive(active);
	}

	public void SetPercent(float percent)
	{
		Debug.LogFormat("setting vault progress percent: {0}", percent);
		_percent = percent;
		_progressBar.Percent = percent;
		float num = percent * 100f;
		if (num > 100f)
		{
			num = 100f;
		}
		TooltipTriggerChest.TooltipData.Text = num.ToString("P1").FixCulture();
		TooltipTriggerBar.TooltipData.Text = num.ToString("P1").FixCulture();
		UpdateInteractable();
	}

	public void UpdatePercent(float percent)
	{
		if (percent > _percent)
		{
			if (_tempCoroutine != null)
			{
				StopCoroutine(_tempCoroutine);
			}
			_tempCoroutine = StartCoroutine(TEMP_ShowProgress(percent - _percent));
		}
		SetPercent(percent);
	}

	public void SetAllowOpen(bool allowOpen)
	{
		_allowOpen = allowOpen;
		UpdateInteractable();
	}

	private void UpdateInteractable()
	{
		_chestButton.Interactable = _allowOpen && _percent >= 1f;
	}

	private void Awake()
	{
		TempProgressQuantity.alpha = 1f;
		TempProgressQuantity.gameObject.UpdateActive(active: false);
		TempProgressLabel.alpha = 1f;
		TempProgressLabel.gameObject.UpdateActive(active: false);
	}

	private IEnumerator TEMP_ShowProgress(float gain)
	{
		TempProgressQuantity.text = $"+{(float)Mathf.RoundToInt(gain * 1000f) * 0.1f}%";
		TempProgressQuantity.gameObject.UpdateActive(active: true);
		TempProgressLabel.gameObject.UpdateActive(active: true);
		yield return new WaitForSeconds(3f);
		TempProgressQuantity.gameObject.UpdateActive(active: false);
		TempProgressLabel.gameObject.UpdateActive(active: false);
		_tempCoroutine = null;
	}
}
