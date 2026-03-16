using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles;

public class SimpleAssetBundleProvisionProgressReporter : MonoBehaviour, IProgress<AssetBundleProvisionProgress>
{
	private const float MEG = 1000000f;

	public TMP_Text ProgressText;

	public Slider ProgressBar;

	private AssetBundleProvisionProgress _progress = new AssetBundleProvisionProgress(AssetBundleProvisionStage.None, 0L, 0L);

	public void Report(AssetBundleProvisionProgress value)
	{
		_progress = value;
	}

	private void Update()
	{
		if (_progress.Total > 0)
		{
			ProgressText.text = $"Downloaded: {(float)_progress.Completed / 1000000f:F1}M / {(float)_progress.Total / 1000000f:F1}M";
			ProgressBar.value = Mathf.Clamp01((float)_progress.Completed / (float)_progress.Total);
		}
	}
}
