using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene;

public class ExtraTurnRenderer : IExtraTurnRenderer
{
	private readonly IFakeCardViewManager _fakeCardViewManager;

	private readonly ICardHolderManager _cardHolderManager;

	private readonly IClientLocProvider _localizationManager;

	private const string EXTRA_TURN_CDC_KEY = "ExtraTurnCDC";

	private int _prvExtraTurnCount;

	public ExtraTurnRenderer(IFakeCardViewManager fakeCardViewManager, ICardHolderManager cardHolderManager, IClientLocProvider localizationManager)
	{
		_fakeCardViewManager = fakeCardViewManager ?? NullFakeCardViewManager.Default;
		_cardHolderManager = cardHolderManager ?? NullCardHolderManager.Default;
		_localizationManager = localizationManager;
	}

	public void Render(IReadOnlyList<ExtraTurn> extraTurns)
	{
		int count = extraTurns.Count;
		for (int i = 0; i < count; i++)
		{
			RenderFakeCard(ExtraTurnKey(i), extraTurns[i].ActivePlayer);
		}
		for (int j = count; j < _prvExtraTurnCount; j++)
		{
			_fakeCardViewManager.DeleteFakeCard(ExtraTurnKey(j));
		}
		_prvExtraTurnCount = count;
	}

	private string ExtraTurnKey(int i)
	{
		return string.Format("{0}-{1}", "ExtraTurnCDC", i);
	}

	private void RenderFakeCard(string key, GREPlayerNum activePlayer)
	{
		DuelScene_CDC orCreateFakeCard = GetOrCreateFakeCard(key);
		orCreateFakeCard.SetTooltipData(TooltipData());
		orCreateFakeCard.SetInputEnabled(enabled: false);
		MoveCard(orCreateFakeCard, activePlayer);
	}

	private DuelScene_CDC GetOrCreateFakeCard(string key)
	{
		if (!_fakeCardViewManager.TryGetFakeCard(key, out var fakeCdc))
		{
			return _fakeCardViewManager.CreateFakeCard(key, CreateIconModel());
		}
		return fakeCdc;
	}

	public static CardData CreateIconModel()
	{
		CardData cardData = CardDataExtensions.CreateBlank();
		cardData.Instance.GrpId = 10u;
		cardData.Instance.CatalogId = WellKnownCatalogId.WellKnownCatalogId_Icon;
		return cardData;
	}

	private TooltipData TooltipData()
	{
		return new TooltipData
		{
			Text = _localizationManager.GetLocalizedText("DuelScene/Prompt/ConsecutiveTurnTooltip")
		};
	}

	private void MoveCard(DuelScene_CDC miniCdc, GREPlayerNum activePlayer)
	{
		if (_cardHolderManager.TryGetCardHolder(activePlayer, CardHolderType.Command, out var cardHolder) && miniCdc.CurrentCardHolder != cardHolder && cardHolder is IGameEffectController gameEffectController)
		{
			miniCdc.CurrentCardHolder?.RemoveCard(miniCdc);
			gameEffectController.AddGameEffect(miniCdc, GameEffectType.TurnModification);
		}
	}
}
