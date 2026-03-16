using System;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.Interactions;

public class DeclareAttackersWorkflow_NPE : DeclareAttackersWorkflow
{
	private readonly NPEDirector _director;

	public DeclareAttackersWorkflow_NPE(DeclareAttackerRequest declareAttackersReq, NPEDirector npeDirector, IObjectPool objPool, IPromptEngine promptEngine, IGameStateProvider gameStateProvider, ICardViewProvider cardViewProvider, IBattlefieldCardHolder battlefield, UIManager uiManager, GameManager gameManager)
		: base(declareAttackersReq, objPool, promptEngine, gameStateProvider, cardViewProvider, battlefield, uiManager, gameManager.InteractionSystem, gameManager.UIMessageHandler)
	{
		_director = npeDirector;
	}

	protected override PromptButtonData AttackWithAllButton(string buttonText, string buttonSFX)
	{
		Action action = _director.AttackWithAllButtonFunctionality();
		if (action == null)
		{
			return base.AttackWithAllButton(buttonText, buttonSFX);
		}
		PromptButtonData promptButtonData = base.AttackWithAllButton(buttonText, buttonSFX);
		return new PromptButtonData
		{
			ButtonText = buttonText,
			ButtonCallback = action,
			ButtonSFX = buttonSFX,
			ClearsInteractions = false,
			Style = promptButtonData.Style
		};
	}

	protected override PromptButtonData NoAttackersButton(string buttonText, string buttonSFX)
	{
		Action action = _director.NoAttackersButtonFunctionality(_attackers);
		if (action == null)
		{
			return base.NoAttackersButton(buttonText, buttonSFX);
		}
		PromptButtonData promptButtonData = base.NoAttackersButton(buttonText, buttonSFX);
		return new PromptButtonData
		{
			ButtonText = buttonText,
			ButtonCallback = action,
			ButtonSFX = buttonSFX,
			ClearsInteractions = false,
			Style = promptButtonData.Style
		};
	}

	protected override PromptButtonData SubmitAttacksButton(MTGALocalizedString buttonText, string buttonSFX, bool enabled = true)
	{
		Action action = _director.SubmitAttackersButtonFunctionality(_declaredAttackers);
		if (action == null)
		{
			return base.SubmitAttacksButton(buttonText, buttonSFX);
		}
		PromptButtonData promptButtonData = base.SubmitAttacksButton(buttonText, buttonSFX);
		return new PromptButtonData
		{
			ButtonText = buttonText,
			ButtonCallback = action,
			ButtonSFX = buttonSFX,
			Enabled = enabled,
			ClearsInteractions = false,
			Style = promptButtonData.Style
		};
	}
}
