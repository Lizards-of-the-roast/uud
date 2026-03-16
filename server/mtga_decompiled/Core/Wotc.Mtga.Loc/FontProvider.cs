using MTGA.Loc;
using Wotc.Mtga.Loc.CachingPatterns;

namespace Wotc.Mtga.Loc;

public class FontProvider : IFontProvider
{
	private readonly IClientLocProvider _locProvider;

	private readonly ICachingPattern<string, FontMaterialMap> _fontCache;

	public FontProvider(IClientLocProvider locProvider, ICachingPattern<string, FontMaterialMap> fontCache)
	{
		_locProvider = locProvider ?? NullLocProvider.Default;
		_fontCache = fontCache ?? new NullCache<string, FontMaterialMap>();
	}

	public void OnLanguageChanged(string languageCode)
	{
		_fontCache.ClearCache();
	}

	public FontMaterialMap GetLocalizedFont(string key)
	{
		string text = _locProvider.GetLocalizedText(key);
		if (string.IsNullOrWhiteSpace(text))
		{
			text = Languages.DefaultFontName;
		}
		if (_fontCache.TryGetCached(text, out var value))
		{
			return value;
		}
		FontMaterialMap fontMaterialMap = LocalizationManagerUtilities.LoadFontMaterialFromResources(text);
		_fontCache.SetCached(text, fontMaterialMap);
		return fontMaterialMap;
	}
}
