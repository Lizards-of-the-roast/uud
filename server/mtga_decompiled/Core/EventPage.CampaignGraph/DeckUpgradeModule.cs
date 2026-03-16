using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.Input;
using Core.Meta.MainNavigation.Store;
using GreClient.CardData;
using MTGA.KeyboardManager;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;

namespace EventPage.CampaignGraph;

public class DeckUpgradeModule : OverlayModule, IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	[SerializeField]
	private Animator _animator;

	[Header("Card and Deckbox Parameters")]
	[SerializeField]
	private Transform[] _cardAddedAnchors;

	[SerializeField]
	private Transform[] _cardRemovedAnchors;

	[SerializeField]
	private MeshRenderer[] _deckboxComponents;

	[SerializeField]
	private MetaCardHolder _cardHolder;

	[SerializeField]
	private CardRolloverZoomBase _cardZoomView;

	[Header("UI Parameters")]
	[SerializeField]
	private CustomButton _upgradeDeckButton;

	private int _upgradeTrigger = Animator.StringToHash("Upgrade");

	private bool _moduleInitialized;

	private List<CDCMetaCardView> _instantiatedCards = new List<CDCMetaCardView>();

	private MeshRendererReferenceLoader[] _meshRendererReferenceLoaders;

	private IColorChallengeStrategy _colorChallengeStrategy;

	private ICardRolloverZoom _cardRolloverZoom;

	private bool _showing;

	public PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper_PopUps;

	private void Awake()
	{
		_upgradeDeckButton.OnClick.AddListener(OnUpgradeDeckButtonClicked);
		tryInitMeshRenderers();
	}

	private void OnDestroy()
	{
		if (_meshRendererReferenceLoaders != null)
		{
			MeshRendererReferenceLoader[] meshRendererReferenceLoaders = _meshRendererReferenceLoaders;
			for (int i = 0; i < meshRendererReferenceLoaders.Length; i++)
			{
				meshRendererReferenceLoaders[i]?.Cleanup();
			}
			_meshRendererReferenceLoaders = null;
		}
	}

	public override void Init(EventTemplate parentTemplate, KeyboardManager keyboardManager, IActionSystem actionSystem, AssetLookupSystem assetLookupSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_colorChallengeStrategy = Pantry.Get<IColorChallengeStrategy>();
		base.Init(parentTemplate, keyboardManager, actionSystem, assetLookupSystem, cardDatabase, cardViewBuilder);
	}

	private void tryInitMeshRenderers()
	{
		if (_meshRendererReferenceLoaders == null)
		{
			_meshRendererReferenceLoaders = new MeshRendererReferenceLoader[_deckboxComponents.Length];
			for (int i = 0; i < _deckboxComponents.Length; i++)
			{
				_meshRendererReferenceLoaders[i] = new MeshRendererReferenceLoader(_deckboxComponents[i]);
			}
		}
	}

	public override void SetZoomHandler(ICardRolloverZoom cardRolloverZoom)
	{
		_cardRolloverZoom = cardRolloverZoom;
		_cardHolder.CanDragCards = (MetaCardView _) => false;
	}

	public override void Show()
	{
		base.gameObject.SetActive(value: false);
		if (!_colorChallengeStrategy.TryGetDeckUpgradePacket(out var deckUpgrade))
		{
			_moduleInitialized = false;
			return;
		}
		CreateCards(deckUpgrade);
		if (_meshRendererReferenceLoaders == null)
		{
			tryInitMeshRenderers();
		}
		Client_DeckSummary client_DeckSummary = base.EventContext.PlayerEvent?.CourseData?.CourseDeck?.Summary;
		uint id = client_DeckSummary?.DeckTileId ?? deckUpgrade.CardsAdded[0];
		_moduleInitialized = true;
		string artPath = ((client_DeckSummary.DeckArtId == 0) ? (_cardDatabase.CardDataProvider.GetCardPrintingById(id)?.ImageAssetPath ?? string.Empty) : _cardDatabase.DatabaseUtilities.GetPrintingsByArtId(client_DeckSummary.DeckArtId)?.FirstOrDefault()?.ImageAssetPath);
		DeckBoxUtil.SetDeckBoxTexture(artPath, _cardMaterialBuilder.TextureLoader, _cardMaterialBuilder.CropDatabase, _meshRendererReferenceLoaders);
		_keyboardManager?.Subscribe(this);
		if (!_showing)
		{
			_actionSystem.PushFocus(this);
		}
		_showing = true;
	}

	public override void UpdateModule()
	{
	}

	public override void LateUpdateModule()
	{
		ContentControllerRewards rewardsContentController = SceneLoader.GetSceneLoader().GetRewardsContentController();
		if (rewardsContentController.Visible)
		{
			rewardsContentController.RegisterRewardClosedCallback(UpdateGameObjectActiveState);
		}
		else
		{
			UpdateGameObjectActiveState();
		}
	}

	private void UpdateGameObjectActiveState()
	{
		base.gameObject.SetActive(_moduleInitialized);
		SceneLoader.GetSceneLoader().GetRewardsContentController().UnregisterRewardsClosedCallback(UpdateGameObjectActiveState);
	}

	public override void Hide()
	{
		foreach (CDCMetaCardView instantiatedCard in _instantiatedCards)
		{
			instantiatedCard.Cleanup();
			Object.Destroy(instantiatedCard.gameObject);
		}
		_instantiatedCards.Clear();
		_keyboardManager?.Unsubscribe(this);
		if (_showing)
		{
			_actionSystem.PopFocus(this);
		}
		base.gameObject.SetActive(value: false);
		_showing = false;
	}

	private void CreateCards(Client_DeckUpgrade upgradePacket)
	{
		CardDatabase cardDatabase = WrapperController.Instance.CardDatabase;
		CardViewBuilder cardViewBuilder = WrapperController.Instance.CardViewBuilder;
		_cardHolder.RolloverZoomView = _cardRolloverZoom ?? _cardZoomView;
		for (int i = 0; i < _cardAddedAnchors.Length; i++)
		{
			if (i < upgradePacket.CardsAdded.Count)
			{
				_cardAddedAnchors[i].gameObject.UpdateActive(active: true);
				CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(upgradePacket.CardsAdded[i]);
				CardData data = new CardData(cardPrintingById.CreateInstance(), cardPrintingById);
				CDCMetaCardView cDCMetaCardView = cardViewBuilder.CreateCDCMetaCardView(data, _cardAddedAnchors[i]);
				cDCMetaCardView.Holder = _cardHolder;
				_instantiatedCards.Add(cDCMetaCardView);
				foreach (Transform item in _cardAddedAnchors[i])
				{
					if ((bool)item.GetComponent<CDCPart>())
					{
						item.gameObject.UpdateActive(active: false);
					}
				}
			}
			else
			{
				_cardAddedAnchors[i].gameObject.UpdateActive(active: false);
			}
		}
		for (int j = 0; j < _cardRemovedAnchors.Length; j++)
		{
			if (j < upgradePacket.CardsToRemove.Count)
			{
				_cardRemovedAnchors[j].gameObject.UpdateActive(active: true);
				CardPrintingData cardPrintingById2 = cardDatabase.CardDataProvider.GetCardPrintingById(upgradePacket.CardsToRemove[j]);
				CardData data2 = new CardData(cardPrintingById2.CreateInstance(), cardPrintingById2);
				CDCMetaCardView cDCMetaCardView2 = cardViewBuilder.CreateCDCMetaCardView(data2, _cardRemovedAnchors[j]);
				_cardHolder.RolloverZoomView = _cardRolloverZoom ?? _cardZoomView;
				cDCMetaCardView2.Holder = _cardHolder;
				_instantiatedCards.Add(cDCMetaCardView2);
				foreach (Transform item2 in _cardRemovedAnchors[j])
				{
					if ((bool)item2.GetComponent<CDCPart>())
					{
						item2.gameObject.UpdateActive(active: false);
					}
				}
			}
			else
			{
				_cardRemovedAnchors[j].gameObject.UpdateActive(active: false);
			}
		}
	}

	private void OnUpgradeDeckButtonClicked()
	{
		if (_animator != null)
		{
			_animator.SetTrigger(_upgradeTrigger);
		}
	}

	private void Animator_Hide()
	{
		Hide();
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			_upgradeDeckButton.Click();
			return true;
		}
		return false;
	}

	public void OnBack(ActionContext context)
	{
		_upgradeDeckButton.Click();
	}
}
