using UnityEngine;

namespace Wizards.Mtga.UI;

[ExecuteAlways]
public class ForceWidthWithScale : MonoBehaviour
{
	[SerializeField]
	private RectTransform _bounds;

	[SerializeField]
	private RectTransform _target;

	[SerializeField]
	private float _margins;

	private float _prevBoundsWidth;

	private float _prevTargetWidth;

	private void Update()
	{
		if (_prevBoundsWidth != _bounds.rect.width || _prevTargetWidth != _target.rect.width)
		{
			UpdateScale();
		}
	}

	[ContextMenu("Update Scale")]
	public void UpdateScale()
	{
		_prevBoundsWidth = _bounds.rect.width;
		_prevTargetWidth = _target.rect.width;
		float num = _bounds.rect.width - _margins;
		if (num < _target.rect.width)
		{
			float num2 = num / _target.rect.width;
			_target.localScale = new Vector3(num2, num2, num2);
		}
		else
		{
			_target.localScale = Vector3.one;
		}
	}
}
