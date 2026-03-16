using MovementSystem;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Battlefield;

public class BattlefieldCardHolder_MP : BattlefieldCardHolder, IBattlefieldCardHolder, ICardHolder
{
	public BattlefieldRegionDefinition MultiplayerSlushRegion = new BattlefieldRegionDefinition();

	public override void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		base.Init(gameManager, cardViewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
		IContext context = gameManager.Context;
		IBattlefieldStackFactory stackFactory = new BattlefieldStackFactory(context, new CanStackComparer(context, gameManager.ReferenceMapAggregate, gameManager.UIManager));
		base.Layout = (_battlefieldLayout = new BattlefieldLayout_MP(this, _gameManager.Logger, stackFactory, context, _gameManager.InteractionSystem, _gameManager.NpeDirector));
	}

	void IBattlefieldCardHolder.SetOpponentFocus(params uint[] playerIds)
	{
		(base.Layout as BattlefieldLayout_MP).FocusPlayerIds = playerIds;
		LayoutNow();
	}
}
