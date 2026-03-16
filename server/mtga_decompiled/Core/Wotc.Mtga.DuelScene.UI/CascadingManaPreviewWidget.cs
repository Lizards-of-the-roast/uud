using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UI;

public class CascadingManaPreviewWidget : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Image sprite;

	[SerializeField]
	private Image _highlight;

	[SerializeField]
	private TextMeshProUGUI _quantity;

	[SerializeField]
	private Transform vfxAnchor;

	[Header("Animation Triggers")]
	[SerializeField]
	private string _hiddenBool = "Hidden";

	[SerializeField]
	private string _introBool = "Intro";

	[SerializeField]
	private string _highlightBool = "Highlight";

	[SerializeField]
	private string _previewBool = "Preview";

	[SerializeField]
	private string _setBool = "Set";

	public void Highlight()
	{
		SetAnimBool(_previewBool, value: false);
		SetAnimBool(_highlightBool, value: true);
	}

	public void Preview(Sprite icon)
	{
		sprite.sprite = icon;
		sprite.gameObject.UpdateActive(active: false);
		SetAnimBool(_previewBool, value: true);
	}

	public void AutoPreview(Sprite icon)
	{
		SetAnimBool(_introBool, value: false);
		SetAnimBool(_hiddenBool, value: false);
		SetAnimBool(_highlightBool, value: true);
		Preview(icon);
	}

	public void Intro()
	{
		SetAnimBool(_hiddenBool, value: false);
		SetAnimBool(_introBool, value: true);
	}

	public void ResetWidget()
	{
		sprite.sprite = null;
		sprite.gameObject.UpdateActive(active: false);
		SetAnimBool(_hiddenBool, value: true);
		SetAnimBool(_highlightBool, value: false);
		SetAnimBool(_previewBool, value: false);
		SetAnimBool(_setBool, value: false);
		SetQuantity(0u);
		base.transform.ZeroOut();
		Color color = _highlight.color;
		color.a = 0f;
		_highlight.color = color;
		sprite.gameObject.UpdateActive(active: false);
	}

	public void Set(Sprite icon, GameObject vfx)
	{
		sprite.sprite = icon;
		SetAnimBool(_setBool, value: true);
		if ((bool)vfx)
		{
			vfx.transform.SetParent(vfxAnchor);
			vfx.transform.ZeroOut();
			vfx.transform.SetParent(null);
		}
	}

	public void SetQuantity(uint value)
	{
		if (value <= 1)
		{
			_quantity.text = string.Empty;
			_quantity.gameObject.UpdateActive(active: false);
		}
		else
		{
			_quantity.text = value.ToString();
			_quantity.gameObject.UpdateActive(active: true);
		}
	}

	private void SetAnimBool(string boolean, bool value)
	{
		if ((bool)animator)
		{
			animator.SetBool(boolean, value);
		}
	}
}
