using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HasbroGo;

public class SelectableTabNavigation : MonoBehaviour
{
	[SerializeField]
	private List<Selectable> selectables;

	private int selectedSelectableFieldIndex;

	private void Update()
	{
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab))
		{
			if (CanTabThrough())
			{
				do
				{
					selectedSelectableFieldIndex = (selectables.Count - 1 + selectedSelectableFieldIndex) % selectables.Count;
				}
				while (!selectables[selectedSelectableFieldIndex].interactable);
				selectables[selectedSelectableFieldIndex].Select();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Tab) && CanTabThrough())
		{
			do
			{
				selectedSelectableFieldIndex = ++selectedSelectableFieldIndex % selectables.Count;
			}
			while (!selectables[selectedSelectableFieldIndex].interactable);
			selectables[selectedSelectableFieldIndex].Select();
		}
	}

	public void OnSelectableSelected(Selectable selectable)
	{
		if (selectable.interactable)
		{
			UpdateSelectedSelectable(selectable);
		}
	}

	private void UpdateSelectedSelectable(Selectable selectable)
	{
		for (int i = 0; i < selectables.Count; i++)
		{
			if (selectables[i] == selectable)
			{
				selectedSelectableFieldIndex = i;
				break;
			}
		}
	}

	private bool CanTabThrough()
	{
		foreach (Selectable selectable in selectables)
		{
			if (selectable.interactable)
			{
				return true;
			}
		}
		return false;
	}
}
