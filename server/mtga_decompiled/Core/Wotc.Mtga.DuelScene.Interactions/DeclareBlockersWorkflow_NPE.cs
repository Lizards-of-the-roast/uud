using System;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Interactions;

public class DeclareBlockersWorkflow_NPE : DeclareBlockersWorkflow
{
	private readonly NPEDirector _director;

	public DeclareBlockersWorkflow_NPE(DeclareBlockersRequest request, NPEDirector director, IObjectPool objectPool, IClientLocProvider clientLocProvider, IPromptEngine promptEngine, IGameStateProvider gameStateProvider, ICardViewProvider cardViewProvider, IBattlefieldCardHolder battlefieldCardHolder, UIManager uiManager)
		: base(request, objectPool, clientLocProvider, promptEngine, gameStateProvider, cardViewProvider, battlefieldCardHolder, uiManager)
	{
		_director = director;
	}

	protected override PromptButtonData SubmitBlockersButton(MTGALocalizedString buttonText, string buttonSFX, ButtonStyle.StyleType buttonStyle)
	{
		Action action = _director.BlockButtonFunctionality(_request.AllBlockers);
		if (action == null)
		{
			return base.SubmitBlockersButton(buttonText, buttonSFX, buttonStyle);
		}
		return new PromptButtonData
		{
			ButtonText = buttonText,
			ButtonCallback = action,
			ButtonSFX = buttonSFX,
			ClearsInteractions = false,
			Style = buttonStyle
		};
	}
}
