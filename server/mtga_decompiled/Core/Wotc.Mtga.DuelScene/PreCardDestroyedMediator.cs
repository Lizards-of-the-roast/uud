using System;
using MovementSystem;

namespace Wotc.Mtga.DuelScene;

public class PreCardDestroyedMediator : IDisposable
{
	private readonly CardViewBuilder _cardViewBuilder;

	private readonly ISplineMovementSystem _splineMovementSystem;

	public PreCardDestroyedMediator(CardViewBuilder cardViewBuilder, ISplineMovementSystem splineMovementSystem)
	{
		_cardViewBuilder = cardViewBuilder;
		_splineMovementSystem = splineMovementSystem;
		_cardViewBuilder.preCardDestroyEvent += OnPreCardDestroyed;
	}

	private void OnPreCardDestroyed(BASE_CDC cardView)
	{
		if (!(cardView == null))
		{
			_splineMovementSystem.RemovePermanentGoal(cardView.Root);
		}
	}

	public void Dispose()
	{
		_cardViewBuilder.preCardDestroyEvent -= OnPreCardDestroyed;
	}
}
