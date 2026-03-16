using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorChangingController : MonoBehaviour
{
	[Tooltip("A list of colors to cycle between")]
	public List<Color> ColorList = new List<Color>();

	[Tooltip("Amount of time to take when cycling between two colors")]
	public float CycleTime;

	private Color _startColor;

	private Color _goalColor;

	private int _colorIndex;

	private float _timer;

	private Image _image;

	private TextMeshProUGUI _text;

	private void Start()
	{
		ResetColor();
		_image = GetComponent<Image>();
		if (_image == null)
		{
			_text = GetComponent<TextMeshProUGUI>();
			if (_text == null)
			{
				Debug.LogWarning("ColorChangingController script does not have a text or image component assigned on " + base.gameObject.name + ".", base.gameObject);
			}
		}
	}

	private void ResetColor()
	{
		_colorIndex = 0;
		ComputeNewColor();
	}

	private void ComputeNewColor()
	{
		if (ColorList != null && ColorList.Count != 0)
		{
			_colorIndex = Mathf.Clamp(_colorIndex, 0, ColorList.Count - 1);
			_startColor = ColorList[_colorIndex];
			_colorIndex++;
			if (_colorIndex >= ColorList.Count)
			{
				_colorIndex = 0;
			}
			_goalColor = ColorList[_colorIndex];
			_timer = 0f;
			UpdateColor(0f);
		}
	}

	private void UpdateColor(float dT)
	{
		_timer += dT;
		if (_timer > CycleTime)
		{
			ComputeNewColor();
			return;
		}
		float t = _timer / CycleTime;
		Color color = Color.Lerp(_startColor, _goalColor, t);
		if (_image != null)
		{
			_image.color = color;
		}
		else if (_text != null)
		{
			_text.color = color;
		}
	}

	private void Update()
	{
		UpdateColor(Time.deltaTime);
	}
}
