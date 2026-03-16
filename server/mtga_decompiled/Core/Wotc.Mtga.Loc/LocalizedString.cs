using System;

namespace Wotc.Mtga.Loc;

[Serializable]
public struct LocalizedString
{
	public string mTerm;

	public bool mRTL_IgnoreArabicFix;

	public int mRTL_MaxLineLength;

	public bool mRTL_ConvertNumbers;

	public bool m_DontLocalizeParameters;

	private IClientLocProvider _locMan;

	public static implicit operator string(LocalizedString s)
	{
		return s.ToString();
	}

	public static implicit operator LocalizedString(string term)
	{
		return new LocalizedString
		{
			mTerm = term
		};
	}

	public LocalizedString(LocalizedString str, IClientLocProvider locMan = null)
	{
		_locMan = locMan;
		mTerm = str.mTerm;
		mRTL_IgnoreArabicFix = str.mRTL_IgnoreArabicFix;
		mRTL_MaxLineLength = str.mRTL_MaxLineLength;
		mRTL_ConvertNumbers = str.mRTL_ConvertNumbers;
		m_DontLocalizeParameters = str.m_DontLocalizeParameters;
	}

	public override string ToString()
	{
		if (_locMan == null)
		{
			_locMan = Languages.ActiveLocProvider;
		}
		return _locMan.GetLocalizedText(mTerm);
	}
}
