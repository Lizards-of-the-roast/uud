using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene.Interactions.Mulligan;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public class OpeningHandBrowser : CardBrowserBase, ISortedBrowser
{
	public OpeningHandBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		base.AllowsHoverInteractions = true;
	}

	protected override ICardLayout GetCardHolderLayout()
	{
		if (PlatformUtils.IsDesktop())
		{
			return new CardLayout_Fan
			{
				Radius = 40f,
				OverlapOffset = 0f,
				OverlapRotation = -7.5f,
				TiltRatio = 1f,
				VerticalOffset = 0f,
				MaxDeltaAngle = 4f,
				TotalDeltaAngle = 30f
			};
		}
		return base.GetCardHolderLayout();
	}

	public void SetOpeningHandText()
	{
		MtgGameState currentGameState = _gameManager.CurrentGameState;
		GREPlayerNum clientPlayerEnum = currentGameState.ActivePlayer.ClientPlayerEnum;
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(Languages.ActiveLocProvider.GetLocalizedText((clientPlayerEnum == GREPlayerNum.LocalPlayer) ? "DuelScene/StartingPlayer/Player_First_No_Arrow" : "DuelScene/StartingPlayer/Opponent_First_No_Arrow"));
		uint mulliganCount = currentGameState.Opponent.MulliganCount;
		uint count = (uint)currentGameState.OpponentHand.CardIds.Count;
		bool flag = currentGameState.Opponent.PendingMessageType != ClientMessageType.MulliganResp;
		uint freeMulliganCount = currentGameState.GameInfo.FreeMulliganCount;
		bool flag2 = freeMulliganCount > mulliganCount;
		string empty = string.Empty;
		switch (currentGameState.GameInfo.MulliganType)
		{
		case MulliganType.Paris:
		case MulliganType.Vancouver:
			empty = ((!flag2) ? Languages.ActiveLocProvider.GetLocalizedText("DuelScene/StartingPlayer/OpeningHandDetails", ("newHandSize", (count - 1).ToString())) : Languages.ActiveLocProvider.GetLocalizedText("DuelScene/StartingPlayer/FreeMulligan_Description_Opponent"));
			break;
		case MulliganType.London:
			if (flag)
			{
				empty = ((mulliganCount - freeMulliganCount != count) ? Languages.ActiveLocProvider.GetLocalizedText((count == 1) ? "DuelScene/StartingPlayer/OpponentKeepSingular" : "DuelScene/StartingPlayer/OpponentKeepPlural", ("count", count.ToString())) : Languages.ActiveLocProvider.GetLocalizedText("DuelScene/StartingPlayer/OpponentKeepPlural", ("count", 0.ToString())));
			}
			else if (flag2)
			{
				empty = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/StartingPlayer/FreeMulligan_Description_Opponent");
			}
			else
			{
				uint num = count - (mulliganCount - freeMulliganCount);
				empty = Languages.ActiveLocProvider.GetLocalizedText((num == 1) ? "DuelScene/StartingPlayer/OpponentChoosingSingular" : "DuelScene/StartingPlayer/OpponentChoosingPlural", ("count", num.ToString()));
			}
			break;
		default:
			throw new NotImplementedException(currentGameState.GameInfo.MulliganType.ToString());
		}
		component.SetSubheaderText(empty);
	}

	public override List<DuelScene_CDC> GetCardViews()
	{
		return cardHolder.CardViews;
	}

	protected override void SetupCards()
	{
	}

	public void Sort(List<DuelScene_CDC> toSort)
	{
		MulliganWorkflow.SortCards(toSort, _gameManager.CardDatabase.GreLocProvider);
	}
}
