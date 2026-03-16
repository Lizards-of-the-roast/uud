using System;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using Core.Code.AssetLookupTree.AssetLookup;
using Wizards.Mtga;

namespace Core.Code.Decks;

public class DeckBuilderLayoutState : IDisposable
{
	private DeckBuilderLayout? _layoutInUse;

	private readonly IAccountClient _accountClient;

	private readonly AssetLookupSystem _assetLookupSystem;

	private bool _isListViewSideboarding;

	public DeckBuilderLayout LayoutInUse
	{
		get
		{
			DeckBuilderLayout? layoutInUse = _layoutInUse;
			if (!layoutInUse.HasValue)
			{
				DeckBuilderLayout? deckBuilderLayout = (_layoutInUse = (MDNPlayerPrefs.GetUseColumnView(_accountClient?.AccountInformation?.PersonaID) ? DeckBuilderLayout.Column : DeckBuilderLayout.List));
				return deckBuilderLayout.GetValueOrDefault();
			}
			return layoutInUse.GetValueOrDefault();
		}
		set
		{
			if (value != _layoutInUse)
			{
				_layoutInUse = value;
				MDNPlayerPrefs.SetUseColumnView(_accountClient?.AccountInformation?.PersonaID, value == DeckBuilderLayout.Column);
				this.OnLayoutChanged?.Invoke(value);
			}
		}
	}

	public bool IsListViewSideboarding
	{
		get
		{
			return _isListViewSideboarding;
		}
		set
		{
			if (_isListViewSideboarding != value)
			{
				_isListViewSideboarding = value;
				this.IsListViewSideboardingUpdated?.Invoke(value);
			}
		}
	}

	public bool IsColumnViewExpanded { get; set; }

	public bool HideSideboardBeforeFormatSelected { get; set; }

	public bool LimitSubButtonsInHalfView { get; set; }

	public bool LargeCardsInPool { get; set; } = true;

	public bool CanUseLargeCardsInColumnView { get; set; }

	public event Action<DeckBuilderLayout> OnLayoutChanged;

	public event Action<bool> IsListViewSideboardingUpdated;

	public event Action<bool> SideboardVisibilityUpdated;

	public static DeckBuilderLayoutState Create()
	{
		return new DeckBuilderLayoutState(Pantry.Get<IAccountClient>(), Pantry.Get<AssetLookupManager>());
	}

	private DeckBuilderLayoutState(IAccountClient accountClient, AssetLookupManager alm)
	{
		_accountClient = accountClient;
		_assetLookupSystem = alm.AssetLookupSystem;
		_assetLookupSystem.Blackboard.AddFillerDelegate(FillerDelegate);
	}

	public void UpdateSideboardVisibility()
	{
		bool sideboardVisibility = GetSideboardVisibility();
		this.SideboardVisibilityUpdated?.Invoke(sideboardVisibility);
	}

	public bool GetSideboardVisibility()
	{
		return GetSideboardVisibility(Pantry.Get<DeckBuilderContextProvider>().Context, HideSideboardBeforeFormatSelected);
	}

	private static bool GetSideboardVisibility(DeckBuilderContext context, bool hideSideboardBeforeFormatSelected)
	{
		if (context.Format != null && context.Format.FormatIncludesCommandZone)
		{
			return false;
		}
		bool flag = !context.IsReadOnly || context.Deck.sideboard.Count > 0;
		bool flag2 = context.IsAmbiguousFormat && context.IsConstructed && context.IsFirstEdit;
		return (context.IsConstructed || context.IsDrafting) && !context.IsSideboarding && !(flag2 && hideSideboardBeforeFormatSelected) && flag;
	}

	public bool ShowOnlyOneSubButton()
	{
		if (LimitSubButtonsInHalfView && !IsColumnViewExpanded)
		{
			return LayoutInUse == DeckBuilderLayout.Column;
		}
		return false;
	}

	public void Dispose()
	{
		if (_assetLookupSystem.Blackboard != null)
		{
			_assetLookupSystem.Blackboard.RemoveFillerDelegate(FillerDelegate);
		}
	}

	private void FillerDelegate(IBlackboard blackboard)
	{
		blackboard.InHorizontalDeckBuilder = LayoutInUse == DeckBuilderLayout.Column;
	}
}
