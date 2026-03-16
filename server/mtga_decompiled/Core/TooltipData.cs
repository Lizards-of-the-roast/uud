using System;
using Wotc.Mtga.Loc;

[Serializable]
public class TooltipData
{
	[Obsolete("Set localized strings to TooltipData.Text")]
	public string TooltipText;

	public TooltipSystem.TooltipPositionAnchor RelativePosition;

	public TooltipSystem.TooltipStyle TooltipStyle;

	private IClientLocProvider _locMan;

	protected string _text = "UNSET";

	private const string UNSET_TEXT = "UNSET";

	public IClientLocProvider LocMan
	{
		get
		{
			if (_locMan == null)
			{
				_locMan = Languages.ActiveLocProvider;
			}
			return _locMan;
		}
		set
		{
			_locMan = value;
		}
	}

	public virtual string Text
	{
		get
		{
			if (_text == "UNSET")
			{
				if (!string.IsNullOrEmpty(TooltipText))
				{
					return "~" + TooltipText + "~";
				}
				return "";
			}
			if (LocMan.DoesContainTranslation(_text))
			{
				return LocMan.GetLocalizedText(_text);
			}
			return _text;
		}
		set
		{
			_text = value;
		}
	}
}
