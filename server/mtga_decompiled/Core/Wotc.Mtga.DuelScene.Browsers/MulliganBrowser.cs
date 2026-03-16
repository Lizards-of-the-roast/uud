using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions.Mulligan;

namespace Wotc.Mtga.DuelScene.Browsers;

public class MulliganBrowser : CardBrowserBase, ISortedBrowser
{
	private MulliganWorkflow _mulliganWorkflow;

	private bool _mulliganSelected;

	private BrowserHeader _browserHeaderView;

	private TextMeshProUGUI _mulliganConsequenceTextView;

	private EventTrigger _mulliganRulesButtonEventTriggerView;

	private EventTrigger.Entry _mulliganRulesButtonEventTriggerEntry;

	private ICardDatabaseAdapter _cardDatabase;

	public string HeaderText
	{
		set
		{
			_browserHeaderView.SetHeaderText(value);
		}
	}

	public string SubheaderText
	{
		set
		{
			_browserHeaderView.SetSubheaderText(value);
		}
	}

	public string MulliganConsequenceText
	{
		set
		{
			_mulliganConsequenceTextView.SetText(value);
		}
	}

	public event Action KeepPressed;

	public event Action MulliganPressed;

	public event Action RulesPressed;

	public MulliganBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		base.AllowsHoverInteractions = true;
		_mulliganWorkflow = _duelSceneBrowserProvider as MulliganWorkflow;
		_cardDatabase = gameManager.Context.Get<ICardDatabaseAdapter>();
	}

	public override void Init()
	{
		_mulliganRulesButtonEventTriggerEntry = new EventTrigger.Entry();
		_mulliganRulesButtonEventTriggerEntry.eventID = EventTriggerType.PointerClick;
		_mulliganRulesButtonEventTriggerEntry.callback.AddListener(OnMulliganRulesPressed);
		base.Init();
	}

	public override void UpdateButtons()
	{
		base.UpdateButtons();
		Dictionary<string, ButtonStateData> buttonStateData = _mulliganWorkflow.GetButtonStateData();
		ButtonStateData buttonStateData2 = buttonStateData["KeepButton"];
		GetBrowserElement(buttonStateData2.BrowserElementKey).GetComponent<StyledButton>().SetModel(new PromptButtonData
		{
			ButtonText = buttonStateData2.LocalizedString,
			Style = buttonStateData2.StyleType,
			Enabled = buttonStateData2.Enabled,
			ButtonCallback = OnKeepPressed
		});
		ButtonStateData buttonStateData3 = buttonStateData["MulliganButton"];
		GetBrowserElement(buttonStateData3.BrowserElementKey).GetComponent<StyledButton>().SetModel(new PromptButtonData
		{
			ButtonText = buttonStateData3.LocalizedString,
			Style = buttonStateData3.StyleType,
			Enabled = buttonStateData3.Enabled,
			ButtonCallback = OnMulliganPressed
		});
		_mulliganRulesButtonEventTriggerView = GetBrowserElement("MulliganRulesButton").GetComponent<EventTrigger>();
		_mulliganRulesButtonEventTriggerView.triggers.Remove(_mulliganRulesButtonEventTriggerEntry);
		_mulliganRulesButtonEventTriggerView.triggers.Add(_mulliganRulesButtonEventTriggerEntry);
	}

	protected override void InitScrollableLayout()
	{
		if (!PlatformUtils.IsDesktop())
		{
			base.InitScrollableLayout();
		}
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

	protected override void InitializeUIElements()
	{
		base.InitializeUIElements();
		_browserHeaderView = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		_mulliganConsequenceTextView = GetBrowserElement("UnderCardsText").GetComponent<TextMeshProUGUI>();
	}

	protected override void SetupCards()
	{
		cardViews = _mulliganWorkflow.GetCardsToDisplay();
		MoveCardViewsToBrowser(cardViews);
	}

	protected override void ReleaseCards()
	{
		if (!_mulliganSelected || cardViews.Count <= 1)
		{
			base.ReleaseCards();
		}
	}

	internal void OnKeepPressed()
	{
		_mulliganSelected = false;
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_mulligan_keep, GetBrowserRoot().gameObject);
		this.KeepPressed?.Invoke();
	}

	internal void OnMulliganPressed()
	{
		_mulliganSelected = true;
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_mulligan_discard, GetBrowserRoot().gameObject);
		this.MulliganPressed?.Invoke();
	}

	private void OnMulliganRulesPressed(BaseEventData arg0)
	{
		this.RulesPressed?.Invoke();
	}

	private void OnDestroy()
	{
		this.RulesPressed = null;
		this.MulliganPressed = null;
		this.KeepPressed = null;
		_mulliganWorkflow = null;
		_browserHeaderView = null;
		_mulliganConsequenceTextView = null;
		if ((bool)_mulliganRulesButtonEventTriggerView)
		{
			if (_mulliganRulesButtonEventTriggerEntry != null)
			{
				_mulliganRulesButtonEventTriggerView.triggers.Remove(_mulliganRulesButtonEventTriggerEntry);
				_mulliganRulesButtonEventTriggerEntry.callback = null;
				_mulliganRulesButtonEventTriggerEntry = null;
			}
			_mulliganRulesButtonEventTriggerView = null;
		}
	}

	public void Sort(List<DuelScene_CDC> toSort)
	{
		MulliganWorkflow.SortCards(toSort, _cardDatabase.GreLocProvider);
	}
}
