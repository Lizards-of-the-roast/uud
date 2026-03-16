using UnityEngine;

namespace Core.Meta.MainNavigation.NavBar;

[RequireComponent(typeof(RectTransform))]
public class NavBarMidBackgroundResizer : MonoBehaviour
{
	[SerializeField]
	private RectTransform _midButtonsRectTransform;

	[SerializeField]
	private RectTransform _midFlexibleSpaceRectTransform;

	[SerializeField]
	private RectTransform _currenciesContainerRectTransform;

	[SerializeField]
	private float _widthOffset;

	[SerializeField]
	private RectTransform _targetRectTransform;

	private void OnRectTransformDimensionsChange()
	{
		Vector2 sizeDelta = _targetRectTransform.sizeDelta;
		sizeDelta.x = _midButtonsRectTransform.rect.width + _midFlexibleSpaceRectTransform.rect.width + _currenciesContainerRectTransform.rect.width + _widthOffset;
		_targetRectTransform.sizeDelta = sizeDelta;
	}
}
