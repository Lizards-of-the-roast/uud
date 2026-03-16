using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.Input;
using MTGA.KeyboardManager;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

public class NPEObjectivesController : MonoBehaviour
{
	[Serializable]
	public struct CharacterData
	{
		public Sprite Image;

		public Sprite Background;

		public Sprite Foreground;

		public Sprite PlaneImage;

		public string PlaneTitle;

		public string Title;

		[LocTerm]
		public string RewardTitle;

		public bool Hidden;

		public bool Unlocked;

		public List<RewardData> Rewards;
	}

	[Serializable]
	public struct RewardData
	{
		public enum Type
		{
			Card,
			Deck,
			Key
		}

		public Type ItemType;

		public string ItemId;
	}

	[SerializeField]
	private SparkyController _sparkyPrefab;

	[SerializeField]
	private float _sparkyScale = 1f;

	[SerializeField]
	private DismissableDeluxeTooltip _bolasIntro_prefab;

	[SerializeField]
	private CustomButton _optionsButton;

	public bool PauseProgess;

	private DismissableDeluxeTooltip _bolasIntro;

	private SparkyController _sparky;

	private Animator _stateMachine;

	[SerializeField]
	private NPEMetaDeckView _playerTutorialDeckbox;

	[SerializeField]
	private GameObject _deckInspector;

	[SerializeField]
	private TMP_Text _unlockedCardsLabel;

	[SerializeField]
	private List<NPEMetaDeckView> _decks;

	public Action PlayButtonSystemAction;

	public Action FinishSystemAction;

	[SerializeField]
	private CustomButton _playButton;

	[SerializeField]
	private NPEContentControllerRewards _rewardsController;

	[SerializeField]
	private GameObject _objectivesParent;

	[SerializeField]
	private Animator _characterForegroundAnimator;

	[SerializeField]
	private List<CharacterData> _characters;

	[SerializeField]
	private List<NPEObjective> _characterObjectives;

	private int _recentGame;

	private const int BOLASGAME = 4;

	[SerializeField]
	private float _delayBeforeCompletion = 0.5f;

	[SerializeField]
	private float _delayBeforeRewards = 0.5f;

	[SerializeField]
	private float _secondsToFillAPortion = 3f;

	[SerializeField]
	private ObjectiveProgressBar _objectiveBar;

	private Coroutine _revealRewardsCoroutine;

	[SerializeField]
	private Image _currentCharacterBackground;

	[SerializeField]
	private Image _currentCharacterForeground;

	[SerializeField]
	private float _sequenceDelay = 0.25f;

	private int _currentTrackIndex = -1;

	private NPEState _npeState;

	[SerializeField]
	private CardRolloverZoomBase _cardRolloverZoom;

	private SettingsMenuHost _settingsMenuHost;

	private bool _bolasDone;

	private int[] _rewardAmounts = new int[4] { 6, 8, 10, 12 };

	private static readonly int Rewards = Animator.StringToHash("ShowRewards");

	private static readonly int DeckInspectorReveal = Animator.StringToHash("DeckInspectorReveal");

	private static readonly int MouseOver = Animator.StringToHash("MouseOver");

	private static readonly int Click = Animator.StringToHash("Click");

	private static readonly int ClickDown = Animator.StringToHash("ClickDown");

	private static readonly int Enter = Animator.StringToHash("Enter");

	private static readonly int StageCompleted = Animator.StringToHash("StageCompleted");

	private static readonly int ReadyForGame = Animator.StringToHash("ReadyForGame");

	private static readonly int BolasRevealed = Animator.StringToHash("BolasRevealed");

	private static readonly int Stage = Animator.StringToHash("Stage");

	private static readonly int SparkySaid = Animator.StringToHash("SparkySaid");

	private static readonly int SparkyArrived = Animator.StringToHash("SparkyArrived");

	public int NumObjectives => _characters.Count;

	public void Init(NPEState npeState, CardDatabase cardDatabase, KeyboardManager keyboardManager, IActionSystem actionSystem, AssetLookupSystem assetLookupSystem, CardViewBuilder cardViewBuilder, SettingsMenuHost settingsMenuHost, DeckFormat currentEventFormat)
	{
		_settingsMenuHost = settingsMenuHost;
		_npeState = npeState;
		_stateMachine = GetComponent<Animator>();
		_sparky = UnityEngine.Object.Instantiate(_sparkyPrefab, base.transform);
		_sparky.OnArrived += OnArrived;
		_sparky.OnSaid += OnSaid;
		_sparky.gameObject.SetActive(value: false);
		_bolasIntro = UnityEngine.Object.Instantiate(_bolasIntro_prefab, base.transform);
		Transform obj = _bolasIntro.transform;
		Vector3 position = obj.position;
		obj.localPosition = new Vector3(position.x, position.y, -500f);
		IUnityObjectPool unityObjectPool = Pantry.Get<IUnityObjectPool>();
		IObjectPool genericObjectPool = Pantry.Get<IObjectPool>();
		_cardRolloverZoom.Initialize(cardViewBuilder, cardDatabase, Languages.ActiveLocProvider, unityObjectPool, genericObjectPool, keyboardManager, currentEventFormat);
		_rewardsController.NPEInit(cardDatabase, assetLookupSystem, _cardRolloverZoom, cardViewBuilder, keyboardManager, actionSystem);
		_bolasDone = false;
		_bolasIntro.gameObject.SetActive(value: false);
		_objectivesParent.SetActive(value: true);
		_characterForegroundAnimator.gameObject.SetActive(value: true);
		for (int i = 0; i < _characterObjectives.Count; i++)
		{
			_characterObjectives[i].SetImageSprite(_characters[i].Image);
			_characterObjectives[i].SetText(_characters[i].Title);
		}
		_rewardsController.CleanUp();
		_currentCharacterBackground.sprite = _characters[_characters.Count - 1].Background;
		_currentCharacterForeground.sprite = _characters[_characters.Count - 1].Foreground;
		_objectiveBar.SetPct(0f, forceVisible: true);
		_recentGame = 0;
		_playButton.OnClick.RemoveAllListeners();
		_playButton.OnClick.AddListener(PlayButtonClicked);
		_optionsButton.OnClick.RemoveAllListeners();
		_optionsButton.OnClick.AddListener(OptionsButtonClicked);
		NPEState npeState2 = _npeState;
		npeState2.InvokeNPEHomeSkipTutorialSequence = (Action)Delegate.Combine(npeState2.InvokeNPEHomeSkipTutorialSequence, new Action(ShowCompletedFromSkip));
		if (_sparky != null)
		{
			_sparky.Pause = false;
			_sparky.transform.localScale = Vector3.one * _sparkyScale;
		}
		_stateMachine.SetInteger(Stage, _recentGame);
		_playButton.gameObject.SetActive(value: false);
		if (_stateMachine != null)
		{
			StateMachineFlagSMB.RestorePersistentFlags(_stateMachine);
		}
		foreach (NPEMetaDeckView deck in _decks)
		{
			deck.Init(cardDatabase, cardViewBuilder, null);
		}
	}

	private void OnDisable()
	{
		if (_stateMachine != null)
		{
			SMBehaviour.DeactivateStateMachine(_stateMachine);
		}
		_bolasIntro.gameObject.SetActive(value: false);
		PauseProgess = false;
	}

	private void OnArrived()
	{
		_stateMachine.SetTrigger(SparkyArrived);
	}

	private void OnSaid()
	{
		_stateMachine.SetTrigger(SparkySaid);
	}

	public void ReturnToScene(int nextGame, int previousGame)
	{
		StartCoroutine(Coroutine_ShowScene(nextGame, previousGame));
	}

	private void SetBackgroundToPlane(int charNum)
	{
		_currentCharacterBackground.sprite = _characters[charNum].PlaneImage;
		_currentCharacterForeground.sprite = _characters[_characters.Count - 1].Foreground;
		_characterForegroundAnimator.SetBool(Enter, value: true);
	}

	private void OnBolasAnimationFinish()
	{
		_characterObjectives[4].UnLock();
		StartCoroutine(RevealDeckKey(4));
		_bolasDone = true;
	}

	private IEnumerator Coroutine_ShowScene(int nextGame, int previousGame, bool finishedAllGames = false)
	{
		_playButton.gameObject.SetActive(value: false);
		_playerTutorialDeckbox.DisableOpenHitbox();
		_stateMachine.SetInteger(Stage, nextGame);
		bool shouldProgressTrack;
		if (previousGame == -1)
		{
			shouldProgressTrack = false;
			_currentTrackIndex = nextGame;
		}
		else
		{
			shouldProgressTrack = previousGame != nextGame;
			if (shouldProgressTrack)
			{
				_currentTrackIndex = previousGame;
			}
			else
			{
				_currentTrackIndex = nextGame;
			}
		}
		_unlockedCardsLabel.text = "";
		bool bolasSequence = nextGame == 4 && shouldProgressTrack;
		if (shouldProgressTrack)
		{
			_currentCharacterBackground.sprite = _characters[_currentTrackIndex].Background;
			_currentCharacterForeground.sprite = _characters[_currentTrackIndex].Foreground;
		}
		else
		{
			SetBackgroundToPlane(_currentTrackIndex);
		}
		int numObjectives = _characterObjectives.Count();
		float num = 1f / (float)(numObjectives - 1);
		float percent = (float)Mathf.Max(_currentTrackIndex, 0) * num;
		if (previousGame == -1)
		{
			_rewardsController.SetUp(_currentTrackIndex);
		}
		else
		{
			AccountInformation accountInformation = Pantry.Get<IAccountClient>().AccountInformation;
			if (previousGame == 1 && MDNPlayerPrefs.CheckIfInExperimentalGroup_Experiment003(accountInformation.PersonaID))
			{
				_rewardsController.SetUp(1);
			}
		}
		for (int i = 0; i < numObjectives; i++)
		{
			if (i < _currentTrackIndex)
			{
				_characterObjectives[i].SetToCompleted();
				if (i == _currentTrackIndex - 1)
				{
					AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_progress_complete, _objectiveBar.gameObject);
				}
			}
			if (i == _currentTrackIndex)
			{
				_characterObjectives[i].SetToNormal();
				yield return RevealDeckKey(_currentTrackIndex);
			}
			if (i > _currentTrackIndex)
			{
				_characterObjectives[i].SetToLock();
			}
		}
		if (previousGame == -1)
		{
			yield return new WaitForEndOfFrame();
		}
		_objectiveBar.SetPct(percent, forceVisible: true);
		_objectiveBar.EnableSpark(isEnabled: true);
		if (!shouldProgressTrack)
		{
			_rewardsController.SetUpUnlockedCards(_currentTrackIndex);
			_playerTutorialDeckbox.EnableOpenHitbox();
			_playButton.gameObject.SetActive(value: true);
			yield break;
		}
		_rewardsController.SetUpUnlockedCards(_currentTrackIndex + 1);
		_stateMachine.SetTrigger(StageCompleted);
		_playButton.gameObject.SetActive(value: false);
		if (_currentTrackIndex < _characters.Count - 2)
		{
			yield return _rewardsController.Coroutine_AwardKey(_currentTrackIndex);
		}
		yield return new WaitForSeconds(_delayBeforeCompletion);
		_characterObjectives[_currentTrackIndex].CompleteThisGame();
		yield return null;
		do
		{
			yield return null;
		}
		while (PauseProgess);
		if (!finishedAllGames)
		{
			yield return new WaitForSeconds(_delayBeforeRewards);
			yield return ShowRewards(_characters[_currentTrackIndex]);
			_playerTutorialDeckbox.EnableOpenHitbox();
		}
		_currentTrackIndex++;
		if (finishedAllGames)
		{
			yield return ShowFinish();
		}
		else if (bolasSequence)
		{
			SetBackgroundToPlane(_characters.Count - 1);
			yield return SparkyTalkScene();
			_bolasIntro.Launch(OnBolasAnimationFinish, autoSkip: false);
			yield return BolasPlaying();
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_start, _objectiveBar.gameObject);
			yield return ProgressToTheNextBubble(_currentTrackIndex, bolasSequence);
			_stateMachine.SetBool(BolasRevealed, value: true);
			_playButton.gameObject.SetActive(value: true);
			SetBackgroundToPlane(_currentTrackIndex);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_start, _objectiveBar.gameObject);
			yield return ProgressToTheNextBubble(_currentTrackIndex, bolasSequence);
			yield return SparkyTalkScene();
			_playButton.gameObject.SetActive(value: true);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_stop, _objectiveBar.gameObject);
			SetBackgroundToPlane(_currentTrackIndex);
		}
	}

	private IEnumerator SparkyTalkScene()
	{
		_stateMachine.SetTrigger(ReadyForGame);
		yield return null;
		do
		{
			yield return null;
		}
		while (PauseProgess);
		_stateMachine.ResetTrigger(ReadyForGame);
	}

	private IEnumerator ShowFinish(bool fromSkip = false)
	{
		_rewardsController.Clear();
		_playButton.gameObject.SetActive(value: false);
		int last = _characterObjectives.Count - 1;
		_objectiveBar.SetPct(1f, forceVisible: true);
		_objectiveBar.EnableSpark(isEnabled: true);
		SetBackgroundToPlane(last);
		if (fromSkip)
		{
			_playerTutorialDeckbox.DisableOpenHitbox();
			GetComponent<Animator>().SetTrigger(StageCompleted);
			_rewardsController.AwardAllKeys();
			_characterForegroundAnimator.SetBool(Enter, value: true);
			foreach (NPEObjective characterObjective in _characterObjectives)
			{
				characterObjective.SetToCompleted();
			}
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_stop, _objectiveBar.gameObject);
		}
		else
		{
			_playerTutorialDeckbox.EnableOpenHitbox();
			yield return SparkyTalkScene();
			_playerTutorialDeckbox.DisableOpenHitbox();
			_characterObjectives[last].CompleteThisGame();
			yield return _rewardsController.Coroutine_AwardKey(last);
			yield return new WaitForSeconds(_delayBeforeRewards);
		}
		if (_npeState.FirstTutorialRunthrough)
		{
			yield return _rewardsController.Coroutine_UnlockAnimation();
			_npeState.BI_NPEProgressUpdate(new NPEState.NPEProgressContext(NPEState.NPEProgressMarker.NPE_Home_Monocolor_Deck_Reward_Shown));
			yield return ShowRewards(_characters[last + 1]);
			_playerTutorialDeckbox.EnableOpenHitbox();
			_npeState.BI_NPEProgressUpdate(new NPEState.NPEProgressContext(NPEState.NPEProgressMarker.NPE_Home_Monocolor_Deck_Reward_Dismissed));
		}
		FinishSystemAction();
	}

	private IEnumerator ShowRewards(CharacterData data)
	{
		if (_rewardsController == null)
		{
			yield break;
		}
		List<Guid> list = new List<Guid>();
		foreach (RewardData reward in data.Rewards)
		{
			if (reward.ItemType == RewardData.Type.Deck)
			{
				Guid item = new Guid(reward.ItemId);
				list.Add(item);
			}
		}
		Promise<Dictionary<Guid, Client_Deck>> promise = _npeState.PreconDeckManager.EnsurePreconDecks();
		yield return promise.AsCoroutine();
		ClientInventoryUpdateReportItem t = new ClientInventoryUpdateReportItem();
		List<Guid> list2 = new List<Guid>();
		foreach (RewardData reward2 in data.Rewards)
		{
			switch (reward2.ItemType)
			{
			case RewardData.Type.Card:
				t.aetherizedCards.Add(new AetherizedCardInformation
				{
					grpId = int.Parse(reward2.ItemId)
				});
				break;
			case RewardData.Type.Deck:
				try
				{
					list2.Add(new Guid(reward2.ItemId));
				}
				catch
				{
					Debug.LogErrorFormat("Invalid GUID ({0}) specified for deckId in reward {1}", reward2.ItemId, data.Title);
				}
				break;
			}
		}
		t.delta = new InventoryDelta
		{
			decksAdded = list2.ToArray()
		};
		_stateMachine.SetBool(Rewards, value: true);
		_rewardsController.RegisterRewardWillCloseCallback(RewardsButtonClicked);
		yield return _rewardsController.AddAndDisplayRewardsCoroutine(t, Languages.ActiveLocProvider.GetLocalizedText(data.RewardTitle), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
		_rewardsController.DisableDeckPreviewPopup();
		yield return new WaitUntil(() => !_rewardsController.Visible);
		_playerTutorialDeckbox.EnableOpenHitbox();
		_stateMachine.SetBool(Rewards, value: false);
		_rewardsController.EnableDeckPreviewPopup();
	}

	private IEnumerator ProgressToTheNextBubble(int nextGame, bool isthebolas)
	{
		int num = _characterObjectives.Count((NPEObjective o) => o.gameObject.activeSelf);
		float num2 = 1f / (float)(num - 1);
		float percent = (float)Mathf.Max(nextGame - 1, 0) * num2;
		float desiredPercentage = (float)nextGame * num2;
		float speed = num2 / _secondsToFillAPortion;
		while (percent <= desiredPercentage)
		{
			percent += Time.deltaTime * speed;
			_objectiveBar.SetPct(percent, forceVisible: true);
			yield return null;
		}
		if (!isthebolas && nextGame < _characterObjectives.Count)
		{
			_characterObjectives[nextGame].UnLock();
			yield return RevealDeckKey(nextGame);
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_stop, _objectiveBar.gameObject);
	}

	private IEnumerator BolasPlaying()
	{
		while (!_bolasDone)
		{
			yield return null;
		}
	}

	private IEnumerator RevealDeckKey(int keyIndex)
	{
		yield return new WaitForEndOfFrame();
		Vector3 position = new Vector3(_characterObjectives[keyIndex].transform.position.x, _characterObjectives[keyIndex].transform.position.y - 0.5f, _characterObjectives[keyIndex].transform.position.z);
		yield return _rewardsController.Coroutine_CreateKey(keyIndex, position, _characterObjectives[keyIndex].transform.rotation);
	}

	private IEnumerator RevealRewards()
	{
		bool unrevealed = false;
		IEnumerable<RewardDisplayCard> enumerable = _rewardsController.gameObject.GetComponentsInChildren<RewardDisplayCard>().Where(delegate(RewardDisplayCard card)
		{
			AnimatorStateInfo currentAnimatorStateInfo = card.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
			return currentAnimatorStateInfo.IsName("Intro") || currentAnimatorStateInfo.IsName("Unrevealed");
		});
		if (enumerable.Any())
		{
			unrevealed = true;
			foreach (RewardDisplayCard item in enumerable)
			{
				item.GetComponent<Animator>().SetTrigger(ClickDown);
				item.GetComponent<Animator>().SetTrigger(Click);
				yield return new WaitForSeconds(_sequenceDelay);
			}
		}
		IEnumerable<MetaDeckView> enumerable2 = from deck in _rewardsController.gameObject.GetComponentsInChildren<MetaDeckView>()
			where !deck.GetComponent<Animator>().GetCurrentAnimatorStateInfo(2).IsName("RewardInteract_DeckBoxLid_Open")
			select deck;
		if (enumerable2.Any())
		{
			unrevealed = true;
			foreach (MetaDeckView item2 in enumerable2)
			{
				item2.TriggerOpenEffect();
				item2.GetComponent<Animator>().SetTrigger(MouseOver);
				yield return new WaitForSeconds(_sequenceDelay);
			}
		}
		if (!unrevealed)
		{
			_rewardsController.Clear();
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_claim_reward, _playButton.gameObject);
		}
	}

	public void ShowSceneAfterFinishingAllGames()
	{
		StartCoroutine(Coroutine_ShowScene(NPEState.NPE_games.Count(), NPEState.NPE_games.Count() - 1, finishedAllGames: true));
	}

	public void ShowCompletedFromSkip()
	{
		_playButton.gameObject.SetActive(value: false);
		StartCoroutine(ShowFinish(fromSkip: true));
	}

	public void PlayButtonClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_stop, _objectiveBar.gameObject);
		_objectiveBar.EnableSpark(isEnabled: false);
		_playButton.gameObject.SetActive(value: false);
		PlayButtonSystemAction();
	}

	private void RewardsButtonClicked()
	{
		if (_revealRewardsCoroutine != null)
		{
			StopCoroutine(_revealRewardsCoroutine);
		}
		_revealRewardsCoroutine = StartCoroutine(RevealRewards());
	}

	private void OptionsButtonClicked()
	{
		_settingsMenuHost.Open();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
	}

	public void ShowDeckContentsDisplay()
	{
		GetComponent<Animator>().enabled = false;
		_deckInspector.SetActive(value: true);
		StartCoroutine(Coroutine_StaggerCardReveal());
	}

	private IEnumerator Coroutine_StaggerCardReveal()
	{
		RewardDisplayCard[] componentsInChildren = _deckInspector.GetComponentsInChildren<RewardDisplayCard>(includeInactive: true);
		foreach (RewardDisplayCard obj in componentsInChildren)
		{
			obj.GetComponent<Animator>().enabled = false;
			obj.card.gameObject.SetActive(value: false);
		}
		RewardDisplayCard[] componentsInChildren2 = _deckInspector.GetComponentsInChildren<RewardDisplayCard>(includeInactive: true);
		foreach (RewardDisplayCard obj2 in componentsInChildren2)
		{
			obj2.GetComponent<Animator>().enabled = true;
			obj2.GetComponent<Animator>().SetBool(DeckInspectorReveal, value: true);
			obj2.card.gameObject.SetActive(value: true);
			yield return new WaitForSeconds(0.04f);
		}
	}

	public void HideDeckContentDisplay()
	{
		_rewardsController.HideDeckInspector(GetComponent<Animator>());
	}

	public void UpdateCardsUnlockedText()
	{
		if (_currentTrackIndex <= _rewardAmounts.Length)
		{
			_unlockedCardsLabel.enabled = true;
			_unlockedCardsLabel.text = Languages.ActiveLocProvider.GetLocalizedText("NPE/Rewards/NPE_Tutorial_Deck_Unlocks", ("number", _rewardAmounts[_currentTrackIndex - 1].ToString()));
		}
		else
		{
			_unlockedCardsLabel.enabled = false;
		}
	}

	public void Debug_Reset(Action callback = null)
	{
		foreach (NPEObjective characterObjective in _characterObjectives)
		{
			UnityEngine.Object.Destroy(characterObjective.gameObject);
		}
		if (_rewardsController != null)
		{
			_rewardsController.Clear();
		}
		callback?.Invoke();
	}

	private void OnDestroy()
	{
		_playButton.OnClick.RemoveAllListeners();
		_optionsButton.OnClick.RemoveAllListeners();
		NPEState npeState = _npeState;
		npeState.InvokeNPEHomeSkipTutorialSequence = (Action)Delegate.Remove(npeState.InvokeNPEHomeSkipTutorialSequence, new Action(ShowCompletedFromSkip));
	}
}
