using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Loc;

public class ButtonPhaseContext : MonoBehaviour
{
	[Header("Assets")]
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Localize _text;

	[SerializeField]
	private Image _iconImage;

	private bool _visible;

	private string _string;

	public void SetText(MTGALocalizedString localizedString, Sprite icon = null)
	{
		if (!(_string == localizedString))
		{
			_string = localizedString;
			_text.SetText(localizedString);
			LayoutRebuilder.MarkLayoutForRebuild((RectTransform)_text.transform);
			_iconImage.sprite = icon;
			_iconImage.enabled = icon != null;
			if (_animator.isActiveAndEnabled)
			{
				_animator.SetTrigger("Reintro");
			}
		}
	}

	public void Clear()
	{
		_animator.gameObject.SetActive(value: false);
	}

	public void Show(bool visible)
	{
		_visible = visible;
		_animator.gameObject.SetActive(_visible);
	}
}
