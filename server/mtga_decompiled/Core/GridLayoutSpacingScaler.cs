using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GridLayoutSpacingScaler : MonoBehaviour
{
	private int _lastScreenWidth;

	private int _lastScreenHeight;

	public float initialCellWidth;

	public float initialCellHeight;

	public Vector2 initialSize;

	private GridLayoutGroup _gridLayoutGroup;

	private RectTransform _rectTransform;

	private void Start()
	{
		_gridLayoutGroup = GetComponent<GridLayoutGroup>();
		_rectTransform = GetComponent<RectTransform>();
		initialCellWidth = _gridLayoutGroup.cellSize.x;
		initialCellHeight = _gridLayoutGroup.cellSize.y;
		initialSize = _gridLayoutGroup.cellSize;
		_lastScreenWidth = Screen.width;
		_lastScreenHeight = Screen.height;
		StartCoroutine(OnScreenSizeChange());
	}

	private IEnumerator OnScreenSizeChange()
	{
		yield return new WaitForEndOfFrame();
		_gridLayoutGroup.spacing = Vector2.zero;
		float num = _rectTransform.rect.width - (float)_gridLayoutGroup.padding.left - (float)_gridLayoutGroup.padding.right;
		float num2 = Mathf.Floor(num / (initialCellWidth + _gridLayoutGroup.spacing.x));
		float num3 = 0f;
		if (num > initialCellWidth)
		{
			num3 = num % initialCellWidth / num2 / initialCellWidth;
		}
		_gridLayoutGroup.cellSize = initialSize * (1f + num3);
	}
}
