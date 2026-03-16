using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Interactions.SelectCounters;

public class BrowserTextProvider : IBrowserTextProvider
{
	private readonly IEntityNameProvider<uint> _nameProvider;

	private readonly IClientLocProvider _locProvider;

	public BrowserTextProvider(IEntityNameProvider<uint> nameProvider, IClientLocProvider locProvider)
	{
		_locProvider = locProvider ?? NullLocProvider.Default;
		_nameProvider = nameProvider ?? NullIdNameProvider.Default;
	}

	public BrowserText GetBrowserText(uint sourceId, uint count)
	{
		string key = ((count > 1) ? "DuelScene/Interaction/SelectCounters/ChooseCountersToRemove" : "DuelScene/Interaction/SelectCounters/ChooseCounterToRemove");
		return new BrowserText(_locProvider.GetLocalizedText("DuelScene/ClientPrompt/ChooseX_Choose", ("quantity", count.ToString("N0"))), _locProvider.GetLocalizedText(key, ("cardname", _nameProvider.GetName(sourceId))));
	}
}
