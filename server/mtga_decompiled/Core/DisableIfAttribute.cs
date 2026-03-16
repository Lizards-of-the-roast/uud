using UnityEngine;

public class DisableIfAttribute : PropertyAttribute
{
	public bool toDisable;

	public string boolVarName = "";

	public DisableIfAttribute(string _boolVarName)
	{
		boolVarName = _boolVarName;
	}
}
