using UnityEngine;

public class InspectorSimpleHeaderAttribute : PropertyAttribute
{
	public readonly string labelNote = "";

	public readonly string colorHTMLString;

	public readonly Color color;

	public readonly float textHeightIncrease;

	public InspectorSimpleHeaderAttribute(string _labelNote, string _colorHTMLString = "white")
	{
		labelNote = _labelNote;
		colorHTMLString = _colorHTMLString;
		if (!ColorUtility.TryParseHtmlString(colorHTMLString, out color))
		{
			colorHTMLString = "white";
		}
	}
}
