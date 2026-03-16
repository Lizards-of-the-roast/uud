using TMPro;
using UnityEngine;

public class Widget_TextBox : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _valueText;

	private int? _valueCache;

	private void Awake()
	{
		_valueText.text = "";
	}

	public void SetValue(int value)
	{
		if (!_valueCache.HasValue || _valueCache.Value != value)
		{
			_valueCache = value;
			_valueText.text = _valueCache.Value.ToString("N0");
		}
	}
}
