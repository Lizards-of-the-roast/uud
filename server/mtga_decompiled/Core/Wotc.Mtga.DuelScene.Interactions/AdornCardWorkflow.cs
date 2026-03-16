using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Browsers;

namespace Wotc.Mtga.DuelScene.Interactions;

public class AdornCardWorkflow : WorkflowBase<StringInputRequest>
{
	private const string RESET_OPTION_TEXT = "Reset";

	private static IReadOnlyCollection<string> ALL_SKIN_CODES;

	private readonly IDatabaseUtilities _cardDatabaseUtils;

	private readonly IBrowserController _browserController;

	public AdornCardWorkflow(StringInputRequest req, IBrowserController browserController, IDatabaseUtilities cardDataProvider)
		: base(req)
	{
		_browserController = browserController ?? NullBrowserController.Default;
		_cardDatabaseUtils = cardDataProvider ?? new NullDatabaseUtilities();
	}

	protected override void ApplyInteractionInternal()
	{
		IReadOnlyCollection<string> allSkinCodes = GetAllSkinCodes();
		KeywordSelectionBrowserProvider keywordSelectionBrowserProvider = new KeywordSelectionBrowserProvider(allSkinCodes, allSkinCodes, 0u, 1u, OnBrowserSubmit, _request.CanCancel, string.Empty, "Select card skin code");
		IBrowser openedBrowser = _browserController.OpenBrowser(keywordSelectionBrowserProvider);
		keywordSelectionBrowserProvider.SetOpenedBrowser(openedBrowser);
	}

	private void OnBrowserSubmit(IReadOnlyList<string> submissions)
	{
		_request.SubmitValue(submissions?.FirstOrDefault());
	}

	private IReadOnlyCollection<string> GetAllSkinCodes()
	{
		if (ALL_SKIN_CODES == null)
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (KeyValuePair<uint, CardPrintingData> allPrinting in _cardDatabaseUtils.GetAllPrintings())
			{
				hashSet.UnionWith(allPrinting.Value.KnownSupportedStyles);
			}
			ALL_SKIN_CODES = (IReadOnlyCollection<string>)(object)hashSet.OrderBy((string x) => x).Prepend("Reset").ToArray();
		}
		return ALL_SKIN_CODES;
	}
}
