using MTGA.Loc;

namespace Wotc.Mtga.Loc;

public class NullFontProvider : IFontProvider
{
	public void OnLanguageChanged(string languageCode)
	{
	}

	public FontMaterialMap GetLocalizedFont(string key)
	{
		return null;
	}
}
