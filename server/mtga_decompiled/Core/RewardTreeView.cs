using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Utils;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Arena.Promises;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class RewardTreeView : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	private enum ButtonState
	{
		Claim,
		Suggest,
		Hidden
	}

	private struct AnimatingOrbInfos
	{
		public GameObject animatingOrb;

		public Vector3 origin;

		public GameObject unlockVfx;

		public EPP_OrbSlotView[] targetSlots;
	}

	public string TrackName;

	public Transform OrbSlotConnectorLayer;

	[SerializeField]
	private float _timeToEngageHanger;

	[SerializeField]
	private Dictionary<int, EPP_OrbSlotView> _orbSlotViews;

	[SerializeField]
	private GameObject _unlockVfx;

	[SerializeField]
	private bool _useClickAndConfirm;

	[SerializeField]
	private AnimationCurve _moveOrbToSlotCurve;

	[SerializeField]
	private float _moveOrbToSlotStaggerDelaySeconds = 0.2f;

	[NonSerialized]
	public bool PauseAnimation;

	private ContentControllerRewards _rewardsPanel;

	private EPPDeckUpgradeController _deckUpgrade;

	private bool _ongoingOrbTransaction;

	private bool _ongoingColorOrbTransaction;

	private int _suggestedPick = -1;

	private readonly List<int> _currentPicks = new List<int>();

	private bool _isUnlocking;

	private List<GameObject> _orbsToAnimate;

	private Dictionary<GameObject, Vector3> _animatingOrbOrigins;

	private bool? _useLargeOrbs;

	private EPP_OrbSlotView _currentlyShowingOrbSlot;

	private EPP_OrbSlotView _currentlyHoveredOrbSlot;

	private EPP_OrbSlotView _currentlySelectedOrbSlot;

	private EPP_OrbSlotView _challengerOrbSlot;

	private DateTime _challengeStarted;

	private static readonly int InsertOrb = Animator.StringToHash("InsertOrb");

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private CardMaterialBuilder _cardMaterialBuilder;

	private static SetMasteryDataProvider MasteryPassProvider => Pantry.Get<SetMasteryDataProvider>();

	public event Action<EPP_OrbSlotView> OnShowHanger;

	public event Action OnClearHanger;

	public event Action<bool> OnUpdateOrangeButton;

	public event Action<bool> OnUpdateOutlineButton;

	public event Action<int> OnUpdateOrbPicks;

	public event Action<int> OnUpdateOrbInvNumber;

	public event Action<MTGALocalizedString> OnUpdateOrbPlacedNumber;

	private float MoveOrbToSlotCurve(float t)
	{
		if (_moveOrbToSlotCurve != null && _moveOrbToSlotCurve.length != 0)
		{
			return _moveOrbToSlotCurve.Evaluate(t);
		}
		return DefaultMoveOrbToSlotCurve(t);
	}

	private float DefaultMoveOrbToSlotCurve(float t)
	{
		if (!(t < 0.5f))
		{
			return 1f - (t = (1f - t) * 2f) * t * t * 0.5f;
		}
		return (t *= 2f) * t * t * 0.5f;
	}

	public void InitializeRewardTreeView(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, bool useLargeOrbs, List<GameObject> orbsToAnimate = null)
	{
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_cardMaterialBuilder = cardMaterialBuilder;
		_useLargeOrbs = useLargeOrbs;
		if (_orbsToAnimate == null && orbsToAnimate != null)
		{
			_orbsToAnimate = orbsToAnimate;
			foreach (GameObject item in _orbsToAnimate)
			{
				item.UpdateActive(active: false);
				_animatingOrbOrigins[item] = item.transform.position;
			}
		}
		if (_orbSlotViews != null)
		{
			foreach (EPP_OrbSlotView value2 in _orbSlotViews.Values)
			{
				if (!PlatformUtils.IsHandheld())
				{
					value2.PointerEnterOrbSlot = (Action<EPP_OrbSlotView>)Delegate.Remove(value2.PointerEnterOrbSlot, new Action<EPP_OrbSlotView>(InspectOrbSlot));
					value2.PointerExitOrbSlot = (Action<EPP_OrbSlotView>)Delegate.Remove(value2.PointerExitOrbSlot, new Action<EPP_OrbSlotView>(DisengageOrbSlot));
				}
				if (_useClickAndConfirm)
				{
					value2.ClickOrbSlot = (Action<EPP_OrbSlotView>)Delegate.Remove(value2.ClickOrbSlot, new Action<EPP_OrbSlotView>(PickOrbSlot));
				}
				else
				{
					value2.ClickOrbSlot = (Action<EPP_OrbSlotView>)Delegate.Remove(value2.ClickOrbSlot, new Action<EPP_OrbSlotView>(UnlockOrbSlot));
				}
				value2.SetState(OrbSlot.OrbState.Unavailable);
			}
		}
		Dictionary<int, OrbSlot> orbSlotMap = MasteryPassProvider.GetOrbSlotMap(TrackName);
		_orbSlotViews = new Dictionary<int, EPP_OrbSlotView>();
		foreach (EPP_OrbSlotView item2 in from v in GetComponentsInChildren<EPP_OrbSlotView>()
			orderby v.MyIDForServerPairing
			select v)
		{
			int myIDForServerPairing = item2.MyIDForServerPairing;
			if (myIDForServerPairing != -1)
			{
				if (_orbSlotViews.ContainsKey(myIDForServerPairing))
				{
					Debug.LogError($"More than one orbView has the same id {myIDForServerPairing} for track {TrackName}");
					continue;
				}
				_orbSlotViews.Add(myIDForServerPairing, item2);
				item2.SetState(OrbSlot.OrbState.Unavailable);
			}
		}
		foreach (var (num2, orbslot) in orbSlotMap)
		{
			if (_orbSlotViews.TryGetValue(num2, out var value))
			{
				value.Init(orbslot, this, useLargeOrbs);
				if (!PlatformUtils.IsHandheld())
				{
					EPP_OrbSlotView ePP_OrbSlotView = value;
					ePP_OrbSlotView.PointerEnterOrbSlot = (Action<EPP_OrbSlotView>)Delegate.Combine(ePP_OrbSlotView.PointerEnterOrbSlot, new Action<EPP_OrbSlotView>(InspectOrbSlot));
					EPP_OrbSlotView ePP_OrbSlotView2 = value;
					ePP_OrbSlotView2.PointerExitOrbSlot = (Action<EPP_OrbSlotView>)Delegate.Combine(ePP_OrbSlotView2.PointerExitOrbSlot, new Action<EPP_OrbSlotView>(DisengageOrbSlot));
				}
				if (_useClickAndConfirm)
				{
					EPP_OrbSlotView ePP_OrbSlotView3 = value;
					ePP_OrbSlotView3.ClickOrbSlot = (Action<EPP_OrbSlotView>)Delegate.Combine(ePP_OrbSlotView3.ClickOrbSlot, new Action<EPP_OrbSlotView>(PickOrbSlot));
				}
				else
				{
					EPP_OrbSlotView ePP_OrbSlotView4 = value;
					ePP_OrbSlotView4.ClickOrbSlot = (Action<EPP_OrbSlotView>)Delegate.Combine(ePP_OrbSlotView4.ClickOrbSlot, new Action<EPP_OrbSlotView>(UnlockOrbSlot));
				}
			}
			else
			{
				Debug.LogError($"Missing orbs slot view {num2} in {TrackName}");
			}
		}
		SetOrbVisualsToModelData();
		SetWebVisualsToModelData();
	}

	private void OnEnable()
	{
		this.OnClearHanger?.Invoke();
		if (_currentlyShowingOrbSlot != null)
		{
			_currentlyShowingOrbSlot.SetPresenting(isPresenting: false);
		}
		_currentlyShowingOrbSlot = null;
		_ongoingColorOrbTransaction = false;
		_ongoingOrbTransaction = false;
	}

	private void OnDisable()
	{
		_unlockVfx.UpdateActive(active: false);
		foreach (GameObject item in _orbsToAnimate)
		{
			item.UpdateActive(active: false);
			item.transform.position = _animatingOrbOrigins[item];
		}
	}

	private void Awake()
	{
		_animatingOrbOrigins = new Dictionary<GameObject, Vector3>();
	}

	private void SetButtonState(ButtonState state)
	{
		this.OnUpdateOrangeButton?.Invoke(state == ButtonState.Claim);
		this.OnUpdateOutlineButton?.Invoke(state == ButtonState.Suggest);
	}

	private EPP_OrbSlotView GetOrbView(int id)
	{
		_orbSlotViews.TryGetValue(id, out var value);
		return value;
	}

	private int GetIdFromView(EPP_OrbSlotView view)
	{
		return view.OrbSlotModel.serverRewardNode.id;
	}

	public void InspectOrbSlot(EPP_OrbSlotView slotView)
	{
		if (!_isUnlocking)
		{
			if (_currentlyShowingOrbSlot == null)
			{
				ShowHanger(slotView);
			}
			else
			{
				_currentlyHoveredOrbSlot = slotView;
			}
			if (_currentlyHoveredOrbSlot != _currentlySelectedOrbSlot)
			{
				SetButtonState((_currentPicks.Count <= 0) ? ButtonState.Suggest : ButtonState.Hidden);
			}
		}
	}

	public void DisengageOrbSlot(EPP_OrbSlotView slotView)
	{
		if (!_isUnlocking)
		{
			if (_currentlySelectedOrbSlot != null)
			{
				_currentlyHoveredOrbSlot = _currentlySelectedOrbSlot;
				SetButtonState(ButtonState.Claim);
			}
			else
			{
				_currentlyHoveredOrbSlot = null;
			}
		}
	}

	private void Update()
	{
		if (_currentlyHoveredOrbSlot != null && !_isUnlocking)
		{
			if (_challengerOrbSlot == null || _currentlyHoveredOrbSlot.MyIDForServerPairing != _challengerOrbSlot.MyIDForServerPairing)
			{
				_challengerOrbSlot = _currentlyHoveredOrbSlot;
				_challengeStarted = DateTime.UtcNow;
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_masterytree_rollover, base.gameObject);
			}
			else if ((float)(DateTime.UtcNow - _challengeStarted).Milliseconds > _timeToEngageHanger && (_currentlyShowingOrbSlot == null || _challengerOrbSlot.MyIDForServerPairing != _currentlyShowingOrbSlot.MyIDForServerPairing))
			{
				ShowHanger(_challengerOrbSlot);
			}
		}
	}

	private void UnlockOrbSlot(EPP_OrbSlotView slotView)
	{
		UnlockOrbSlots(new List<EPP_OrbSlotView> { slotView });
	}

	private void UnlockOrbSlots(List<EPP_OrbSlotView> slotViews)
	{
		_isUnlocking = true;
		EventSystem.current.SetSelectedGameObject(null);
		if (slotViews.All((EPP_OrbSlotView _) => IsClickableOrb(_, checkOrbCounts: false)))
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_masterytree_orb_click, base.gameObject);
			this.OnClearHanger?.Invoke();
			if (_currentlyShowingOrbSlot != null)
			{
				_currentlyShowingOrbSlot.SetPresenting(isPresenting: false);
			}
			_currentlyShowingOrbSlot = null;
			List<OrbSlot> slots = slotViews.Select((EPP_OrbSlotView _) => _.OrbSlotModel).ToList();
			StartCoroutine(Coroutine_SpendOrbsOnSlots(slots));
		}
	}

	private bool IsClickableOrb(EPP_OrbSlotView slotView, bool checkOrbCounts = true)
	{
		OrbSlot orbSlotModel = slotView.OrbSlotModel;
		if ((orbSlotModel.currentState == OrbSlot.OrbState.Available || slotView.VisibleSlotState == OrbSlot.OrbState.Available) && string.IsNullOrEmpty(orbSlotModel.serverRewardNode.unlockQuestMetric) && (!checkOrbCounts || MasteryPassProvider.GetOrbCount(TrackName) > _currentPicks.Count) && !_ongoingColorOrbTransaction)
		{
			return !_ongoingOrbTransaction;
		}
		return false;
	}

	public void InitDisplay(ContentControllerRewards rewardsPanel, EPPDeckUpgradeController deckUpgrade)
	{
		_rewardsPanel = rewardsPanel;
		_deckUpgrade = deckUpgrade;
		_deckUpgrade.Init(_cardDatabase, _cardViewBuilder);
		SetOrbVisualsToModelData();
		StartCoroutine(Coroutine_PlayAnyQueuedUpUnlockingOfColorOrbSlots());
	}

	private void UpdateUnspentOrbVisuals(int numberOrbs)
	{
		this.OnUpdateOrbInvNumber?.Invoke(numberOrbs);
		bool flag = false;
		if (numberOrbs > 0)
		{
			_suggestedPick = -1;
			Dictionary<int, OrbSlot> orbSlotMap = MasteryPassProvider.GetOrbSlotMap(TrackName);
			for (int num = orbSlotMap.Count - 1; num >= 0; num--)
			{
				if (orbSlotMap.TryGetValue(num, out var value) && value.currentState == OrbSlot.OrbState.Available && string.IsNullOrEmpty(value.serverRewardNode.unlockQuestMetric))
				{
					_suggestedPick = value.serverRewardNode.id;
					break;
				}
			}
			flag = _suggestedPick != -1;
		}
		bool flag2 = !_ongoingColorOrbTransaction && !_ongoingOrbTransaction;
		SetButtonState((flag && flag2) ? ButtonState.Suggest : ButtonState.Hidden);
	}

	private void SetOrbVisualsToModelData()
	{
		Dictionary<int, OrbSlot> orbSlotMap = MasteryPassProvider.GetOrbSlotMap(TrackName);
		if (orbSlotMap == null)
		{
			return;
		}
		int num = 0;
		foreach (OrbSlot value in orbSlotMap.Values)
		{
			if (value.currentState == OrbSlot.OrbState.Unlocked)
			{
				num++;
			}
		}
		MTGALocalizedString mTGALocalizedString = "MainNav/General/X_Of_Y";
		mTGALocalizedString.Parameters = new Dictionary<string, string>
		{
			{
				"x",
				num.ToString()
			},
			{
				"y",
				orbSlotMap.Count.ToString()
			}
		};
		this.OnUpdateOrbPlacedNumber?.Invoke(mTGALocalizedString);
		UpdateUnspentOrbVisuals(MasteryPassProvider.GetOrbCount(TrackName));
	}

	private void SetWebVisualsToModelData()
	{
		if (_orbSlotViews == null)
		{
			return;
		}
		foreach (EPP_OrbSlotView value in _orbSlotViews.Values)
		{
			value.SetToModel();
		}
	}

	public void SuggestPick()
	{
		HighlightPick(_suggestedPick);
	}

	private void ShowHanger(EPP_OrbSlotView slotView)
	{
		if (_currentlyShowingOrbSlot != null)
		{
			_currentlyShowingOrbSlot.SetPresenting(isPresenting: false);
		}
		OrbSlot orbSlotModel = slotView.OrbSlotModel;
		EPP_OrbSlotView orbView = GetOrbView(orbSlotModel.serverRewardNode.id);
		slotView.SetPresenting(isPresenting: true);
		_currentlyShowingOrbSlot = slotView;
		this.OnShowHanger?.Invoke(orbView);
	}

	private void HighlightPick(int pickId)
	{
		if (pickId == -1)
		{
			return;
		}
		EPP_OrbSlotView ePP_OrbSlotView = _orbSlotViews[pickId];
		Vector3 position = ePP_OrbSlotView.transform.position;
		position.z = _unlockVfx.transform.position.z;
		_unlockVfx.transform.position = position;
		_unlockVfx.UpdateActive(active: false);
		_unlockVfx.SetActive(value: true);
		ShowHanger(ePP_OrbSlotView);
		ePP_OrbSlotView.SetSuggested();
		SetButtonState(ButtonState.Claim);
		if (!_currentPicks.Contains(pickId))
		{
			_currentPicks.Add(pickId);
			this.OnUpdateOrbPicks?.Invoke(_currentPicks.Count);
		}
		_currentlyHoveredOrbSlot = ePP_OrbSlotView;
		_currentlySelectedOrbSlot = ePP_OrbSlotView;
		ePP_OrbSlotView.SetSelected(isSelected: true);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_masterytree_rollover, base.gameObject);
		foreach (int childId in ePP_OrbSlotView.OrbSlotModel.serverRewardNode.childIds)
		{
			GetOrbView(childId).SetToModel();
		}
	}

	public void AutoPick()
	{
		Dictionary<int, OrbSlot> orbSlotMap = MasteryPassProvider.GetOrbSlotMap(TrackName);
		List<EPP_OrbSlotView> list = new List<EPP_OrbSlotView>();
		foreach (int currentPick in _currentPicks)
		{
			orbSlotMap.TryGetValue(currentPick, out var value);
			if (value == null)
			{
				SimpleLog.LogError($"Bad slot ID {currentPick} in currently picked orbs");
				list = null;
				break;
			}
			list.Add(_orbSlotViews[value.serverRewardNode.id]);
		}
		if (list != null && list.Count > 0)
		{
			SetButtonState(ButtonState.Hidden);
			UnlockOrbSlots(list);
		}
	}

	public void PickOrbSlot(EPP_OrbSlotView slotView)
	{
		int idFromView = GetIdFromView(slotView);
		if (_currentPicks.Contains(idFromView))
		{
			if (!AllDescendentIds(idFromView).Except(new int[1] { idFromView }).Intersect(_currentPicks).Any())
			{
				AutoPick();
			}
			return;
		}
		InspectOrbSlot(slotView);
		if (IsClickableOrb(slotView))
		{
			OrbSlot orbSlotModel = slotView.OrbSlotModel;
			HighlightPick(orbSlotModel.serverRewardNode.id);
		}
		else
		{
			ClearCurrentSelectedPicks();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		this.OnClearHanger?.Invoke();
		if (_currentlyShowingOrbSlot != null)
		{
			_currentlyShowingOrbSlot.SetPresenting(isPresenting: false);
		}
		_currentlyHoveredOrbSlot = null;
		_currentlyShowingOrbSlot = null;
		if (_currentPicks.Count > 0)
		{
			ClearCurrentSelectedPicks();
		}
	}

	private void ClearCurrentSelectedPicks()
	{
		int[] slotIds = _currentPicks.SelectMany(AllDescendentIds).Distinct().ToArray();
		foreach (int currentPick in _currentPicks)
		{
			GetOrbView(currentPick).SetSelected(isSelected: false);
		}
		ResetSlotsToModel(slotIds);
		_currentPicks.Clear();
		_currentlySelectedOrbSlot = null;
		this.OnUpdateOrbPicks?.Invoke(_currentPicks.Count);
		SetButtonState(ButtonState.Suggest);
	}

	private void ResetSlotsToModel(IEnumerable<int> slotIds)
	{
		foreach (int slotId in slotIds)
		{
			GetOrbView(slotId).SetToModel();
		}
	}

	private int[] AllDescendentIds(int slotId)
	{
		return GetOrbView(slotId).OrbSlotModel.serverRewardNode.childIds.SelectMany(AllDescendentIds).Append(slotId).ToArray();
	}

	private IEnumerator ActivateAndWaitForIntroAnim(GameObject animatingOrb, Vector3 origin)
	{
		Animator orbToMoveAnimator = animatingOrb.GetComponent<Animator>();
		animatingOrb.transform.position = origin;
		animatingOrb.SetActive(value: true);
		yield return null;
		AnimatorStateInfo currentAnimatorStateInfo = orbToMoveAnimator.GetCurrentAnimatorStateInfo(0);
		int introName = currentAnimatorStateInfo.shortNameHash;
		while (currentAnimatorStateInfo.normalizedTime < 1f && currentAnimatorStateInfo.shortNameHash == introName)
		{
			yield return null;
			currentAnimatorStateInfo = orbToMoveAnimator.GetCurrentAnimatorStateInfo(0);
		}
	}

	private IEnumerator Coroutine_AnimateOrbsToUnlockSlots(AnimatingOrbInfos animatingOrbInfos)
	{
		bool flag = animatingOrbInfos.targetSlots.Length != 0;
		if (flag)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_masterytree_orb_flying, base.gameObject);
		}
		List<IEnumerator> list = new List<IEnumerator>();
		List<GameObject> extraGameObjects = new List<GameObject>();
		int siblingIndex = animatingOrbInfos.animatingOrb.transform.GetSiblingIndex();
		foreach (var item3 in animatingOrbInfos.targetSlots.Select((EPP_OrbSlotView _, int idx) => (_: _, idx: idx)))
		{
			EPP_OrbSlotView item = item3._;
			int item2 = item3.idx;
			GameObject gameObject = UnityEngine.Object.Instantiate(animatingOrbInfos.animatingOrb, animatingOrbInfos.animatingOrb.transform.parent);
			gameObject.transform.SetSiblingIndex(siblingIndex + 1);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(animatingOrbInfos.unlockVfx, animatingOrbInfos.unlockVfx.transform.parent);
			extraGameObjects.Add(gameObject);
			extraGameObjects.Add(gameObject2);
			float delay = _moveOrbToSlotStaggerDelaySeconds * (float)item2;
			list.Add(Coroutine_AnimateOrbToUnlockSlot(gameObject, animatingOrbInfos.origin, item, gameObject2, delay));
		}
		if (flag)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_masterytree_orb_set, base.gameObject);
		}
		yield return list.WaitOnAll(this);
		foreach (GameObject item4 in extraGameObjects)
		{
			UnityEngine.Object.Destroy(item4);
		}
		_currentlySelectedOrbSlot = null;
		_isUnlocking = false;
	}

	private IEnumerator Coroutine_AnimateOrbToUnlockSlot(GameObject animatingOrb, Vector3 animatingOrbOrigin, EPP_OrbSlotView targetSlot, GameObject unlockVfx, float delay = 0f)
	{
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		Animator orbToMoveAnimator = animatingOrb.GetComponent<Animator>();
		if (!animatingOrb.activeSelf)
		{
			yield return ActivateAndWaitForIntroAnim(animatingOrb, animatingOrbOrigin);
		}
		while (PauseAnimation)
		{
			yield return null;
		}
		orbToMoveAnimator.SetTrigger(InsertOrb);
		yield return null;
		Vector3 targetPosition = targetSlot.transform.position;
		while (orbToMoveAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			float normalizedTime = orbToMoveAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			float t = MoveOrbToSlotCurve(normalizedTime);
			animatingOrb.transform.position = Vector3.Lerp(animatingOrbOrigin, targetPosition, t);
			yield return null;
		}
		animatingOrb.SetActive(value: false);
		animatingOrb.transform.position = animatingOrbOrigin;
		targetSlot.SetState(OrbSlot.OrbState.Unlocked);
		targetSlot.SetSelected(isSelected: false);
		Vector3 position = targetPosition;
		position.z = unlockVfx.transform.position.z;
		unlockVfx.transform.position = position;
		unlockVfx.UpdateActive(active: false);
		unlockVfx.SetActive(value: true);
	}

	private IEnumerator Coroutine_SpendOrbsOnSlots(List<OrbSlot> slots)
	{
		_ongoingOrbTransaction = true;
		int[] nodeIds = slots.Select((OrbSlot _) => _.serverRewardNode.id).ToArray();
		ClientInventoryUpdateReportItem inventoryUpdate = null;
		WrapperController.Instance.InventoryManager.Subscribe(InventoryUpdateSource.BattlePassLevelMasteryTree, OnInventoryUpdated);
		WrapperController.Instance.InventoryManager.Subscribe(InventoryUpdateSource.EarlyPlayerProgressionMasteryTree, OnInventoryUpdated);
		WrapperController.Instance.InventoryManager.Subscribe(InventoryUpdateSource.CampaignGraphPurchaseNode, OnInventoryUpdated);
		Promise<ClientPlayerTrackUpdate> spendOrbRequestFiber = MasteryPassProvider.SpendOrbsOnNodes(TrackName, nodeIds);
		yield return spendOrbRequestFiber.AsCoroutine().WithLoadingIndicator();
		ClientPlayerTrackUpdate result = spendOrbRequestFiber.Result;
		if (!spendOrbRequestFiber.Successful)
		{
			switch (MasteryPassProvider.GetError(spendOrbRequestFiber))
			{
			case MasteryPassError.TrackDisabled:
			case MasteryPassError.WebDisabled:
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("EPP/RewardWeb/OrbSpend_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("EPP/RewardWeb/OrbSpend_Error_TrackOrWebDisabled_Text"));
				break;
			case MasteryPassError.NotEnoughOrbs:
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("EPP/RewardWeb/OrbSpend_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("EPP/RewardWeb/OrbSpend_Error_NotEnoughOrbs_Text"));
				break;
			case MasteryPassError.MetricUnlockOnly:
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("EPP/RewardWeb/OrbSpend_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("EPP/RewardWeb/OrbSpend_Error_MetricUnlockOnly_Text"));
				break;
			default:
				StartCoroutine(MasteryPassProvider.Refresh());
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("EPP/RewardWeb/OrbSpend_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Generic_NoncriticalError_Text"), delegate
				{
					SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
				});
				break;
			}
			_ongoingOrbTransaction = false;
			yield break;
		}
		MasteryPassProvider.ProcessTrackUpdate(result);
		UpdateUnspentOrbVisuals(result.orbCountDiff.currentOrbCount);
		(OrbSlot, EPP_OrbSlotView)[] orbViews = slots.Select((OrbSlot _) => (_: _, GetOrbView(_.serverRewardNode.id))).ToArray();
		GameObject gameObject = _orbsToAnimate[_orbsToAnimate.Count - 1];
		gameObject.GetComponent<Animator>();
		AnimatingOrbInfos animatingOrbInfos = new AnimatingOrbInfos
		{
			animatingOrb = gameObject,
			origin = _animatingOrbOrigins[gameObject],
			unlockVfx = _unlockVfx,
			targetSlots = orbViews.Select(((OrbSlot, EPP_OrbSlotView) _) => _.Item2).ToArray()
		};
		yield return Coroutine_AnimateOrbsToUnlockSlots(animatingOrbInfos);
		_currentPicks.Clear();
		_currentlySelectedOrbSlot = null;
		this.OnUpdateOrbPicks?.Invoke(_currentPicks.Count);
		yield return new WaitForSeconds(1f);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_masterytree_orb_reward, base.gameObject);
		yield return new WaitUntil(() => inventoryUpdate != null);
		WrapperController.Instance.InventoryManager.UnSubscribe(InventoryUpdateSource.BattlePassLevelMasteryTree, OnInventoryUpdated);
		WrapperController.Instance.InventoryManager.UnSubscribe(InventoryUpdateSource.EarlyPlayerProgressionMasteryTree, OnInventoryUpdated);
		WrapperController.Instance.InventoryManager.UnSubscribe(InventoryUpdateSource.CampaignGraphPurchaseNode, OnInventoryUpdated);
		(OrbSlot, EPP_OrbSlotView)[] array = orbViews;
		for (int num = 0; num < array.Length; num++)
		{
			(OrbSlot, EPP_OrbSlotView) tuple = array[num];
			OrbSlot item = tuple.Item1;
			EPP_OrbSlotView item2 = tuple.Item2;
			UpgradePacket upgrade = item.serverRewardNode.upgradePacket;
			if (upgrade == null)
			{
				continue;
			}
			_deckUpgrade.DisplayUpgrade(upgrade, item2.Title, _cardDatabase, _cardViewBuilder, _cardMaterialBuilder);
			yield return new WaitUntil(() => !_deckUpgrade.gameObject.activeSelf);
			ClientInventoryUpdateReportItem clientInventoryUpdateReportItem = new ClientInventoryUpdateReportItem
			{
				aetherizedCards = new List<AetherizedCardInformation>(inventoryUpdate.aetherizedCards),
				delta = inventoryUpdate.delta
			};
			clientInventoryUpdateReportItem.delta.cardsAdded = (from x in inventoryUpdate.delta.cardsAdded.ToList()
				where !upgrade.cardsAdded.Contains((uint)x)
				select x).ToArray();
			foreach (uint card in upgrade.cardsAdded)
			{
				AetherizedCardInformation aetherizedCardInformation = clientInventoryUpdateReportItem.aetherizedCards.Find((AetherizedCardInformation a) => a.grpId == card);
				if (aetherizedCardInformation != null)
				{
					clientInventoryUpdateReportItem.aetherizedCards.Remove(aetherizedCardInformation);
				}
			}
			inventoryUpdate = clientInventoryUpdateReportItem;
		}
		ClientInventoryUpdateReportItem clientInventoryUpdateReportItem2 = inventoryUpdate;
		bool? obj;
		if (clientInventoryUpdateReportItem2 == null)
		{
			obj = null;
		}
		else
		{
			InventoryDelta delta = clientInventoryUpdateReportItem2.delta;
			obj = ((delta != null) ? new bool?(!delta.IsEmpty()) : ((bool?)null));
		}
		if (obj ?? true)
		{
			yield return _rewardsPanel.AddAndDisplayRewardsCoroutine(inventoryUpdate, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
			inventoryUpdate = null;
			yield return new WaitUntil(() => !_rewardsPanel.Visible);
		}
		_ongoingOrbTransaction = false;
		if (!_useLargeOrbs.HasValue)
		{
			_useLargeOrbs = false;
		}
		InitializeRewardTreeView(_cardDatabase, _cardViewBuilder, _cardMaterialBuilder, _useLargeOrbs.Value);
		MasteryPassProvider.GetOrbInventoryChangeQueueAndClear(TrackName);
		void OnInventoryUpdated(ClientInventoryUpdateReportItem u)
		{
			inventoryUpdate = u;
		}
	}

	private IEnumerator Coroutine_PlayAnyQueuedUpUnlockingOfColorOrbSlots()
	{
		_ongoingColorOrbTransaction = true;
		Queue<RewardWebChange> queue = MasteryPassProvider.GetRewardWebChangeQueue(TrackName);
		while (queue.Count > 0)
		{
			RewardWebChange rewardWebChange = queue.Dequeue();
			int index = rewardWebChange.ID % 5;
			EPP_OrbSlotView orbView = GetOrbView(rewardWebChange.ID);
			if (rewardWebChange.Transition != RewardWebChange.StateTransition.BecomeUnlocked_ColorOrb)
			{
				continue;
			}
			orbView.SetState(OrbSlot.OrbState.Available);
			GameObject gameObject = _orbsToAnimate[index];
			Vector3 animatingOrbOrigin = _animatingOrbOrigins[gameObject];
			yield return Coroutine_AnimateOrbToUnlockSlot(gameObject, animatingOrbOrigin, orbView, _unlockVfx);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_masterytree_orb_set, base.gameObject);
			UpgradePacket upgradePacket = orbView.OrbSlotModel.serverRewardNode.upgradePacket;
			if (upgradePacket != null)
			{
				_deckUpgrade.DisplayUpgrade(upgradePacket, orbView.Title, _cardDatabase, _cardViewBuilder, _cardMaterialBuilder);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_masterytree_orb_reward, base.gameObject);
				yield return new WaitUntil(() => !_deckUpgrade.gameObject.activeSelf);
			}
		}
		_currentPicks.Clear();
		_currentlySelectedOrbSlot = null;
		this.OnUpdateOrbPicks?.Invoke(_currentPicks.Count);
		SetWebVisualsToModelData();
		_ongoingColorOrbTransaction = false;
	}
}
