using System;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public class ClearDynamicAbilitiesMediator : IDisposable
{
	private readonly IDynamicAbilityDataProvider _dynamicAbilityProvider;

	public ClearDynamicAbilitiesMediator(IDynamicAbilityDataProvider dynamicAbilityProvider)
	{
		_dynamicAbilityProvider = dynamicAbilityProvider ?? NullDynamicAbilityDataProvider.Default;
	}

	public void Dispose()
	{
		_dynamicAbilityProvider.Clear();
	}
}
