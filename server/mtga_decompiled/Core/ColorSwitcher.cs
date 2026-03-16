using UnityEngine;
using UnityEngine.UI;

public class ColorSwitcher : MonoBehaviour
{
	[SerializeField]
	private Image _imageSource;

	[SerializeField]
	private Color[] _availableColors;

	private Color _originalColor;

	private void Awake()
	{
		_originalColor = _imageSource.color;
	}

	public void SwitchColor(int colorIndex)
	{
		if (colorIndex < 0 || colorIndex >= _availableColors.Length)
		{
			_imageSource.color = _originalColor;
		}
		else
		{
			_imageSource.color = _availableColors[colorIndex];
		}
	}
}
