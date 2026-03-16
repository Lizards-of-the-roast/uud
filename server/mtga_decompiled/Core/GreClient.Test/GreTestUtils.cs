using System.Linq;
using System.Text;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Test;

public static class GreTestUtils
{
	public static string PrintGameState(MtgGameState state, ICardDatabaseAdapter db)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(state.CurrentPhase.ToString()).Append(" - ").Append(state.CurrentStep.ToString())
			.AppendLine();
		stringBuilder.AppendFormat("{3}{4}Local Player({2}): {0} - {1}\n", state.LocalPlayer.LifeTotal, state.LocalPlayer.ManaPoolString, state.LocalPlayer.InstanceId, (state.ActivePlayer == state.LocalPlayer) ? "->" : "  ", (state.DecidingPlayer == state.LocalPlayer) ? "*" : " ");
		stringBuilder.AppendFormat("{3}{4}Opponent({2})    : {0} - {1}\n", state.Opponent.LifeTotal, state.Opponent.ManaPoolString, state.Opponent.InstanceId, (state.ActivePlayer == state.Opponent) ? "->" : "  ", (state.DecidingPlayer == state.Opponent) ? "*" : " ");
		stringBuilder.AppendLine("Local Battlefield:");
		foreach (MtgCardInstance localPlayerBattlefieldCard in state.LocalPlayerBattlefieldCards)
		{
			GreClient.CardData.CardData card = new GreClient.CardData.CardData(localPlayerBattlefieldCard, db.CardDataProvider.GetCardPrintingById(localPlayerBattlefieldCard.GrpId));
			stringBuilder.AppendLine("  " + FormatCardLine(card, db.GreLocProvider));
		}
		stringBuilder.AppendLine("Opponent Battlefield:");
		foreach (MtgCardInstance opponentBattlefieldCard in state.OpponentBattlefieldCards)
		{
			GreClient.CardData.CardData card2 = new GreClient.CardData.CardData(opponentBattlefieldCard, db.CardDataProvider.GetCardPrintingById(opponentBattlefieldCard.GrpId));
			stringBuilder.AppendLine("  " + FormatCardLine(card2, db.GreLocProvider));
		}
		stringBuilder.AppendLine("Stack:");
		foreach (MtgCardInstance visibleCard in state.Stack.VisibleCards)
		{
			GreClient.CardData.CardData card3 = new GreClient.CardData.CardData(visibleCard, db.CardDataProvider.GetCardPrintingById(visibleCard.GrpId));
			stringBuilder.AppendLine("  " + FormatCardLine(card3, db.GreLocProvider));
		}
		stringBuilder.AppendLine("Local Graveyard:");
		foreach (MtgCardInstance visibleCard2 in state.LocalGraveyard.VisibleCards)
		{
			GreClient.CardData.CardData card4 = new GreClient.CardData.CardData(visibleCard2, db.CardDataProvider.GetCardPrintingById(visibleCard2.GrpId));
			stringBuilder.AppendLine("  " + FormatCardLine(card4, db.GreLocProvider));
		}
		stringBuilder.AppendLine("Opponent Graveyard:");
		foreach (MtgCardInstance visibleCard3 in state.OpponentGraveyard.VisibleCards)
		{
			GreClient.CardData.CardData card5 = new GreClient.CardData.CardData(visibleCard3, db.CardDataProvider.GetCardPrintingById(visibleCard3.GrpId));
			stringBuilder.AppendLine("  " + FormatCardLine(card5, db.GreLocProvider));
		}
		stringBuilder.AppendLine("Limbo:");
		foreach (MtgCardInstance visibleCard4 in state.Limbo.VisibleCards)
		{
			GreClient.CardData.CardData card6 = new GreClient.CardData.CardData(visibleCard4, db.CardDataProvider.GetCardPrintingById(visibleCard4.GrpId));
			stringBuilder.AppendLine("  " + FormatCardLine(card6, db.GreLocProvider));
		}
		stringBuilder.AppendLine("Visible:");
		foreach (MtgCardInstance value in state.VisibleCards.Values)
		{
			GreClient.CardData.CardData cardData = new GreClient.CardData.CardData(value, db.CardDataProvider.GetCardPrintingById(value.GrpId));
			stringBuilder.AppendLine("  " + FormatCardLine(cardData, db.GreLocProvider) + " " + ((cardData.Zone != null) ? cardData.Zone.Type.ToString() : "None"));
		}
		return stringBuilder.ToString();
	}

	private static string FormatCardLine(GreClient.CardData.CardData card, IGreLocProvider greLocProvider)
	{
		string text = (card.IsTapped ? "[T]" : "   ");
		string text2 = "";
		string text3 = "";
		string text4 = "";
		if (card.CardTypes.Contains(CardType.Creature))
		{
			text2 = "[" + card.Power.Value + "/" + (card.Toughness.Value - card.Damage) + "]";
		}
		if (card.Counters.Count > 0)
		{
			text3 = "[" + card.Counters.Count + " counters]";
		}
		if (card.Targets.Count > 0)
		{
			text4 = "[" + card.Targets.Count + " targets]";
		}
		return $"{card.InstanceId,4}{card.GrpId,6} {text}{greLocProvider.GetLocalizedText(card.TitleId),-25} {text2}{text3} {text4}";
	}
}
