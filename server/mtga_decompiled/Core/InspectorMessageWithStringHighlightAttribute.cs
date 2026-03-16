using UnityEngine;

public class InspectorMessageWithStringHighlightAttribute : PropertyAttribute
{
	public string varNameToHighlight = "";

	public string textToDisplayLeft = "";

	public string textToDisplayRight = "";

	public string defaultStateString = "";

	public readonly string colorHTMLString;

	public readonly Color color;

	public InspectorMessageWithStringHighlightAttribute(string _varNameToHighlight, string _colorHTMLString = "green", string _textToDisplayLeft = "", string _textToDisplayRight = "", string _defaultStateString = "")
	{
		varNameToHighlight = _varNameToHighlight;
		textToDisplayLeft = _textToDisplayLeft;
		textToDisplayRight = _textToDisplayRight;
		defaultStateString = _defaultStateString;
		colorHTMLString = _colorHTMLString;
		if (!ColorUtility.TryParseHtmlString(colorHTMLString, out color))
		{
			colorHTMLString = "green";
		}
	}
}
