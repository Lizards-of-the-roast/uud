using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Browsers;

public class ViewDismissBrowser : CardBrowserBase
{
	private readonly IViewDismissBrowserProvider _browserProvider;

	public readonly GREPlayerNum PlayerNum;

	private BrowserHeader browserHeader;

	private BrowserSubtitle browserSubtitle;

	private Scrollbar scrollbar;

	public ViewDismissBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		base.AllowsHoverInteractions = true;
		_browserProvider = _duelSceneBrowserProvider as IViewDismissBrowserProvider;
		PlayerNum = _browserProvider.PlayerNum;
		scrollableLayout = new CardLayout_ScrollableBrowser(GetCardHolderLayoutData(_browserProvider.GetCardHolderLayoutKey()));
		cardHolder = cardHolderManager.GetCardHolder(GREPlayerNum.Invalid, CardHolderType.CardBrowserViewDismiss) as CardBrowserCardHolder;
		cardHolder.LockCardDetails = _browserProvider is ICardLock cardLock && cardLock.LockCardDetails;
	}

	protected override void InitCardHolder()
	{
		cardHolder.ApplyTargetOffset = _browserProvider.ApplyTargetOffset;
		cardHolder.ApplyControllerOffset = _browserProvider.ApplyControllerOffset;
	}

	public override Transform GetBrowserRoot()
	{
		return BrowserManager.ViewDismissRoot;
	}

	protected override void InitializeUIElements()
	{
		browserHeader = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		browserHeader.SetHeaderText(_browserProvider.Header);
		browserHeader.SetSubheaderText(_browserProvider.SubHeader);
		GameObject browserElement = GetBrowserElement("Subtitle");
		if (browserElement != null)
		{
			browserSubtitle = browserElement.GetComponent<BrowserSubtitle>();
		}
		SetUpCardCountSubtitle();
		scrollbar = GetBrowserElement("Scrollbar").GetComponent<Scrollbar>();
		scrollbar.onValueChanged.RemoveAllListeners();
		scrollbar.onValueChanged.AddListener(OnScroll);
		scrollbar.value = scrollableLayout.ScrollPosition;
		scrollbar.gameObject.SetActive(cardViews.Count > scrollableLayout.FrontCount);
		ScrollWheelInputForScrollbar component = GetBrowserElement("Scrollbar").GetComponent<ScrollWheelInputForScrollbar>();
		component.ElementCount = cardViews.Count;
		component.ElementsFeatured = scrollableLayout.FrontCount;
		base.InitializeUIElements();
	}

	protected override void SetupCards()
	{
		cardViews = _browserProvider.GetCardsToDisplay();
		MoveCardViewsToBrowser(cardViews);
	}

	public virtual void Refresh()
	{
		ReleaseCards();
		SetupCards();
		browserHeader = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		browserHeader.SetHeaderText(_browserProvider.Header);
		browserHeader.SetSubheaderText(_browserProvider.SubHeader);
		SetUpCardCountSubtitle();
		scrollbar.gameObject.SetActive(cardViews.Count > scrollableLayout.FrontCount);
		UpdateButtons();
	}

	private void SetUpCardCountSubtitle()
	{
		if (browserSubtitle != null)
		{
			string subTitleText = ((cardViews.Count <= 1) ? Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Browsers/Card_Text", ("numCards", cardViews.Count.ToString())) : Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Browsers/Cards_Text", ("numCards", cardViews.Count.ToString())));
			browserSubtitle.SetSubTitleText(subTitleText);
		}
	}

	protected override void CardHolder_OnCardRemoved(DuelScene_CDC cardView)
	{
		if (cardHolder.LockCardDetails && cardView.RevealOverride != null)
		{
			cardView.RemoveRevealOverride();
		}
	}

	protected override void ReleaseCards()
	{
		base.ReleaseCards();
		if (cardHolderManager.TryGetCardHolder(GREPlayerNum.Opponent, CardHolderType.Hand, out var cardHolder) && cardHolder is OpponentHandCardHolder_Handheld opponentHandCardHolder_Handheld)
		{
			opponentHandCardHolder_Handheld.StartCoroutine(opponentHandCardHolder_Handheld.ClearLockCardDetails());
		}
		if (base.cardHolder.LockCardDetails)
		{
			base.cardHolder.StartCoroutine(base.cardHolder.ClearLockCardDetails());
		}
	}
}
