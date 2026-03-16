using Wizards.Arena.DeckValidation.Client;
using Wizards.Mtga.Decks;

public class DeckDisplayInfo
{
	public enum DeckDisplayState
	{
		Valid,
		Craftable,
		Uncraftable,
		Invalid,
		Malformed,
		Unavailable
	}

	public readonly Client_Deck Deck;

	public readonly DeckDisplayState DisplayState;

	public readonly ClientSideDeckValidationResult ValidationResult;

	public readonly string TooltipText;

	public readonly bool UseHistoricLabel;

	public readonly string ColorChallengeEventLock;

	public bool IsValid => DisplayState == DeckDisplayState.Valid;

	public bool IsCraftable => DisplayState == DeckDisplayState.Craftable;

	public bool IsUncraftable => DisplayState == DeckDisplayState.Uncraftable;

	public bool IsUnowned
	{
		get
		{
			if (DisplayState != DeckDisplayState.Uncraftable)
			{
				return DisplayState == DeckDisplayState.Craftable;
			}
			return true;
		}
	}

	public bool IsWarning => DisplayState == DeckDisplayState.Invalid;

	public bool IsMalformed => DisplayState == DeckDisplayState.Malformed;

	public DeckDisplayInfo(Client_Deck deck, DeckDisplayState displayState, ClientSideDeckValidationResult validationResult, string tooltipText = "", bool useHistoricLabel = false, string colorChallengeEventLock = null)
	{
		Deck = deck;
		DisplayState = displayState;
		ValidationResult = validationResult;
		TooltipText = tooltipText;
		UseHistoricLabel = useHistoricLabel;
		ColorChallengeEventLock = colorChallengeEventLock;
	}
}
