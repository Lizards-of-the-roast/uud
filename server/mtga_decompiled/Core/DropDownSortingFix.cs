using UnityEngine;

public class DropDownSortingFix : MonoBehaviour
{
	public string sortingLayer;

	private Canvas canvas;

	private void Start()
	{
		canvas = GetComponent<Canvas>();
	}

	private void Update()
	{
		if (!(canvas.sortingLayerName != sortingLayer))
		{
			return;
		}
		canvas.sortingLayerName = sortingLayer;
		Transform child = canvas.rootCanvas.transform.GetChild(canvas.rootCanvas.transform.childCount - 1);
		if (child.gameObject.name == "Blocker")
		{
			Canvas component = child.gameObject.GetComponent<Canvas>();
			if ((bool)component)
			{
				component.sortingLayerName = sortingLayer;
			}
			else
			{
				Debug.LogWarning("Warrning: Atempted to fix 'Blocker' sorting for a drop down menu but it was found not to have a canvas to fix.");
			}
		}
		else
		{
			Debug.LogWarning("Warrning: Atempted to fix 'Blocker' sorting for a drop down menu but something moved the blocker somewhere else.");
		}
	}
}
