using System;
using Wotc.Mtga.Cards.Database;

internal class GRELocalizedString : MTGALocalizedString
{
	public uint locId;

	public bool forceUpper;

	private readonly IGreLocProvider _locManager;

	public GRELocalizedString(IGreLocProvider locManager)
	{
		_locManager = locManager ?? throw new NullReferenceException("IGreLocProvider");
	}

	public override string ToString()
	{
		string localizedText = _locManager.GetLocalizedText(locId, null, formatted: false);
		if (forceUpper)
		{
			return localizedText.ToUpper();
		}
		return localizedText;
	}
}
