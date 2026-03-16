using MTGA.Loc;

namespace Wotc.Mtga.Loc;

public interface IFontProvider
{
	void OnLanguageChanged(string languageCode);

	FontMaterialMap GetLocalizedFont(string key);
}
