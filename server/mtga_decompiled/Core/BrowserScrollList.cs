using UnityEngine;
using UnityEngine.UI;

public class BrowserScrollList : MonoBehaviour
{
	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	private Transform listParent;

	[SerializeField]
	private Image background;

	[SerializeField]
	private RectMask2D rectMask;

	private bool scrollElementsVisible = true;

	public bool HideScrollElementsWithScrollBar { get; set; }

	public void AddListItem(Transform listItem)
	{
		listItem.transform.SetParent(listParent, worldPositionStays: false);
	}

	private void ShowScrollElements(bool showElements)
	{
		background.enabled = showElements;
		rectMask.enabled = showElements;
		scrollElementsVisible = showElements;
	}

	public void LateUpdate()
	{
		if (HideScrollElementsWithScrollBar && scrollElementsVisible != scrollbar.gameObject.activeSelf)
		{
			ShowScrollElements(scrollbar.gameObject.activeSelf);
		}
	}
}
