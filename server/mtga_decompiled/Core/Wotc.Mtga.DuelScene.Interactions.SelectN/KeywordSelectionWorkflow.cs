using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.UXEvents;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class KeywordSelectionWorkflow : WorkflowBase<SelectNRequest>
{
	private readonly IBrowserManager _browserManager;

	private readonly KeywordData _keywordData;

	private IBrowser _openedBrowser;

	private readonly IPromptTextProvider _promptTextProvider;

	public KeywordSelectionWorkflow(SelectNRequest request, IBrowserManager browserManager, KeywordData keywordData, IPromptTextProvider promptTextProvider)
		: base(request)
	{
		_keywordData = keywordData;
		_browserManager = browserManager;
		_promptTextProvider = promptTextProvider;
	}

	public override bool CanApply(List<UXEvent> events)
	{
		if (events.Count == 0)
		{
			return !_browserManager.IsAnyBrowserOpen;
		}
		return false;
	}

	public override void CleanUp()
	{
		_openedBrowser?.Close();
		base.CleanUp();
	}

	protected override void ApplyInteractionInternal()
	{
		KeywordSelectionBrowserProvider keywordSelectionBrowserProvider = new KeywordSelectionBrowserProvider(_keywordData.SortedKeywords, _keywordData.HintingOptions, (uint)_request.MinSel, _request.MaxSel, delegate(IReadOnlyList<string> selections)
		{
			if (selections == null)
			{
				_request.Cancel();
			}
			else
			{
				List<uint> list = new List<uint>();
				foreach (string selection in selections)
				{
					list.Add(_keywordData.IdsByKeywords[selection]);
				}
				_request.SubmitSelection(list);
			}
		}, _request.CanCancel, string.Empty, _promptTextProvider.GetPromptText(_request.Prompt));
		_openedBrowser = _browserManager.OpenBrowser(keywordSelectionBrowserProvider);
		keywordSelectionBrowserProvider.SetOpenedBrowser(_openedBrowser);
	}
}
