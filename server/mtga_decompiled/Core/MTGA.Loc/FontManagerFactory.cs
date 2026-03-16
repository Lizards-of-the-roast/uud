using System;
using Wizards.Mtga;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Loc.CachingPatterns;

namespace MTGA.Loc;

public static class FontManagerFactory
{
	public static IFontProvider Create()
	{
		return new FontProvider(Pantry.Get<IClientLocProvider>(), new DictionaryCache<string, FontMaterialMap>(25, StringComparer.OrdinalIgnoreCase));
	}
}
