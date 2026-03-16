using UnityEngine;

public class HideIfAttribute : PropertyAttribute
{
	public bool toHide;

	public string boolVarName = "";

	public HideIfAttribute(string _boolVarName)
	{
		boolVarName = _boolVarName;
	}
}
