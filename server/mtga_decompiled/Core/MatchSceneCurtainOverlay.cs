using UnityEngine;

public class MatchSceneCurtainOverlay : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _curtainCanvasGroup;

	[SerializeField]
	private float _fadeSpeed = 1f;

	private bool _enabled = true;

	public float CurtainAlpha => _curtainCanvasGroup.alpha;

	public void SetEnabled(bool enabled)
	{
		_enabled = enabled;
		if (_enabled && !base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	private void Update()
	{
		float num = (_enabled ? 1 : 0);
		float num2 = _curtainCanvasGroup.alpha;
		if (num != num2)
		{
			float num3 = Time.deltaTime * _fadeSpeed * (Mathf.Approximately(num, 0f) ? (-1f) : 1f);
			num2 = Mathf.Clamp01(num2 + num3);
			_curtainCanvasGroup.alpha = num2;
		}
		if (num == 0f && num2 == 0f)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
