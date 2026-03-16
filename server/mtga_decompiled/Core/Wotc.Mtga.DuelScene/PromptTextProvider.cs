using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class PromptTextProvider : IPromptTextProvider
{
	private const string PARAM_NUMBER = "number";

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IPromptEngine _promptEngine;

	private readonly IGameStateProvider _gameStateProvider;

	public PromptTextProvider(ICardDatabaseAdapter cardDatabase, IPromptEngine promptEngine, IGameStateProvider gameStateProvider)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_promptEngine = promptEngine ?? NullPromptEngine.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public string GetPromptText(Prompt prompt)
	{
		if (!ShouldSkipPrompt(prompt))
		{
			return GetPromptTextInternal(prompt, _gameStateProvider.LatestGameState);
		}
		return string.Empty;
	}

	private string GetPromptTextInternal(Prompt prompt, MtgGameState gameState)
	{
		string promptText = _promptEngine.GetPromptText(prompt, gameState, _cardDatabase);
		int count = gameState.RemainingSelections.Count;
		if (count == 0)
		{
			return promptText;
		}
		string localizedText = _cardDatabase.ClientLocProvider.GetLocalizedText("DuelScene/Prompt/ActionRemaining", ("number", count.ToString()));
		return promptText + " " + localizedText;
	}

	public static bool ShouldSkipPrompt(Prompt prompt)
	{
		if (prompt != null)
		{
			return ShouldSkipPrompt(prompt.PromptId);
		}
		return true;
	}

	private static bool ShouldSkipPrompt(uint promptID)
	{
		return promptID switch
		{
			1u => true, 
			2u => true, 
			36u => true, 
			37u => true, 
			46u => true, 
			118u => true, 
			_ => false, 
		};
	}
}
