using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public class NullFakeCardViewManager : IFakeCardViewManager, IFakeCardViewProvider, IFakeCardViewController
{
	public static readonly IFakeCardViewManager Default = new NullFakeCardViewManager();

	private static readonly IFakeCardViewProvider Provider = NullFakeCardViewProvider.Default;

	private static readonly IFakeCardViewController Controller = NullFakeCardViewController.Default;

	public IEnumerable<DuelScene_CDC> GetAllFakeCards()
	{
		return Provider.GetAllFakeCards();
	}

	public DuelScene_CDC GetFakeCard(string key)
	{
		return Provider.GetFakeCard(key);
	}

	public DuelScene_CDC CreateFakeCard(string key, ICardDataAdapter cardData, bool isVisible = false)
	{
		return Controller.CreateFakeCard(key, cardData, isVisible);
	}

	public bool DeleteFakeCard(string key)
	{
		return Controller.DeleteFakeCard(key);
	}
}
