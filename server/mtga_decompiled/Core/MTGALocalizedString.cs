using System;
using System.Collections.Generic;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

[Serializable]
public class MTGALocalizedString
{
	[LocTerm]
	public string Key;

	public Dictionary<string, string> Parameters;

	private readonly IClientLocProvider _overrideLocProvider;

	public MTGALocalizedString()
	{
	}

	public MTGALocalizedString(IClientLocProvider overrideLocProvider)
	{
		_overrideLocProvider = overrideLocProvider;
	}

	public static implicit operator MTGALocalizedString(string _val)
	{
		return new MTGALocalizedString
		{
			Key = _val
		};
	}

	public static implicit operator string(MTGALocalizedString s)
	{
		if (s == null)
		{
			return string.Empty;
		}
		return s.ToString();
	}

	public override string ToString()
	{
		return (_overrideLocProvider ?? Languages.ActiveLocProvider)?.GetLocalizedText(Key, Parameters.AsTuples());
	}
}
