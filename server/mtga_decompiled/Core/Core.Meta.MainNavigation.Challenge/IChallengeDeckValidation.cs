using System;
using Wizards.Arena.DeckValidation.Client;

namespace Core.Meta.MainNavigation.Challenge;

public interface IChallengeDeckValidation
{
	ClientSideDeckValidationResult ValidateDeck(Guid deckId, string format);
}
