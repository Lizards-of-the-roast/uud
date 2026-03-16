using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class ZoneCountElement : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Color _highlightColor = Color.yellow;

	[SerializeField]
	private Image _maxSizeIcon;

	public void SetVisible(bool visible)
	{
		base.gameObject.UpdateActive(visible);
	}

	public void SetCount(int count)
	{
		bool flag = count == int.MaxValue;
		_text.text = (flag ? " " : count.ToString());
		if (_maxSizeIcon != null)
		{
			_maxSizeIcon.enabled = flag;
		}
	}

	public void SetHighlighted(bool highlighted)
	{
		Color color = (highlighted ? _highlightColor : Color.white);
		_text.color = color;
		_icon.color = color;
		if (_maxSizeIcon != null)
		{
			_maxSizeIcon.color = color;
		}
	}
}
