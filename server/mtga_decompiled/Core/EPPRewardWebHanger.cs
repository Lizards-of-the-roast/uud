using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.MainNavigation.RewardTrack;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN.Objectives;
using Wizards.Unification.Models.Player;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.RewardWeb;

public class EPPRewardWebHanger : MonoBehaviour
{
	[SerializeField]
	private bool _isBattlePass;

	[SerializeField]
	private bool _hideOnClick;

	[SerializeField]
	private List<Sprite> _colorOrbSprites;

	[SerializeField]
	private List<Image> _colorOrbImagesInHanger;

	[SerializeField]
	private Localize _titleLabel;

	[SerializeField]
	private Localize _descriptionLabel;

	[SerializeField]
	private Transform _anchorForChest;

	[SerializeField]
	private EPPRewardWebCardView _cardsView;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private List<GameObject> _3DColorOrbs;

	[SerializeField]
	private List<Sprite> _bgArts;

	[SerializeField]
	private Image _art;

	private AssetLoader.AssetTracker<Sprite> _artSpriteLoader = new AssetLoader.AssetTracker<Sprite>("RewardWebHangerArtSpriteTracker");

	public Action OnEventTriggerPointerEnter;

	public Action OnEventTriggerPointerExit;

	private int _currentId = -1;

	private RectTransform _transform;

	private bool _initialize;

	private CardDatabase _cardDatabase;

	private CardMaterialBuilder _cardMaterialBuilder;

	public void DisengageOrbSlot()
	{
		_currentId = -1;
		base.gameObject.SetActive(value: false);
	}

	public void Initialize(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, ICardRolloverZoom zoomHandler)
	{
		if (!_initialize)
		{
			_initialize = true;
			_transform = GetComponent<RectTransform>();
			_cardsView.Initialize(cardViewBuilder, zoomHandler, cardDatabase);
			_cardDatabase = cardDatabase;
			_cardMaterialBuilder = cardMaterialBuilder;
		}
	}

	private void Awake()
	{
		Initialize(WrapperController.Instance.CardDatabase, WrapperController.Instance.CardViewBuilder, WrapperController.Instance.CardMaterialBuilder, SceneLoader.GetSceneLoader().GetCardZoomView());
	}

	private void Update()
	{
		if (CustomInputModule.PointerWasPressedThisFrame() && _hideOnClick && !RectTransformUtility.RectangleContainsScreenPoint(_transform, CustomInputModule.GetPointerPosition(), CurrentCamera.Value))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void enableAndTransitionIn()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	private List<int> ColorFlagsForColor(CardColorFlags color)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < 5; i++)
		{
			if ((((int)color >> i) & 1) != 0)
			{
				list.Add(i);
			}
		}
		return list;
	}

	private void DisplayOrbsForColors(List<int> foundColors)
	{
		int i = 0;
		for (int num = Mathf.Min(foundColors.Count, _colorOrbImagesInHanger.Count); i < num; i++)
		{
			int index = foundColors[i];
			_colorOrbImagesInHanger[i].gameObject.UpdateActive(active: true);
			_colorOrbImagesInHanger[i].sprite = _colorOrbSprites[index];
		}
		for (; i < _colorOrbImagesInHanger.Count; i++)
		{
			_colorOrbImagesInHanger[i].gameObject.UpdateActive(active: false);
		}
	}

	private void displayBackgroundBasedOnFoundColors(List<int> foundColors)
	{
		int index = ((foundColors.Count == 1) ? foundColors[0] : (_bgArts.Count - 1));
		_art.sprite = _bgArts[index];
	}

	private void updateLocBasedOnChest(ClientChestDescription chest, MTGALocalizedString titleLoc, MTGALocalizedString descriptionLoc)
	{
		if (!string.IsNullOrEmpty(chest.headerLocKey))
		{
			titleLoc.Key = chest.headerLocKey;
		}
		if (!string.IsNullOrEmpty(chest.descriptionLocKey))
		{
			descriptionLoc.Key = chest.descriptionLocKey;
		}
		if (chest.locParams != null)
		{
			descriptionLoc.Parameters = (titleLoc.Parameters = chest.locParams.ToDictionary((KeyValuePair<string, int> p) => p.Key, (KeyValuePair<string, int> p) => p.Value.ToString()));
		}
	}

	private void DisplayTitleAndDescription(MTGALocalizedString titleLoc, MTGALocalizedString descriptionLoc)
	{
		_titleLabel.SetText(titleLoc);
		bool flag = !string.IsNullOrEmpty(descriptionLoc.Key);
		_descriptionLabel.gameObject.UpdateActive(flag);
		if (flag)
		{
			_descriptionLabel.SetText(descriptionLoc);
		}
	}

	private void UpdateLocWithFallbacks(MTGALocalizedString titleLoc, MTGALocalizedString descriptionLoc, string styleName)
	{
		if (string.IsNullOrEmpty(titleLoc.Key))
		{
			titleLoc.Key = "EPP/Nodes/Card_Title_Style";
		}
		if (string.IsNullOrEmpty(descriptionLoc.Key) && !string.IsNullOrEmpty(styleName))
		{
			descriptionLoc.Key = (_isBattlePass ? "EPP/Nodes/Generic_Styles_Desc_BP" : "EPP/Nodes/Generic_Styles_Desc");
		}
	}

	private void UpdateLocWithCardTitleParameters(MTGALocalizedString titleLoc, MTGALocalizedString descriptionLoc, uint chestCard)
	{
		CardPrintingData cardPrintingById = WrapperController.Instance.CardDatabase.CardDataProvider.GetCardPrintingById(chestCard);
		if (cardPrintingById != null)
		{
			string value = CardUtilities.FormatComplexTitle(WrapperController.Instance.CardDatabase.GreLocProvider.GetLocalizedText(cardPrintingById.TitleId));
			Dictionary<string, string> parameters = (titleLoc.Parameters = new Dictionary<string, string>
			{
				{ "card title", value },
				{ "cardName", value }
			});
			if (!string.IsNullOrEmpty(descriptionLoc.Key))
			{
				descriptionLoc.Parameters = parameters;
			}
		}
	}

	public void InspectOrbSlot(EPP_OrbSlotView slotView)
	{
		OrbSlot orbSlotModel = slotView.OrbSlotModel;
		int id = orbSlotModel.serverRewardNode.id;
		if (_currentId == id)
		{
			return;
		}
		_currentId = id;
		enableAndTransitionIn();
		List<int> foundColors = ColorFlagsForColor(slotView.Color);
		DisplayOrbsForColors(foundColors);
		displayBackgroundBasedOnFoundColors(foundColors);
		uint grpId = 0u;
		string styleName = null;
		MTGALocalizedString titleLoc = slotView.Title.Key;
		MTGALocalizedString descriptionLoc = slotView.Description.Key;
		ClientChestDescription chest = orbSlotModel.serverRewardNode.chest;
		if (chest != null)
		{
			RewardDisplayData.TryParseCard(chest.referenceId, out grpId, out styleName);
			updateLocBasedOnChest(chest, titleLoc, descriptionLoc);
		}
		if (grpId != 0)
		{
			UpdateLocWithFallbacks(titleLoc, descriptionLoc, styleName);
			UpdateLocWithCardTitleParameters(titleLoc, descriptionLoc, grpId);
		}
		DisplayTitleAndDescription(titleLoc, descriptionLoc);
		_anchorForChest.DestroyChildren();
		DisableRewardOrbs();
		List<uint> list = null;
		if (orbSlotModel.serverRewardNode.upgradePacket != null)
		{
			list = orbSlotModel.serverRewardNode.upgradePacket.cardsAdded;
		}
		else if (grpId != 0)
		{
			list = new List<uint> { grpId };
		}
		string text = chest?.prefab;
		if (list != null && (text == null || !text.Contains("Deck")))
		{
			if (list.Count > 1)
			{
				list = (from x in list
					select WrapperController.Instance.CardDatabase.CardDataProvider.GetCardPrintingById(x) into _
					orderby _.Rarity descending
					select _.GrpId).ToList();
			}
			_cardsView.SetCards(list, styleName);
		}
		else
		{
			_cardsView.DisableCards();
			displayChestReward(chest);
		}
	}

	private void displayChestReward(ClientChestDescription chest)
	{
		if (chest == null)
		{
			return;
		}
		NotificationPopupReward notificationPopupReward = null;
		RewardDisplayData rewardDisplayData = TempRewardTranslation.ChestDescriptionToDisplayData(chest, _cardDatabase.CardDataProvider, _cardMaterialBuilder);
		if (rewardDisplayData.OverridePopup3dObject != null)
		{
			notificationPopupReward = UnityEngine.Object.Instantiate(rewardDisplayData.OverridePopup3dObject, _anchorForChest);
			notificationPopupReward.SetBackgroundTexture(rewardDisplayData.PopupObjectBackgroundTexturePath);
			notificationPopupReward.SetForegroundTexture(rewardDisplayData.PopupObjectForegroundTexturePath);
		}
		else if (!string.IsNullOrEmpty(rewardDisplayData.Popup3dObjectPath))
		{
			notificationPopupReward = AssetLoader.Instantiate<NotificationPopupReward>(rewardDisplayData.Popup3dObjectPath, _anchorForChest);
			notificationPopupReward.SetBackgroundTexture(rewardDisplayData.PopupObjectBackgroundTexturePath);
			notificationPopupReward.SetForegroundTexture(rewardDisplayData.PopupObjectForegroundTexturePath);
		}
		if (notificationPopupReward != null)
		{
			RewardDataProvider component = notificationPopupReward.GetComponent<RewardDataProvider>();
			if (component != null)
			{
				component.Data = rewardDisplayData;
			}
		}
	}

	private void DisableRewardOrbs()
	{
		foreach (GameObject _3DColorOrb in _3DColorOrbs)
		{
			_3DColorOrb.SetActive(value: false);
		}
	}

	public EPPRewardWebHanger SetRewardData(RewardDisplayData reward)
	{
		DisplayTitleAndDescription(reward.MainText, reward.DescriptionText);
		return this;
	}

	public EPPRewardWebHanger SetGraphNode(AssetLookupSystem assetLookupSystem, Client_ColorChallengeMatchNode node)
	{
		foreach (Image item in _colorOrbImagesInHanger)
		{
			item.gameObject.UpdateActive(active: false);
		}
		_setCards(node.DeckUpgradeData);
		_setRewards(node.Reward);
		AssetLoaderUtils.TrySetSprite(_art, _artSpriteLoader, ClientEventDefinitionList.GetPoptartBackgroundPath(assetLookupSystem, node.Id));
		return this;
	}

	private void _setCards(Client_DeckUpgrade upgradePacket)
	{
		if (upgradePacket == null || upgradePacket.CardsAdded.Count == 0)
		{
			_cardsView.DisableCards();
		}
		else
		{
			_cardsView.SetCards(upgradePacket.CardsAdded);
		}
	}

	private void _setRewards(RewardDisplayData chest)
	{
		if (chest != null)
		{
			RewardDisplayData.TryParseCard(chest.ReferenceID, out var grpId, out var styleName);
			if (grpId != 0)
			{
				_cardsView.SetCards(new List<uint> { grpId }, styleName);
			}
		}
	}

	public void EventTrigger_PointerEnter()
	{
		OnEventTriggerPointerEnter?.Invoke();
	}

	public void EventTrigger_PointerExit()
	{
		OnEventTriggerPointerExit?.Invoke();
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_art, _artSpriteLoader);
	}

	public void SetZoomHandler(ICardRolloverZoom zoomHandler)
	{
		_cardsView.SetZoomHandler(zoomHandler);
	}
}
