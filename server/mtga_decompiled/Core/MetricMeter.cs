using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetricMeter : MonoBehaviour
{
	[SerializeField]
	public SetCollectionController.Metrics _metric;

	[SerializeField]
	private Image _completionMeter;

	[SerializeField]
	private TMP_Text _completionNumbers;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Image _metricIcon;

	private static readonly int COMPLETIONSTATE_INT = Animator.StringToHash("Completion");

	public void UpdateUI(int numOwned, int numAvailable, bool isFourOf = false)
	{
		_completionMeter.fillAmount = (float)numOwned / (float)numAvailable;
		_completionNumbers.SetText($"{numOwned}/{numAvailable}");
		int value = Convert.ToInt32(numOwned >= numAvailable && isFourOf) + Convert.ToInt32(isFourOf) + Convert.ToInt32(numOwned > 0);
		_animator.SetInteger(COMPLETIONSTATE_INT, value);
	}

	public void UpdateMetricIcon(Sprite metricIcon)
	{
		_metricIcon.sprite = metricIcon;
	}
}
