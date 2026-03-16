using System;
using System.Collections.Generic;
using AssetLookupTree.Payloads.Prefab;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.Browsers;

public class AssignDamageBrowser : CardBrowserBase, ISortedBrowser
{
	public readonly struct SpinnerData
	{
		public readonly uint InstanceId;

		public readonly uint AssignedDamage;

		public readonly bool IsLethal;

		public readonly bool CanIncrement;

		public readonly bool CanDecrement;

		public SpinnerData(uint instanceId, uint assignedDamage, bool isLethal, bool canIncrement, bool canDecrement)
		{
			InstanceId = instanceId;
			AssignedDamage = assignedDamage;
			IsLethal = isLethal;
			CanIncrement = canIncrement;
			CanDecrement = canDecrement;
		}
	}

	private static readonly float spinnerForwardOffset = 1.25f;

	private const float SPINNER_SCALE_CARD = 0.4f;

	private static readonly Color32 _lethalTextColor = new Color32(254, 176, 0, byte.MaxValue);

	private static readonly Color32 _damagedTextColor = new Color32(0, 254, 254, byte.MaxValue);

	private static readonly Color32 _unassignedTextColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private readonly AssignDamageProvider _assignDamageProvider;

	private readonly Transform _cameraTransform;

	private uint _playerId;

	private readonly Dictionary<uint, DuelScene_CDC> _idToCardMap = new Dictionary<uint, DuelScene_CDC>();

	private readonly Dictionary<uint, SpinnerAnimated> _idToSpinnerMap = new Dictionary<uint, SpinnerAnimated>();

	private readonly Dictionary<Transform, uint> _movingTransformsToIds = new Dictionary<Transform, uint>();

	private bool _spinnerTextColorsEnabled = true;

	private AssignDamagePlayerWidget _playerWidget;

	private AssignDamageBrowserLayout _layout;

	private Scrollbar _blockerScrollBar;

	private Toggle _autoAssignLethalToggle;

	private const string UnusedSpinnerName = "Unused Spinner";

	public event Action<uint, int> CardDragCompleted;

	public event Action<uint> AssignmentIncreased;

	public event Action<uint> AssignmentDecreased;

	public event Action<bool> AutoAssignLethalToggled;

	public event Action DoneAction;

	public event Action UndoAction;

	public AssignDamageBrowser(Transform cameraTransform, AssignDamageProvider provider, BrowserManager browserManager, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		_cameraTransform = cameraTransform;
		_assignDamageProvider = provider;
	}

	public override void Init()
	{
		base.Init();
		cardHolder.OnCardHolderUpdated += LayoutAllSpinners;
		_splineMovementSystem.MovementCompleted += OnSplineMovementCompleted;
		_movingTransformsToIds.Clear();
	}

	public override void OnKeyUp(KeyCode keyCode)
	{
		if (keyCode == KeyCode.Z)
		{
			this.UndoAction?.Invoke();
		}
		base.OnKeyUp(keyCode);
	}

	public override bool AllowsDragInteractions(DuelScene_CDC cardView)
	{
		return _idToSpinnerMap.ContainsKey(cardView.InstanceId);
	}

	public override void HandleDrag(DuelScene_CDC draggedCard)
	{
		_movingTransformsToIds.TryAdd(draggedCard.Root, draggedCard.InstanceId);
		UpdateSpinner(draggedCard);
	}

	public override void OnDragRelease(DuelScene_CDC draggedCard)
	{
		int indexForCard = cardHolder.GetIndexForCard(draggedCard);
		indexForCard--;
		_layout.ReorderBlocker(draggedCard, indexForCard);
		this.CardDragCompleted?.Invoke(draggedCard.InstanceId, indexForCard);
		_movingTransformsToIds.Remove(draggedCard.Root);
		LayoutAllSpinners();
	}

	protected override ICardLayout GetCardHolderLayout()
	{
		return _layout;
	}

	protected override void SetupCards()
	{
	}

	public void SetAttackerCard(DuelScene_CDC attacker)
	{
		_layout.SetAttacker(attacker);
		_idToCardMap[attacker.InstanceId] = attacker;
		cardViews.Add(attacker);
		cardHolder.SwapIgnoredCards.Add(attacker);
		MoveCardToBrowser(attacker);
	}

	public void SetBlockerCards(IReadOnlyList<DuelScene_CDC> blockers)
	{
		_layout.SetBlockers(blockers);
		foreach (DuelScene_CDC blocker in blockers)
		{
			_idToCardMap[blocker.InstanceId] = blocker;
			cardViews.Add(blocker);
		}
		MoveCardViewsToBrowser(blockers);
		if (!PlatformUtils.IsHandheld())
		{
			_layout.SetFrontWidth((blockers.Count > 2) ? 9.5f : 6.5f);
		}
		_blockerScrollBar.value = 0f;
		_blockerScrollBar.gameObject.SetActive(blockers.Count > _layout.FrontCount);
		ScrollWheelInputForScrollbar component = GetBrowserElement("Scrollbar").GetComponent<ScrollWheelInputForScrollbar>();
		component.ElementCount = blockers.Count;
		component.ElementsFeatured = _layout.FrontCount;
	}

	public void SetAttackQuarryCard(DuelScene_CDC attackQuarry)
	{
		_layout.SetAttackQuarry(attackQuarry);
		cardHolder.SwapIgnoredCards.Add(attackQuarry);
		MoveCardToBrowser(attackQuarry);
		_idToCardMap[attackQuarry.InstanceId] = attackQuarry;
	}

	public void SetAttackQuarryPlayer(uint playerId, Sprite playerSprite, int lifeTotal)
	{
		_playerId = playerId;
		_playerWidget.gameObject.UpdateActive(active: true);
		_playerWidget.SetAvatarSprite(playerSprite);
		_playerWidget.SetLifeTotal(lifeTotal);
	}

	public void LayoutCardHolder()
	{
		cardHolder.LayoutNow();
	}

	protected override void OnScroll(float scrollValue)
	{
		_layout.ScrollValue = scrollValue;
		cardHolder.LayoutNow();
		LayoutAllSpinners();
	}

	protected override void InitializeUIElements()
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(_assignDamageProvider.Header);
		component.SetSubheaderText(_assignDamageProvider.SubHeader);
		OrderIndicator component2 = GetBrowserElement("OrderIndicator").GetComponent<OrderIndicator>();
		component2.SetText(_assignDamageProvider.OrderIndicatorText_First, _assignDamageProvider.OrderIndicatorText_Last);
		component2.SetArrowDirection(OrderIndicator.ArrowDirection.Right);
		_playerWidget = GetBrowserElement("PlayerWidget").GetComponent<AssignDamagePlayerWidget>();
		_playerWidget.gameObject.UpdateActive(active: false);
		_blockerScrollBar = GetBrowserElement("Scrollbar").GetComponent<Scrollbar>();
		_blockerScrollBar.onValueChanged.RemoveAllListeners();
		_blockerScrollBar.onValueChanged.AddListener(OnScroll);
		_autoAssignLethalToggle = GetBrowserElement("AutoAssignLethalToggle").GetComponent<Toggle>();
		_autoAssignLethalToggle.SetIsOnWithoutNotify(value: true);
		_autoAssignLethalToggle.onValueChanged.AddListener(OnAssignLethalToggleValueChanged);
		GameObject browserElement = GetBrowserElement("CardGroupAMarker");
		GameObject browserElement2 = GetBrowserElement("CardGroupBMarker");
		GameObject browserElement3 = GetBrowserElement("CardGroupCMarker");
		_layout = new AssignDamageBrowserLayout((browserElement != null) ? browserElement.transform.localPosition : new Vector3(-9f, 0f, 0f), (browserElement2 != null) ? browserElement2.transform.localPosition : Vector3.zero, (browserElement3 != null) ? browserElement3.transform.localPosition : new Vector3(9f, 0f, 0f), new CardLayout_ScrollableBrowser(GetCardHolderLayoutData("AssignDamage"))
		{
			ScrollPosition = 0f
		});
		base.InitializeUIElements();
	}

	public void SetAutoAssignLegalToggleActive(bool active)
	{
		_autoAssignLethalToggle.gameObject.SetActive(active);
	}

	public void SetButtons(Dictionary<string, ButtonStateData> buttons)
	{
		base.UpdateButtons(buttons);
	}

	public void SetBrowserCardVFX()
	{
		SetupCardFX();
		foreach (DuelScene_CDC cardView in cardViews)
		{
			if (_cardVFX.TryGetValue(cardView, out var value))
			{
				PlayCardVFX(cardView, value);
			}
		}
	}

	protected override void UpdateButtons(Dictionary<string, ButtonStateData> buttonStateData)
	{
	}

	protected override void OnButtonCallback(string buttonKey)
	{
		if (!(buttonKey != "DoneButton"))
		{
			this.DoneAction?.Invoke();
		}
	}

	protected override void ReleaseUIElements()
	{
		if (_autoAssignLethalToggle != null)
		{
			_autoAssignLethalToggle.onValueChanged.RemoveListener(OnAssignLethalToggleValueChanged);
			_autoAssignLethalToggle.SetIsOnWithoutNotify(value: true);
			_autoAssignLethalToggle = null;
		}
		foreach (KeyValuePair<uint, SpinnerAnimated> item in _idToSpinnerMap)
		{
			SpinnerAnimated value = item.Value;
			value.name = "Unused Spinner";
			value.InstanceId = 0u;
			value.ValueChanged -= Spinner_ValueChanged;
			value.SetTextColor(Color.white);
			value.SetButtonsActive(active: true);
		}
		_idToSpinnerMap.Clear();
		base.ReleaseUIElements();
	}

	public void SetSpinnerValues(IEnumerable<SpinnerData> spinnerDatas)
	{
		foreach (SpinnerData spinnerData in spinnerDatas)
		{
			if (_idToSpinnerMap.TryGetValue(spinnerData.InstanceId, out var value))
			{
				int assignedDamage = (int)spinnerData.AssignedDamage;
				value.MaxValue = (spinnerData.CanIncrement ? (assignedDamage + 1) : assignedDamage);
				value.UseMax = true;
				value.MinValue = (spinnerData.CanDecrement ? (assignedDamage - 1) : assignedDamage);
				value.UseMin = true;
				value.SetTextColor(GetColorForAssignment(spinnerData));
				value.InitValue(assignedDamage);
			}
		}
	}

	public void SetSpinnerButtonsActive(bool active)
	{
		foreach (KeyValuePair<uint, SpinnerAnimated> item in _idToSpinnerMap)
		{
			item.Value.SetButtonsActive(active);
		}
	}

	public void SetSpinnerColorsEnabled(bool isEnabled)
	{
		_spinnerTextColorsEnabled = isEnabled;
	}

	private Color GetColorForAssignment(SpinnerData assignment)
	{
		if (!_spinnerTextColorsEnabled)
		{
			return _unassignedTextColor;
		}
		if (assignment.IsLethal)
		{
			return _lethalTextColor;
		}
		if (assignment.AssignedDamage != 0)
		{
			return _damagedTextColor;
		}
		return _unassignedTextColor;
	}

	public void InitializeSpinners(IReadOnlyList<uint> instanceIds)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CardBrowserElementID = "AssignDamageSpinner";
		BrowserElementPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<BrowserElementPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
		if (payload == null || payload.PrefabPath == null)
		{
			Debug.LogErrorFormat("No spinner prefab found with browser element key '{0}'", _assetLookupSystem.Blackboard.CardBrowserElementID);
			return;
		}
		foreach (uint instanceId in instanceIds)
		{
			SpinnerAnimated component = _unityObjectPool.PopObject(payload.PrefabPath).GetComponent<SpinnerAnimated>();
			component.name = "Spinner for # " + instanceId;
			component.InstanceId = instanceId;
			component.ValueChanged += Spinner_ValueChanged;
			Transform transform = component.transform;
			transform.parent = BrowserManager.WorkflowBrowserRoot;
			transform.ZeroOut();
			transform.localScale = Vector3.one;
			_idToSpinnerMap[instanceId] = component;
			AddBrowserElement(component.name, component.gameObject, canHide: true);
		}
		cardHolder.LayoutNow();
	}

	public void LayoutMovingSpinners()
	{
		LayoutSpinners(updateAll: false);
	}

	private void LayoutAllSpinners()
	{
		LayoutSpinners(updateAll: true);
	}

	private void Spinner_ValueChanged(object sender, ValueChangedEventArgs e)
	{
		int num = e.NewValue - e.OldValue;
		if (num != 0)
		{
			((num > 0) ? this.AssignmentIncreased : this.AssignmentDecreased)?.Invoke(e.AssociatedInstanceId);
		}
	}

	private void OnSplineMovementCompleted(Transform tf)
	{
		if (_movingTransformsToIds.Remove(tf, out var value))
		{
			UpdateSpinner(_idToCardMap[value]);
		}
	}

	private bool IsMovingViaSpline(Transform query)
	{
		return !Mathf.Approximately(1f, _splineMovementSystem.GetProgress(query));
	}

	private bool IsMoving(Transform root)
	{
		if (!_movingTransformsToIds.ContainsKey(root))
		{
			return IsMovingViaSpline(root);
		}
		return true;
	}

	private void UpdateSpinner(DuelScene_CDC cardView)
	{
		if (!_movingTransformsToIds.ContainsKey(cardView.Root) && IsMovingViaSpline(cardView.Root))
		{
			_movingTransformsToIds.Add(cardView.Root, cardView.InstanceId);
		}
		SpinnerAnimated spinnerAnimated = _idToSpinnerMap[cardView.InstanceId];
		Transform transform = spinnerAnimated.transform;
		Vector3 position = cardView.Root.position;
		Vector3 normalized = (_cameraTransform.position - position).normalized;
		Vector3 position2 = position + normalized * spinnerForwardOffset;
		transform.position = position2;
		transform.localScale = 0.4f * Vector3.one;
		spinnerAnimated.gameObject.UpdateActive(_layout.CardIsInView(cardView));
	}

	private void LayoutSpinners(bool updateAll)
	{
		foreach (KeyValuePair<uint, SpinnerAnimated> item in _idToSpinnerMap)
		{
			uint key = item.Key;
			SpinnerAnimated value = item.Value;
			GameObject gameObject = value.gameObject;
			if (!base.IsVisible)
			{
				gameObject.UpdateActive(active: false);
				continue;
			}
			Transform transform = value.transform;
			DuelScene_CDC value2;
			if (key == _playerId)
			{
				gameObject.UpdateActive(active: true);
				Transform transform2 = _playerWidget.transform;
				transform.SetParent(transform2);
				transform.localPosition = Vector3.zero + _playerWidget.SpinnerOffset;
				transform.localScale = Vector3.one;
			}
			else if (_idToCardMap.TryGetValue(key, out value2) && (updateAll || IsMoving(value2.Root)))
			{
				UpdateSpinner(value2);
			}
		}
	}

	private void OnAssignLethalToggleValueChanged(bool newValue)
	{
		this.AutoAssignLethalToggled?.Invoke(newValue);
	}

	protected override void ReleaseCards()
	{
		base.ReleaseCards();
		cardHolder.SwapIgnoredCards.Clear();
	}

	public override void Close()
	{
		cardHolder.OnCardHolderUpdated -= LayoutAllSpinners;
		_splineMovementSystem.MovementCompleted -= OnSplineMovementCompleted;
		base.Close();
		_layout?.Dispose();
		_layout = null;
		this.CardDragCompleted = null;
		this.AssignmentIncreased = null;
		this.AssignmentDecreased = null;
		this.AutoAssignLethalToggled = null;
		this.UndoAction = null;
		this.DoneAction = null;
		_idToCardMap.Clear();
		_movingTransformsToIds.Clear();
	}

	public void Sort(List<DuelScene_CDC> toSort)
	{
		toSort.Sort(_layout.SortCompare);
	}
}
