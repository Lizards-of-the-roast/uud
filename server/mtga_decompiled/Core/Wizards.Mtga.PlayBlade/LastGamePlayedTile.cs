using System;
using AssetLookupTree;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN.Services.Models.Event;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Rank;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.PlayBlade;

public class LastGamePlayedTile : MonoBehaviour
{
	[Header("Serialized Things")]
	[SerializeField]
	private CustomButton _playButton;

	[SerializeField]
	private CustomButton _secondaryButton;

	[SerializeField]
	private Localize _secondaryButtonText;

	[SerializeField]
	private Localize _eventTitleText;

	[SerializeField]
	private Localize _bestOfText;

	[SerializeField]
	private RawImage _eventRawImageLogo;

	[SerializeField]
	private Image _backgroundImage;

	[Header("Scaffolds")]
	[SerializeField]
	private Transform _deckBoxParent;

	[SerializeField]
	private Transform _eventLogoParent;

	[SerializeField]
	private Transform _rankParent;

	[Header("Prefabs")]
	[SerializeField]
	private RankView _rankViewPrefab;

	private readonly AssetLoader.AssetTracker<Texture> _eventImageTextureTracker = new AssetLoader.AssetTracker<Texture>("LastPlayedTileTextureTracker");

	private AssetLoader.AssetTracker<Sprite> _backgroundImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("LastPlayedBackgroundTileSpriteTracker");

	private RecentlyPlayedInfo _model;

	private DeckViewBuilder _deckViewBuilder;

	private Action<DeckViewInfo, RecentlyPlayedInfo> _onDeckDoubleClicked;

	private Action<DeckViewInfo, RecentlyPlayedInfo> _onDeckClicked;

	private Action<RecentlyPlayedInfo> _onPlaySelected;

	private Action<RecentlyPlayedInfo> _onSecondarySelected;

	private DeckView _deckView;

	private void Awake()
	{
		_playButton.OnClick.AddListener(OnButtonClicked);
		if (_secondaryButton != null)
		{
			_secondaryButton.OnClick.AddListener(OnSecondaryButtonClicked);
		}
	}

	private void OnDestroy()
	{
		CleanUp();
		_playButton.OnClick.RemoveListener(OnButtonClicked);
		if (_secondaryButton != null)
		{
			_secondaryButton.OnClick.RemoveListener(OnSecondaryButtonClicked);
		}
		AssetLoaderUtils.CleanupImage(_backgroundImage, _backgroundImageSpriteTracker);
		AssetLoaderUtils.CleanupRawImage(_eventRawImageLogo, _eventImageTextureTracker);
		_model = null;
		_deckViewBuilder = null;
		_onDeckDoubleClicked = null;
		_onPlaySelected = null;
		_onSecondarySelected = null;
	}

	public void Initialize()
	{
		_deckViewBuilder = Pantry.Get<DeckViewBuilder>();
	}

	public void Inject(Action<DeckViewInfo, RecentlyPlayedInfo> onDeckDoubleClicked, Action<DeckViewInfo, RecentlyPlayedInfo> onDeckClicked, Action<RecentlyPlayedInfo> onPlaySelected, Action<RecentlyPlayedInfo> onSecondarySelected)
	{
		_onDeckDoubleClicked = onDeckDoubleClicked;
		_onDeckClicked = onDeckClicked;
		_onPlaySelected = onPlaySelected;
		_onSecondarySelected = onSecondarySelected;
	}

	public void SetModel(RecentlyPlayedInfo recentlyPlayedInfo, AssetLookupSystem assetLookupSystem)
	{
		if (_model != recentlyPlayedInfo)
		{
			CleanUp();
			_model = recentlyPlayedInfo;
			Hydrate(_model, assetLookupSystem);
			base.gameObject.name = "LastPlayedTile - (" + (_model?.EventInfo?.EventName ?? "null") + ")";
		}
	}

	private void Hydrate(RecentlyPlayedInfo recentlyPlayedInfo, AssetLookupSystem assetLookupSystem)
	{
		BladeEventInfo bladeEventInfo = recentlyPlayedInfo?.EventInfo;
		if (bladeEventInfo != null)
		{
			if (recentlyPlayedInfo.SelectedDeckInfo != null)
			{
				_deckView = _deckViewBuilder.CreateDeckView(recentlyPlayedInfo.SelectedDeckInfo, _deckBoxParent);
				_deckView.SetDeckOnClick(OnDeckViewDeckClicked);
				_deckView.SetDeckOnDoubleClick(OnDeckViewDeckDoubleClicked);
				_deckView.ClearValidationIcons();
			}
			if (!recentlyPlayedInfo.IsQueueEvent)
			{
				_eventRawImageLogo.texture = _eventImageTextureTracker.Acquire(bladeEventInfo.LogoImagePath);
			}
			bool active = !recentlyPlayedInfo.IsQueueEvent && !string.IsNullOrEmpty(bladeEventInfo.LogoImagePath);
			_eventLogoParent.gameObject.UpdateActive(active);
			AssetLoaderUtils.TrySetSprite(_backgroundImage, _backgroundImageSpriteTracker, bladeEventInfo.BladeImagePath);
			_eventTitleText.SetText(_model.EventInfo.LocTitle);
			string key = ((_model.EventInfo.WinCondition == MatchWinCondition.BestOf3) ? "PlayBlade/FindMatch/PlayBlade_FindMatch_BestOf_Three" : "PlayBlade/FindMatch/PlayBlade_FindMatch_BestOf_One");
			_bestOfText.SetText(key);
			if (_secondaryButtonText != null)
			{
				string key2 = (_model.IsQueueEvent ? "PlayBlade/FindMatch/PlayBlade_FindMatch_Tile_Queue_Type_Play_Button" : "PlayBlade/FindMatch/PlayBlade_FindMatch_Tile_Event_Type_Play_Button");
				_secondaryButtonText.SetText(key2);
			}
		}
	}

	private void OnDeckViewDeckClicked(DeckViewInfo deckViewInfo)
	{
		_onDeckClicked?.Invoke(deckViewInfo, _model);
	}

	private void OnDeckViewDeckDoubleClicked(DeckViewInfo deckViewInfo)
	{
		_onDeckDoubleClicked?.Invoke(deckViewInfo, _model);
	}

	private void OnButtonClicked()
	{
		_onPlaySelected?.Invoke(_model);
	}

	private void OnSecondaryButtonClicked()
	{
		_onSecondarySelected?.Invoke(_model);
	}

	private void CleanUp()
	{
		_deckViewBuilder.ReleaseDeckView(_deckView);
	}
}
