using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Wotc.Mtga.Unity;

public class SelectionGroup : MonoBehaviour
{
	private readonly HashSet<GameObject> _selectable = new HashSet<GameObject>();

	private GameObject _previousSelection;

	public event Action Selected;

	public event Action Deselected;

	private void Update()
	{
		if (_selectable.Count == 0)
		{
			return;
		}
		_selectable.RemoveWhere((GameObject x) => !x);
		EventSystem current = EventSystem.current;
		if (!current)
		{
			return;
		}
		GameObject currentSelectedGameObject = current.currentSelectedGameObject;
		if (_previousSelection != currentSelectedGameObject)
		{
			if (GroupWasSelected(_previousSelection, currentSelectedGameObject))
			{
				this.Selected?.Invoke();
			}
			else if (GroupWasDeselected(_previousSelection, currentSelectedGameObject))
			{
				this.Deselected?.Invoke();
			}
			_previousSelection = currentSelectedGameObject;
		}
	}

	private bool GroupWasSelected(GameObject previous, GameObject current)
	{
		if (!ValidSelection(previous))
		{
			return ValidSelection(current);
		}
		return false;
	}

	private bool GroupWasDeselected(GameObject previous, GameObject current)
	{
		if (ValidSelection(previous))
		{
			return !ValidSelection(current);
		}
		return false;
	}

	private bool ValidSelection(GameObject selection)
	{
		if ((bool)selection)
		{
			foreach (GameObject item in _selectable)
			{
				if (item == selection || selection.transform.IsChildOf(item.transform))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddSelectable(GameObject gameObj)
	{
		if ((bool)gameObj)
		{
			_selectable.Add(gameObj);
		}
	}

	private void OnDestroy()
	{
		_selectable.Clear();
		this.Deselected = null;
	}
}
