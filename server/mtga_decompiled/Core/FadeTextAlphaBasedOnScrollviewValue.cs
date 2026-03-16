using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class FadeTextAlphaBasedOnScrollviewValue : MonoBehaviour
{
	[SerializeField]
	private float _multiplier = 1f;

	private TMP_Text _label;

	private Color _startingColor;

	private void Awake()
	{
		_label = GetComponent<TMP_Text>();
		_startingColor = _label.color;
	}

	public void ScrollViewValueChanged(Vector2 newScrollviewValues)
	{
		float t = (1f - newScrollviewValues.y) * _multiplier;
		_label.color = Color.Lerp(_startingColor, Color.clear, t);
	}

	public void Reset()
	{
		_label.color = _startingColor;
	}
}
