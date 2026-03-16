namespace Wotc.Mtga.DuelScene.Interactions;

public interface IScryishBrowserProvider : IBasicBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider
{
	int NthFromTop { get; }

	int NthFromBot { get; }
}
