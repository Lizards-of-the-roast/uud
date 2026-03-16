using UnityEngine;

public class InspectorDropDrownAttribute : PropertyAttribute
{
	public string listName = "";

	public InspectorDropDrownAttribute(string _listName)
	{
		listName = _listName;
	}
}
