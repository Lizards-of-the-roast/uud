using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public interface ISubHeaderTextProvider
{
	string GetText(Prompt prompt = null, string defaultKey = null);
}
