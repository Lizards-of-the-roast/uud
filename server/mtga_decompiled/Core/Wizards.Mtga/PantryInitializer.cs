using System;
using System.Collections.Generic;

namespace Wizards.Mtga;

public static class PantryInitializer
{
	public static Pantry InitializePantry()
	{
		return Pantry.Create(new Dictionary<Pantry.Scope, Dictionary<Type, Func<object>>>
		{
			{
				Pantry.Scope.Static,
				PantryServices.Static
			},
			{
				Pantry.Scope.Application,
				PantryServices.Application
			},
			{
				Pantry.Scope.Environment,
				PantryServices.Environment
			},
			{
				Pantry.Scope.Wrapper,
				PantryServices.Wrapper
			},
			{
				Pantry.Scope.Deckbuilder,
				PantryServices.DeckBuilder
			}
		});
	}
}
