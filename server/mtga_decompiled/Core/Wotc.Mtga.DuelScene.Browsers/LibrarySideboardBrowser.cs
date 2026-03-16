using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.Browsers;

public class LibrarySideboardBrowser : CardBrowserBase
{
	private readonly LibrarySideboardBrowserProvider provider;

	private BrowserHeader browserHeader;

	private Scrollbar scrollbar;

	public LibrarySideboardBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		base.AllowsHoverInteractions = true;
		this.provider = provider as LibrarySideboardBrowserProvider;
		cardHolder = cardHolderManager.GetCardHolder(GREPlayerNum.Invalid, CardHolderType.CardBrowserViewDismiss) as CardBrowserCardHolder;
		scrollableLayout = new CardLayout_ScrollableBrowser(GetCardHolderLayoutData(this.provider.GetCardHolderLayoutKey()));
	}

	public override Transform GetBrowserRoot()
	{
		return BrowserManager.ViewDismissRoot;
	}

	protected override void InitializeUIElements()
	{
		base.InitializeUIElements();
		Refresh();
	}

	protected override void SetupCards()
	{
		cardViews = provider.GetCardsToDisplay();
		CardLayout_ScrollableBrowser cardHolderLayoutData = GetCardHolderLayoutData(cardBrowserProvider.GetCardHolderLayoutKey(), (uint)cardViews.Count);
		scrollableLayout.CopyFrom(cardHolderLayoutData);
		MoveCardViewsToBrowser(cardViews);
	}

	public void Refresh()
	{
		ReleaseCards();
		SetupCards();
		browserHeader = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		browserHeader.SetHeaderText(provider.Header);
		browserHeader.SetSubheaderText(provider.SubHeader);
		scrollbar = GetBrowserElement("Scrollbar").GetComponent<Scrollbar>();
		scrollbar.onValueChanged.RemoveAllListeners();
		scrollbar.onValueChanged.AddListener(OnScroll);
		scrollbar.value = 1f;
		scrollbar.gameObject.SetActive(cardViews.Count > scrollableLayout.FrontCount);
		ScrollWheelInputForScrollbar component = scrollbar.GetComponent<ScrollWheelInputForScrollbar>();
		component.ElementCount = cardViews.Count;
		component.ElementsFeatured = scrollableLayout.FrontCount;
		scrollableLayout.ScrollPosition = scrollbar.value;
		UpdateButtons();
	}
}
