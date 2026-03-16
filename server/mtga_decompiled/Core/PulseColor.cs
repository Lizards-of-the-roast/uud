using UnityEngine;
using UnityEngine.UI;

public class PulseColor : MonoBehaviour
{
	public Image TargetImage;

	public float MinColor = 0.5f;

	public float MaxColor = 1f;

	public float PulseTime = 1f;

	private float _currentPulseTime;

	private Color _startColor;

	private void Awake()
	{
		_startColor = TargetImage.color;
		_currentPulseTime = 0f;
	}

	private void Update()
	{
		_currentPulseTime += Time.deltaTime;
		if (_currentPulseTime > 2f * PulseTime)
		{
			_currentPulseTime = 0f;
		}
		float num = 0f;
		if ((double)_currentPulseTime >= 0.0 && _currentPulseTime < PulseTime)
		{
			float num2 = _currentPulseTime / PulseTime;
			num = MinColor + (MaxColor - MinColor) * (1f - num2);
		}
		else
		{
			float num3 = (_currentPulseTime - PulseTime) / PulseTime;
			num = MinColor + (MaxColor - MinColor) * num3;
		}
		TargetImage.color = _startColor * num;
	}
}
