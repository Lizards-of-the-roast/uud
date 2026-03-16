using System;

namespace Wotc.Mtga.DuelScene.Interactions.OptionalAction;

public readonly struct BrowserText
{
	public readonly string Header;

	public readonly string Subheader;

	public readonly string YesText;

	public readonly string NoText;

	public readonly (string, string)[] Params;

	public BrowserText(string header, string subHeader, string yesText, string noText, (string, string)[] parameters)
	{
		Header = header ?? string.Empty;
		Subheader = subHeader ?? string.Empty;
		YesText = yesText ?? string.Empty;
		NoText = noText ?? string.Empty;
		Params = parameters ?? Array.Empty<(string, string)>();
	}
}
