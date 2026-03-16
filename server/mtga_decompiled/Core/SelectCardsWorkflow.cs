using System.Collections.Generic;
using GreClient.Rules;
using WorkflowVisuals;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public abstract class SelectCardsWorkflow<T> : BrowserWorkflowBase<T>, ISelectCardsBrowserProvider, IBasicBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider where T : BaseUserRequest
{
	private class SelectCardsHighlightsGenerator : IHighlightsGenerator
	{
		private readonly IReadOnlyCollection<DuelScene_CDC> _nonSelectableCards;

		private readonly IReadOnlyCollection<DuelScene_CDC> _selectableCards;

		private readonly IReadOnlyCollection<DuelScene_CDC> _selectedCards;

		public SelectCardsHighlightsGenerator(IReadOnlyCollection<DuelScene_CDC> nonSelectableCards, IReadOnlyCollection<DuelScene_CDC> selectableCards, IReadOnlyCollection<DuelScene_CDC> selectedCards)
		{
			_nonSelectableCards = nonSelectableCards;
			_selectableCards = selectableCards;
			_selectedCards = selectedCards;
		}

		public Highlights GetHighlights()
		{
			Highlights highlights = new Highlights();
			foreach (DuelScene_CDC nonSelectableCard in _nonSelectableCards)
			{
				highlights.IdToHighlightType_Workflow[nonSelectableCard.InstanceId] = HighlightType.None;
			}
			foreach (DuelScene_CDC selectableCard in _selectableCards)
			{
				highlights.IdToHighlightType_Workflow[selectableCard.InstanceId] = HighlightType.Hot;
			}
			foreach (DuelScene_CDC selectedCard in _selectedCards)
			{
				highlights.IdToHighlightType_Workflow[selectedCard.InstanceId] = HighlightType.Selected;
			}
			return highlights;
		}
	}

	public readonly struct Selection
	{
		public readonly MtgCardInstance card;

		public readonly uint selectCount;

		public readonly uint minSelect;

		public readonly uint maxSelect;

		public Selection(MtgCardInstance reqCard, uint minSel, uint maxSel, uint current)
		{
			card = reqCard;
			minSelect = minSel;
			maxSelect = maxSel;
			selectCount = current;
		}

		public bool CanAutoSubmit()
		{
			bool num = selectCount >= minSelect && selectCount <= maxSelect;
			bool flag = maxSelect != 0 && selectCount == maxSelect;
			if (num && flag)
			{
				return card.Controller.IsLocalPlayer;
			}
			return false;
		}
	}

	public class ZoneIdComparerLocalFirst : IComparer<uint>
	{
		private MtgGameState _gamestate;

		public ZoneIdComparerLocalFirst(MtgGameState gameState)
		{
			_gamestate = gameState;
		}

		public int Compare(uint x, uint y)
		{
			MtgZone zoneById = _gamestate.GetZoneById(x);
			MtgZone zoneById2 = _gamestate.GetZoneById(y);
			if (zoneById.OwnerNum == GREPlayerNum.Invalid || zoneById2.OwnerNum == GREPlayerNum.Invalid || zoneById.OwnerNum == zoneById2.OwnerNum || zoneById.Type != zoneById2.Type)
			{
				return x.CompareTo(y);
			}
			return zoneById.OwnerNum.CompareTo(zoneById2.OwnerNum);
		}
	}

	protected readonly List<DuelScene_CDC> selectable = new List<DuelScene_CDC>();

	protected readonly List<DuelScene_CDC> currentSelections = new List<DuelScene_CDC>();

	protected readonly List<DuelScene_CDC> nonSelectable = new List<DuelScene_CDC>();

	protected readonly Dictionary<DuelScene_CDC, HighlightType> browserHighlights = new Dictionary<DuelScene_CDC, HighlightType>();

	public bool AllowKeyboardSelection { get; private set; }

	protected SelectCardsWorkflow(T request)
		: base(request)
	{
		_highlightsGenerator = new SelectCardsHighlightsGenerator(nonSelectable, selectable, currentSelections);
	}

	public IEnumerable<DuelScene_CDC> GetSelectableCdcs()
	{
		return selectable;
	}

	protected virtual bool IsHotSelectable(DuelScene_CDC cdc)
	{
		return true;
	}

	public IEnumerable<DuelScene_CDC> GetNonSelectableCdcs()
	{
		return nonSelectable;
	}

	protected override void CardBrowser_OnCardViewSelected(DuelScene_CDC cardView)
	{
		if (!selectable.Contains(cardView))
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid.EventName, cardView.gameObject);
			return;
		}
		if (currentSelections.Contains(cardView))
		{
			currentSelections.Remove(cardView);
			base.Arrows.ClearLines();
		}
		else
		{
			currentSelections.Add(cardView);
		}
		UpdateHighlightsAndDimming();
	}

	protected virtual Dictionary<string, ButtonStateData> GenerateDefaultButtonStates(int currentSelectionCount, int minSelections, int maxSelections, AllowCancel cancelType)
	{
		bool flag = cancelType != AllowCancel.No && cancelType != AllowCancel.None;
		_buttonStateData = new Dictionary<string, ButtonStateData>();
		ButtonStateData buttonStateData = new ButtonStateData();
		buttonStateData.LocalizedString = "DuelScene/KeywordSelection/KeywordSelection_Done";
		buttonStateData.Enabled = currentSelectionCount >= minSelections && currentSelectionCount <= maxSelections;
		buttonStateData.BrowserElementKey = (flag ? "SingleButton" : "2Button_Left");
		if (selectable.Count == 1 && minSelections == 0 && maxSelections == 1 && cancelType == AllowCancel.No)
		{
			AllowKeyboardSelection = true;
			buttonStateData.StyleType = ButtonStyle.StyleType.Secondary;
		}
		else
		{
			buttonStateData.StyleType = ButtonStyle.StyleType.Main;
		}
		_buttonStateData.Add("DoneButton", buttonStateData);
		if (flag)
		{
			ButtonStateData buttonStateData2 = new ButtonStateData();
			buttonStateData2.LocalizedString = ((cancelType == AllowCancel.Continue) ? "DuelScene/ClientPrompt/ClientPrompt_Button_Decline" : "DuelScene/ClientPrompt/ClientPrompt_Button_Cancel");
			buttonStateData2.BrowserElementKey = "2Button_Right";
			buttonStateData2.Enabled = true;
			_buttonStateData.Add("CancelButton", buttonStateData2);
		}
		return _buttonStateData;
	}

	protected virtual Dictionary<string, ButtonStateData> GenerateMultiZoneButtonStates(int minSelections, int maxSelections, AllowCancel cancelType, List<uint> zoneIds, List<uint> additionalZoneIds, uint currentZoneId, MtgGameState gameState, IClientLocProvider clientLocProvider)
	{
		Dictionary<string, ButtonStateData> dictionary = CreateNonZoneButtons(currentSelections.Count, minSelections, maxSelections, cancelType);
		AddZoneButtonsToDictionary(dictionary, zoneIds, additionalZoneIds, currentZoneId, gameState, clientLocProvider);
		return dictionary;
	}

	protected static Dictionary<string, ButtonStateData> GenerateMultiZoneButtonStates(int currentSelectionCount, int minSelections, int maxSelections, AllowCancel cancelType, List<uint> zoneIds, uint currentZoneId, MtgGameState gameState, IClientLocProvider clientLocProvider)
	{
		Dictionary<string, ButtonStateData> dictionary = CreateNonZoneButtons(currentSelectionCount, minSelections, maxSelections, cancelType);
		AddZoneButtonsToDictionary(dictionary, zoneIds, null, currentZoneId, gameState, clientLocProvider);
		return dictionary;
	}

	protected static Dictionary<string, ButtonStateData> CreateNonZoneButtons(int currentSelectionCount, int minSelections, int maxSelections, AllowCancel cancelType)
	{
		bool flag = cancelType != AllowCancel.No && cancelType != AllowCancel.None;
		Dictionary<string, ButtonStateData> dictionary = new Dictionary<string, ButtonStateData>();
		ButtonStateData buttonStateData = new ButtonStateData();
		buttonStateData.BrowserElementKey = (flag ? "SingleButton" : "2Button_Left");
		buttonStateData.StyleType = ButtonStyle.StyleType.Main;
		if (currentSelectionCount >= minSelections && currentSelectionCount <= maxSelections && currentSelectionCount != 0)
		{
			buttonStateData.Enabled = true;
			buttonStateData.LocalizedString = "DuelScene/KeywordSelection/KeywordSelection_Done";
		}
		else if (currentSelectionCount == 0 && minSelections == 0)
		{
			buttonStateData.Enabled = true;
			buttonStateData.LocalizedString = "DuelScene/ClientPrompt/ClientPrompt_Button_FailToFind";
			buttonStateData.StyleType = ButtonStyle.StyleType.Secondary;
		}
		else
		{
			buttonStateData.Enabled = false;
			buttonStateData.LocalizedString = "DuelScene/KeywordSelection/KeywordSelection_Done";
		}
		dictionary.Add("DoneButton", buttonStateData);
		if (flag)
		{
			ButtonStateData buttonStateData2 = new ButtonStateData();
			buttonStateData2.StyleType = ButtonStyle.StyleType.Secondary;
			buttonStateData2.LocalizedString = ((cancelType == AllowCancel.Continue) ? "DuelScene/ClientPrompt/ClientPrompt_Button_Decline" : "DuelScene/ClientPrompt/ClientPrompt_Button_Cancel");
			buttonStateData2.BrowserElementKey = "2Button_Right";
			buttonStateData2.Enabled = true;
			dictionary.Add("CancelButton", buttonStateData2);
		}
		return dictionary;
	}

	protected static void AddZoneButtonsToDictionary(Dictionary<string, ButtonStateData> buttonDict, List<uint> zoneIds, List<uint> additionalZoneIds, uint currentZoneId, MtgGameState gameState, IClientLocProvider locProvider)
	{
		List<uint> list = new List<uint>();
		list.AddRange(zoneIds);
		if (additionalZoneIds != null)
		{
			list.AddRange(additionalZoneIds);
		}
		list.Sort(new ZoneIdComparerLocalFirst(gameState));
		for (int i = 0; i < list.Count; i++)
		{
			MtgZone zoneById = gameState.GetZoneById(list[i]);
			ButtonStateData buttonStateData = new ButtonStateData();
			buttonStateData.StyleType = ButtonStyle.StyleType.MultiZone;
			buttonStateData.BrowserElementKey = $"ZoneButton{i.ToString()}";
			buttonStateData.Enabled = zoneById.Id != currentZoneId;
			string localizedZoneKey = Utils.GetLocalizedZoneKey(zoneById.Type, zoneById.Owner);
			if (additionalZoneIds != null && additionalZoneIds.Contains(zoneById.Id))
			{
				string localizedText = locProvider.GetLocalizedText("DuelScene/Browsers/ZoneSearch", ("zone", locProvider.GetLocalizedText(localizedZoneKey)));
				buttonStateData.LocalizedString = new UnlocalizedMTGAString(localizedText);
			}
			else
			{
				buttonStateData.LocalizedString = localizedZoneKey;
			}
			buttonDict.Add($"ZoneButton{zoneById.Id}", buttonStateData);
		}
	}

	public virtual Dictionary<DuelScene_CDC, HighlightType> GetBrowserHighlights()
	{
		browserHighlights.Clear();
		foreach (DuelScene_CDC item in selectable)
		{
			browserHighlights[item] = ((!IsHotSelectable(item)) ? HighlightType.Cold : HighlightType.Hot);
		}
		foreach (DuelScene_CDC item2 in nonSelectable)
		{
			browserHighlights[item2] = HighlightType.None;
		}
		foreach (DuelScene_CDC currentSelection in currentSelections)
		{
			browserHighlights[currentSelection] = HighlightType.Selected;
		}
		return browserHighlights;
	}

	protected override void SetDimming()
	{
		base.Dimming.IdToIsDimmed = new Dictionary<uint, bool>(_cardsToDisplay.Count);
		foreach (DuelScene_CDC item in nonSelectable)
		{
			base.Dimming.IdToIsDimmed[item.InstanceId] = true;
		}
		foreach (DuelScene_CDC item2 in selectable)
		{
			base.Dimming.IdToIsDimmed[item2.InstanceId] = false;
		}
		foreach (DuelScene_CDC currentSelection in currentSelections)
		{
			base.Dimming.IdToIsDimmed[currentSelection.InstanceId] = false;
		}
		OnUpdateDimming(base.Dimming);
	}

	protected Selection CurrentSelection(uint sourceCardId, uint min, uint max, uint current, MtgGameState gameState)
	{
		return new Selection(SourceInstance(sourceCardId, gameState), min, max, current);
	}

	private MtgCardInstance SourceInstance(uint sourceId, MtgGameState gameState)
	{
		if (!gameState.TryGetCard(sourceId, out var card))
		{
			return null;
		}
		return card;
	}
}
