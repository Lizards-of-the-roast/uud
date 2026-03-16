using UnityEngine;

public class InspectorTextFieldAttribute : PropertyAttribute
{
	public string comparedPropertyName = "";

	public string defaultText = "";

	public InspectorTextFieldAttribute(string _comparedPropertyName = "", string _defaultText = "")
	{
		comparedPropertyName = _comparedPropertyName;
		defaultText = _defaultText;
	}
}
