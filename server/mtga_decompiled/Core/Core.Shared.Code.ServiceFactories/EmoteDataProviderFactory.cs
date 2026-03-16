using System.Collections.Generic;

namespace Core.Shared.Code.ServiceFactories;

public class EmoteDataProviderFactory
{
	public static IEmoteDataProvider Create()
	{
		return new EmoteDatabase((IReadOnlyCollection<ClientEmoteEntry>)(object)new ClientEmoteEntry[11]
		{
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_Hello"),
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_Nice_Thanks"),
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_Thanks"),
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_Nice"),
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_Thinking_YourGo"),
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_Thinking"),
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_YourGo"),
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_Oops_Sorry"),
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_Oops"),
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_Sorry"),
			ClientEmoteEntry.DefaultPhrase("Phrase_Basic_GoodGame")
		});
	}
}
