using System;
using System.Text;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Browsers;

public class ScryishBrowser : ScryBrowser
{
	public Action<DuelScene_CDC> DragReleased;

	private StringBuilder _sb = new StringBuilder();

	public ScryishBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager, ICardBuilder<DuelScene_CDC> cardBuilder)
		: base(browserManager, provider, gameManager, cardBuilder)
	{
	}

	protected override void SetupCards()
	{
		cardViews = _browserProvider.GetCardsToDisplay();
		MtgPlayer libraryOwner = null;
		if (cardViews.Count > 0)
		{
			libraryOwner = cardViews[0].Model.Owner;
		}
		int num = 1;
		int num2 = 1;
		if (_browserProvider is IScryishBrowserProvider scryishBrowserProvider)
		{
			num = scryishBrowserProvider.NthFromTop;
			num2 = scryishBrowserProvider.NthFromBot;
		}
		DuelScene_CDC duelScene_CDC = CreateLibraryPlaceholderCdc(num - 1, isMiddleLibrary: false, libraryOwner);
		if ((bool)duelScene_CDC)
		{
			cardViews.Add(duelScene_CDC);
		}
		duelScene_CDC = CreateLibraryPlaceholderCdc(3, isMiddleLibrary: true, libraryOwner);
		if ((bool)duelScene_CDC)
		{
			cardViews.Add(duelScene_CDC);
		}
		duelScene_CDC = CreateLibraryPlaceholderCdc(num2 - 1, isMiddleLibrary: false, libraryOwner);
		if ((bool)duelScene_CDC)
		{
			cardViews.Add(duelScene_CDC);
		}
		MoveCardViewsToBrowser(cardViews);
	}

	private DuelScene_CDC CreateLibraryPlaceholderCdc(int visualCount, bool isMiddleLibrary, MtgPlayer libraryOwner)
	{
		if (visualCount <= 0)
		{
			return null;
		}
		_sb.Clear();
		if (visualCount > 1)
		{
			_sb.Append("Library");
		}
		if (visualCount == 2)
		{
			_sb.Append(((_sb.Length > 0) ? "," : string.Empty) + "count2");
		}
		if (isMiddleLibrary)
		{
			_sb.Append(((_sb.Length > 0) ? "," : string.Empty) + "median");
		}
		return _cardBuilder.CreateCDC(CardBrowserBase.CreateLibraryPlaceHolderCardData(libraryOwner, (_sb.Length > 0) ? _sb.ToString() : null));
	}

	protected override void InitializeUIElements()
	{
		base.InitializeUIElements();
		if (_browserProvider is IScryishBrowserProvider { NthFromTop: 2 })
		{
			GetBrowserElement("OrderIndicator").GetComponent<OrderIndicator>().SetMiddleText(Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Browsers/OrderRequest_Generic_MiddleText"));
		}
	}

	public override void OnDragRelease(DuelScene_CDC draggedCard)
	{
		base.OnDragRelease(draggedCard);
		DragReleased?.Invoke(draggedCard);
	}
}
