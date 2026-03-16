using AssetLookupTree;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class KeywordSelectionTranslation : IWorkflowTranslation<SelectNRequest>
{
	private readonly GameManager _gameManager;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly ICardHolderManager _cardHolderManager;

	private readonly IGameplaySettingsProvider _gameplaySettings;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IBrowserManager _browserManager;

	private readonly IPromptTextProvider _promptTextProvider;

	private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

	private readonly IResolutionEffectProvider _resolutionEffectProvider;

	private readonly IBrowserHeaderTextProvider _headerTextProvider;

	public KeywordSelectionTranslation(IContext context, AssetLookupSystem assetLookupSystem, GameManager gameManager)
	{
		_assetLookupSystem = assetLookupSystem;
		_cardDatabase = context.Get<ICardDatabaseAdapter>();
		_cardHolderManager = context.Get<ICardHolderManager>();
		_gameplaySettings = context.Get<IGameplaySettingsProvider>();
		_gameStateProvider = context.Get<IGameStateProvider>();
		_browserManager = context.Get<IBrowserManager>();
		_promptTextProvider = context.Get<IPromptTextProvider>();
		_cardBuilder = context.Get<ICardBuilder<DuelScene_CDC>>();
		_resolutionEffectProvider = context.Get<IResolutionEffectProvider>();
		_headerTextProvider = context.Get<IBrowserHeaderTextProvider>();
		_gameManager = gameManager;
	}

	public WorkflowBase Translate(SelectNRequest req)
	{
		KeywordData keywordData = KeywordData.Generate(req, _gameStateProvider.LatestGameState, _gameManager.PromptEngine, _gameManager);
		if (UsePromptButtonWorkflow(keywordData, req))
		{
			return new KeywordPromptButtonsWorkflow(req, keywordData, _cardDatabase, _resolutionEffectProvider, _assetLookupSystem);
		}
		if (keywordData.IdsByKeywords.Count <= 6 && req.MinSel == 1 && req.MaxSel == 1)
		{
			return new KeywordButtonsWorkflow(req, keywordData, _assetLookupSystem, _cardDatabase, _cardHolderManager, _gameStateProvider, _gameplaySettings, _browserManager);
		}
		if (keywordData.SortedKeywords.Count > 3)
		{
			return new KeywordSelectionWorkflow(req, _browserManager, keywordData, _promptTextProvider);
		}
		return new KeywordModalWorkflow(req, keywordData, _cardDatabase, _gameStateProvider, _cardBuilder, _browserManager, _headerTextProvider);
	}

	public static bool UsePromptButtonWorkflow(KeywordData keywordData, SelectNRequest request)
	{
		if (request.MinSel == 1 && request.MaxSel == 1)
		{
			if (keywordData.IdsByKeywords.Count != 2 || request.CanCancel)
			{
				if (keywordData.IdsByKeywords.Count == 1)
				{
					return request.CancellationType == AllowCancel.Continue;
				}
				return false;
			}
			return true;
		}
		return false;
	}
}
