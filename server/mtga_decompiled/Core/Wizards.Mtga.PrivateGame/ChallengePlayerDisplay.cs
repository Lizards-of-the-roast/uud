using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Cosmetic;
using Core.Code.AssetLookupTree.AssetLookup;
using GreClient.CardData;
using MTGA.Social;
using Pooling;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.GeneralUtilities.AdvancedButton;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Inventory;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.PrivateGame;

public class ChallengePlayerDisplay : MonoBehaviour
{
	private static readonly int InWrapper = Animator.StringToHash("InWrapper");

	[SerializeField]
	private GameObject _avatar;

	[SerializeField]
	private GameObject _playerInfo;

	[SerializeField]
	private TMP_Text _playerName;

	[SerializeField]
	private Localize _playerTitle;

	[SerializeField]
	private Localize _playerStatus;

	[SerializeField]
	private GameObject _partyLeaderCrown;

	[SerializeField]
	private Image _avatarImage;

	[SerializeField]
	private GameObject _companionAnchor;

	[SerializeField]
	private GameObject _sleeveAnchor;

	[SerializeField]
	private CardBackSelector _cardSelectorPrefab;

	[SerializeField]
	private GameObject _deckboxAnchor;

	[SerializeField]
	private DeckView _deckBox;

	[SerializeField]
	private List<MeshRenderer> _meshRenderers;

	[SerializeField]
	private GameObject _readyUpGlow;

	[Header("Enemy Player")]
	[SerializeField]
	private GameObject _invitedAvatar;

	[SerializeField]
	private GameObject _playerInvited;

	[SerializeField]
	private GameObject _noPlayer;

	[SerializeField]
	private AdvancedButton _kickButton;

	[SerializeField]
	private AdvancedButton _blockButton;

	[SerializeField]
	private AdvancedButton _addFriendButton;

	[SerializeField]
	private AdvancedButton _noPlayerInviteButton;

	[SerializeField]
	private AdvancedButton _invitedPlayerInviteButton;

	public Action<string> KickButtonPressed;

	public Action<string> BlockButtonPressed;

	public Action<string> AddFriendButtonPressed;

	public Action InviteButtonPressed;

	private AssetLookupSystem _assetLookupSystem;

	private AssetLoader.AssetTracker<Sprite> _avatarBodyImageSpriteTracker;

	private IUnityObjectPool _objectPool;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private List<MeshRendererReferenceLoader> _deckMeshRendererReferenceLoaders;

	private ClientPetSelection _currentPetDisplayed;

	private string _currentSleeveDisplayed;

	private uint _currentDeckArtIdDisplayed;

	private uint _currentDeckTileIdDisplayed;

	private ISocialManager _socialManager;

	private string _currentPlayerName;

	public string PlayerId { get; set; }

	private void Awake()
	{
		_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		_objectPool = Pantry.Get<IUnityObjectPool>();
		_cardDatabase = Pantry.Get<CardDatabase>();
		_cardViewBuilder = Pantry.Get<CardViewBuilder>();
		_socialManager = Pantry.Get<ISocialManager>();
		BuildMeshRendererReferenceLoaders();
	}

	private void BuildMeshRendererReferenceLoaders()
	{
		if (_deckMeshRendererReferenceLoaders != null)
		{
			return;
		}
		_deckMeshRendererReferenceLoaders = new List<MeshRendererReferenceLoader>();
		foreach (MeshRenderer meshRenderer in _meshRenderers)
		{
			_deckMeshRendererReferenceLoaders.Add(new MeshRendererReferenceLoader(meshRenderer));
		}
	}

	public void OnEnable()
	{
		_kickButton?.onClick.AddListener(OnKickButtonClicked);
		_blockButton?.onClick.AddListener(OnBlockButtonClicked);
		_addFriendButton?.onClick.AddListener(OnAddFriendButtonClicked);
		_noPlayerInviteButton?.onClick.AddListener(OnInviteButtonClicked);
		_invitedPlayerInviteButton?.onClick.AddListener(OnInviteButtonClicked);
		_socialManager.FriendsChanged += OnFriendsOrFriendInvitesChanged;
		_socialManager.InvitesIncomingChanged += OnFriendsOrFriendInvitesChanged;
		_socialManager.InvitesOutgoingChanged += OnFriendsOrFriendInvitesChanged;
	}

	public void OnDisable()
	{
		_kickButton?.onClick.RemoveAllListeners();
		_blockButton?.onClick.RemoveAllListeners();
		_addFriendButton?.onClick.RemoveAllListeners();
		_noPlayerInviteButton?.onClick.RemoveAllListeners();
		_invitedPlayerInviteButton?.onClick.RemoveAllListeners();
		_socialManager.FriendsChanged -= OnFriendsOrFriendInvitesChanged;
		_socialManager.InvitesIncomingChanged -= OnFriendsOrFriendInvitesChanged;
		_socialManager.InvitesOutgoingChanged -= OnFriendsOrFriendInvitesChanged;
	}

	public void UpdateView(PVPChallengeData challengeData)
	{
		_kickButton?.gameObject.SetActive(value: false);
		_blockButton?.gameObject.SetActive(value: false);
		_addFriendButton?.gameObject.SetActive(value: false);
		if (string.IsNullOrEmpty(PlayerId))
		{
			_readyUpGlow.gameObject.SetActive(value: false);
			_avatar.SetActive(value: false);
			_playerInfo.SetActive(value: false);
			if (challengeData.Invites.Where((KeyValuePair<string, ChallengeInvite> invite) => invite.Value.Status == InviteStatus.Sent).ToList().Count > 0)
			{
				if (_invitedAvatar != null)
				{
					_invitedAvatar.SetActive(value: true);
				}
				if (_playerInvited != null)
				{
					_playerInvited.SetActive(value: true);
				}
				if (_noPlayer != null)
				{
					_noPlayer.SetActive(value: false);
				}
			}
			else
			{
				if (_invitedAvatar != null)
				{
					_invitedAvatar.SetActive(value: false);
				}
				if (_playerInvited != null)
				{
					_playerInvited.SetActive(value: false);
				}
				if (_noPlayer != null)
				{
					_noPlayer.SetActive(value: true);
				}
			}
			return;
		}
		_avatar.SetActive(value: true);
		_playerInfo.SetActive(value: true);
		if (_invitedAvatar != null)
		{
			_invitedAvatar.SetActive(value: false);
		}
		if (_playerInvited != null)
		{
			_playerInvited.SetActive(value: false);
		}
		if (_noPlayer != null)
		{
			_noPlayer.SetActive(value: false);
		}
		if (!challengeData.ChallengePlayers.TryGetValue(PlayerId, out var value))
		{
			return;
		}
		_currentPlayerName = value.FullDisplayName;
		if (value.PlayerId != challengeData.LocalPlayerId)
		{
			_blockButton?.gameObject.SetActive(value: true);
			UpdateAddFriendButton();
			if (challengeData.LocalPlayerId == challengeData.ChallengeOwnerId)
			{
				_kickButton?.gameObject.SetActive(value: true);
			}
		}
		_readyUpGlow.gameObject.SetActive(value.PlayerStatus == PlayerStatus.Ready);
		_playerName.SetText(SharedUtilities.FormatDisplayName(value.FullDisplayName, Color.white, Color.grey, 0u));
		_playerStatus.SetText(GetStatusLocKey(value.PlayerStatus));
		_partyLeaderCrown.SetActive(value.PlayerId == challengeData.ChallengeOwnerId);
		SetAvatar(value.Cosmetics?.avatarSelection);
		SetCompanion(value.Cosmetics?.petSelection);
		SetSleeve(value.Cosmetics?.cardBackSelection);
		SetDeckBox(value.DeckArtId, value.DeckTileId);
		if (!string.IsNullOrEmpty(value.Cosmetics?.titleSelection))
		{
			_playerTitle.gameObject.SetActive(value: true);
			_playerTitle.SetText(value.Cosmetics.titleSelection);
		}
		else
		{
			_playerTitle.gameObject.SetActive(value: false);
		}
	}

	private static string GetStatusLocKey(PlayerStatus status)
	{
		return status switch
		{
			PlayerStatus.NotReady => "MainNav/Challenges/PlayerCard/NotReadyStatus", 
			PlayerStatus.Ready => "MainNav/Challenges/PlayerCard/ReadyStatus", 
			_ => null, 
		};
	}

	private void SetAvatar(string avatarId)
	{
		if (!string.IsNullOrEmpty(avatarId))
		{
			if (_avatarBodyImageSpriteTracker == null)
			{
				_avatarBodyImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("AvaterSelectBodyImageSprite");
			}
			string avatarBustImagePath = ProfileUtilities.GetAvatarBustImagePath(_assetLookupSystem, avatarId);
			AssetLoaderUtils.TrySetSprite(_avatarImage, _avatarBodyImageSpriteTracker, avatarBustImagePath);
		}
	}

	private void SetCompanion(ClientPetSelection petSelection)
	{
		if (petSelection == null)
		{
			_companionAnchor.gameObject.transform.DestroyChildren();
		}
		else
		{
			if (_currentPetDisplayed != null && !(_currentPetDisplayed.name != petSelection.name) && !(_currentPetDisplayed.variant != petSelection.variant))
			{
				return;
			}
			_companionAnchor.gameObject.transform.DestroyChildren();
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.PetId = petSelection.name;
			_assetLookupSystem.Blackboard.PetVariantId = petSelection.variant;
			GameObject gameObject = null;
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PetPayload> loadedTree))
			{
				PetPayload payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					gameObject = _objectPool.PopObject(payload.WrapperPrefab.RelativePath);
					gameObject.transform.SetParent(_companionAnchor.transform);
					gameObject.transform.ZeroOut();
					gameObject.transform.localRotation = Quaternion.identity;
					Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
					if (componentInChildren != null && componentInChildren.ContainsParameter(InWrapper))
					{
						componentInChildren.SetBool(InWrapper, value: true);
					}
				}
			}
			_currentPetDisplayed = petSelection;
			if (gameObject != null)
			{
				_companionAnchor.gameObject.SetActive(value: true);
			}
			else
			{
				SimpleLog.LogError("Failed to load Companion name: " + petSelection.name + " variant: " + petSelection.variant);
			}
		}
	}

	private void SetSleeve(string sleeveId)
	{
		if (string.IsNullOrEmpty(sleeveId))
		{
			_sleeveAnchor.gameObject.transform.DestroyChildren();
		}
		else if (_currentSleeveDisplayed != sleeveId)
		{
			_sleeveAnchor.gameObject.transform.DestroyChildren();
			_currentSleeveDisplayed = sleeveId;
			CardData data = CardDataExtensions.CreateSkinCard(0u, _cardDatabase, null, sleeveId, faceDown: true);
			CardBackSelector cardBackSelector = UnityEngine.Object.Instantiate(_cardSelectorPrefab, _sleeveAnchor.transform);
			cardBackSelector.CardView.Init(_cardDatabase, _cardViewBuilder);
			cardBackSelector.CardView.SetData(data);
			cardBackSelector.CDC = cardBackSelector.CardView.CardView;
		}
	}

	private void SetDeckBox(uint artId, uint tileId)
	{
		if (artId != 0)
		{
			if (_currentDeckArtIdDisplayed != artId)
			{
				string text = _cardDatabase.DatabaseUtilities.GetPrintingsByArtId(artId)?.FirstOrDefault()?.ImageAssetPath;
				if (!string.IsNullOrEmpty(text))
				{
					SetDeckArt(text);
				}
				_currentDeckArtIdDisplayed = artId;
				_currentDeckTileIdDisplayed = 0u;
			}
		}
		else if (tileId != 0)
		{
			if (_currentDeckTileIdDisplayed != tileId)
			{
				string text2 = _cardDatabase.CardDataProvider.GetCardPrintingById(tileId)?.ImageAssetPath;
				if (!string.IsNullOrEmpty(text2))
				{
					SetDeckArt(text2);
				}
				_currentDeckTileIdDisplayed = tileId;
				_currentDeckArtIdDisplayed = 0u;
			}
		}
		else
		{
			_deckBox.gameObject.SetActive(value: false);
			_currentDeckTileIdDisplayed = 0u;
			_currentDeckArtIdDisplayed = 0u;
		}
	}

	private void SetDeckArt(string artPath)
	{
		_deckBox.gameObject.SetActive(value: true);
		foreach (MeshRendererReferenceLoader deckMeshRendererReferenceLoader in _deckMeshRendererReferenceLoaders)
		{
			deckMeshRendererReferenceLoader.Cleanup();
		}
		DeckBoxUtil.SetDeckBoxTexture(artPath, _cardViewBuilder.CardMaterialBuilder.TextureLoader, _cardViewBuilder.CardMaterialBuilder.CropDatabase, _deckMeshRendererReferenceLoaders.ToArray());
	}

	private void UpdateAddFriendButton()
	{
		_addFriendButton?.gameObject.SetActive(!string.IsNullOrEmpty(PlayerId) && !_socialManager.CheckIfAlreadyFriends(PlayerId) && !_socialManager.CheckIfAlreadyFriendInvited(_currentPlayerName));
	}

	private void OnFriendsOrFriendInvitesChanged()
	{
		UpdateAddFriendButton();
	}

	private void OnKickButtonClicked()
	{
		if (KickButtonPressed != null)
		{
			KickButtonPressed(PlayerId);
		}
	}

	private void OnBlockButtonClicked()
	{
		if (BlockButtonPressed != null)
		{
			BlockButtonPressed(PlayerId);
		}
	}

	private void OnAddFriendButtonClicked()
	{
		if (AddFriendButtonPressed != null)
		{
			AddFriendButtonPressed(PlayerId);
		}
	}

	private void OnInviteButtonClicked()
	{
		if (InviteButtonPressed != null)
		{
			InviteButtonPressed();
		}
	}
}
