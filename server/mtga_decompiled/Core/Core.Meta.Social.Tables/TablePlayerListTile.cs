using System;
using System.Collections;
using AssetLookupTree;
using AssetLookupTree.Payloads.Avatar;
using Core.Meta.MainNavigation.Challenge;
using DG.Tweening;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.Player;
using Wizards.Arena.Models.Network;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Core.Meta.Social.Tables;

public class TablePlayerListTile : MonoBehaviour
{
	[SerializeField]
	private Image _avatarImage;

	[SerializeField]
	private TMP_Text _displayNameText;

	[SerializeField]
	private Localize _displayNameLocalize;

	[SerializeField]
	private Localize _statusLocalize;

	[SerializeField]
	private Button _directChallengeButton;

	private readonly AssetLoader.AssetTracker<Sprite> _avatarImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("TablePlayerTileAvatarSprite");

	private TablesCornerUI _tablesCornerUI;

	private LobbyPlayer _playerModel;

	private string _currentLobbyId;

	private Color _originalChallengeButtonColor;

	private Vector3 _originalChallengeButtonScale;

	private IAccountClient AccountClient => Pantry.Get<IAccountClient>();

	private PVPChallengeController _challengeController => Pantry.Get<PVPChallengeController>();

	private void Awake()
	{
		_originalChallengeButtonColor = _directChallengeButton.GetComponent<Image>().color;
		_originalChallengeButtonScale = _directChallengeButton.transform.localScale;
		_directChallengeButton.onClick.AddListener(OnDirectChallengeButtonClicked);
		_challengeController.RegisterForChallengeChanges(OnChallengeDataChanged);
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_avatarImage, _avatarImageSpriteTracker);
		_directChallengeButton.onClick.RemoveListener(OnDirectChallengeButtonClicked);
		_challengeController.UnRegisterForChallengeChanges(OnChallengeDataChanged);
	}

	private void Update()
	{
		bool flag = _playerModel.PlayerId == AccountClient.AccountInformation?.PersonaID;
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		bool flag2 = sceneLoader != null && sceneLoader.CurrentContentType == NavContentType.Home;
		bool flag3 = _playerModel.Presence == LobbyPlayerPresence.Available;
		_directChallengeButton.gameObject.UpdateActive(!flag && flag2 && flag3);
	}

	public void SetState(TablesCornerUI tablesCornerUI, AssetLookupSystem assetLookupSystem, LobbyPlayer playerModel, string lobbyId)
	{
		_tablesCornerUI = tablesCornerUI;
		_playerModel = playerModel;
		_currentLobbyId = lobbyId;
		UnlocalizedMTGAString text = new UnlocalizedMTGAString
		{
			Key = playerModel.DisplayName
		};
		_displayNameLocalize.SetText((MTGALocalizedString)text);
		_displayNameText.color = TableUtils.TablesColorForPlayer(AccountClient, _playerModel);
		_statusLocalize.SetText(LocKeyForPresence(playerModel.Presence));
		OnChallengeDataChanged();
	}

	private void OnChallengeDataChanged(PVPChallengeData challenge = null)
	{
		if (_challengeController.GetChallengeData(_playerModel.PlayerId) != null)
		{
			_directChallengeButton.transform.DOScale(Vector3.one * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
			_directChallengeButton.GetComponent<Image>().DOColor(new Color(1f, 0.7f, 0.5f), 0.5f).SetLoops(-1, LoopType.Yoyo)
				.SetEase(Ease.InOutSine);
			return;
		}
		Image component = _directChallengeButton.GetComponent<Image>();
		DOTween.Kill(_directChallengeButton.transform, complete: true);
		_directChallengeButton.transform.localScale = _originalChallengeButtonScale;
		DOTween.Kill(component, complete: true);
		component.color = _originalChallengeButtonColor;
	}

	private static MTGALocalizedString LocKeyForPresence(LobbyPlayerPresence presence)
	{
		return presence switch
		{
			LobbyPlayerPresence.Unknown => "Social/Friends/ConnectionState/Offline", 
			LobbyPlayerPresence.Available => "Social/Presence/Available", 
			LobbyPlayerPresence.InMatch => "Social/Presence/Detail_InMatch", 
			_ => throw new ArgumentOutOfRangeException("presence", presence, null), 
		};
	}

	private void TrySetAvatar(AssetLookupSystem assetLookupSystem, string avatarId)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.CosmeticAvatarId = avatarId;
		ThumbnailPayload payload = assetLookupSystem.TreeLoader.LoadTree<ThumbnailPayload>().GetPayload(assetLookupSystem.Blackboard);
		if (payload != null)
		{
			AssetLoaderUtils.TrySetSprite(_avatarImage, _avatarImageSpriteTracker, payload.Reference.RelativePath);
		}
		assetLookupSystem.Blackboard.Clear();
	}

	public void OnDirectChallengeButtonClicked()
	{
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		if (sceneLoader.CurrentContentType != NavContentType.Home)
		{
			NavContentController currentNavContent = sceneLoader.CurrentNavContent;
			if (currentNavContent != null)
			{
				currentNavContent.OnNavBarScreenChange(delegate
				{
					StartCoroutine(Coroutine_OpenPlayBlade_SendChallenge(sceneLoader, _playerModel.PlayerId, _playerModel.DisplayName));
				});
			}
		}
		else
		{
			StartCoroutine(Coroutine_OpenPlayBlade_SendChallenge(sceneLoader, _playerModel.PlayerId, _playerModel.DisplayName));
		}
	}

	private IEnumerator Coroutine_OpenPlayBlade_SendChallenge(SceneLoader sceneLoader, string playerId, string playerName)
	{
		if (sceneLoader.CurrentContentType != NavContentType.Home)
		{
			sceneLoader.GoToLanding(new HomePageContext());
			yield return new WaitUntil(() => sceneLoader.IsLoading);
			yield return new WaitUntil(() => !sceneLoader.IsLoading);
		}
		HomePageContentController homePage = sceneLoader.CurrentNavContent as HomePageContentController;
		homePage?.HidePlayblade();
		yield return new WaitUntil(() => SceneLoader.GetSceneLoader().CurrentContentType == NavContentType.Home);
		homePage?.ChallengeBladeController.Show(PlayBladeController.PlayBladeVisualStates.Challenge);
		homePage?.ChallengeBladeController.ViewFriendChallenge(playerId);
		_tablesCornerUI.Minimize();
	}

	private string LocalPlayerDisplayName()
	{
		if (AccountClient == null || AccountClient.AccountInformation == null)
		{
			return string.Empty;
		}
		return AccountClient.AccountInformation.DisplayName;
	}
}
