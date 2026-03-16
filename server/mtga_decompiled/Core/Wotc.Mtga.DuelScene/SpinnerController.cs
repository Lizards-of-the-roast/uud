using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Distribution;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SpinnerController : IUpdate, IDisposable
{
	private readonly Vector3 CardOffset = new Vector3(0f, 0.25f, 0f);

	private readonly Vector3 BehindCardOffset = new Vector3(-2f, 0.25f, 0f);

	private readonly Vector3 BrowserCardOffset = new Vector3(-0.75f, 0f, -0.15f);

	private readonly Vector3 StackCardOffset = new Vector3(-0.15f, 0f, -0.15f);

	private const float SPINNER_SCALE_BATTLEFIELD = 0.4f;

	private const float SPINNER_SCALE_BATTLEFIELD_BATTLE = 0.3f;

	private const float SPINNER_SCALE_STACK = 0.25f;

	private readonly IObjectPool _genericPool;

	private readonly IUnityObjectPool _unityObjPool;

	private readonly Camera _camera;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IEntityViewManager _viewManager;

	private readonly Transform _root;

	private readonly AssetLookupSystem _assetLookupSystem;

	private Dictionary<uint, SpinnerAnimated> _activeSpinners;

	private Dictionary<uint, Widget_TextBox> _workflowTextBoxes;

	private List<Widget_TextBox> _distributionTextBoxes;

	private List<uint> _spinnerIds;

	private List<uint> _stackedSpinnerIds;

	private Transform _browserRoot;

	private CardHolderReference<StackCardHolder> _stack;

	private CardHolderReference<IBattlefieldCardHolder> _battlefield;

	public bool Active
	{
		get
		{
			if (_activeSpinners.Count <= 0)
			{
				return _workflowTextBoxes.Count > 0;
			}
			return true;
		}
	}

	public event Action<uint, uint> ValueChanged;

	public SpinnerController(IObjectPool genericPool, IUnityObjectPool unityPool, Transform root, Camera camera, IGameStateProvider gameStateProvider, IEntityViewManager viewManager, ICardHolderProvider cardHolderProvider, AssetLookupSystem assetLookupSystem)
	{
		_genericPool = genericPool ?? NullObjectPool.Default;
		_unityObjPool = unityPool;
		_root = root;
		_camera = camera;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_viewManager = viewManager ?? NullEntityViewManager.Default;
		_assetLookupSystem = assetLookupSystem;
		_activeSpinners = _genericPool.PopObject<Dictionary<uint, SpinnerAnimated>>();
		_workflowTextBoxes = _genericPool.PopObject<Dictionary<uint, Widget_TextBox>>();
		_distributionTextBoxes = _genericPool.PopObject<List<Widget_TextBox>>();
		_spinnerIds = _genericPool.PopObject<List<uint>>();
		_stackedSpinnerIds = _genericPool.PopObject<List<uint>>();
		_stack = CardHolderReference<StackCardHolder>.Stack(cardHolderProvider);
		_battlefield = CardHolderReference<IBattlefieldCardHolder>.Battlefield(cardHolderProvider);
	}

	public void SetBrowserRoot(Transform root)
	{
		_browserRoot = root;
	}

	public void Open(IReadOnlyCollection<SpinnerData> data)
	{
		_spinnerIds.Clear();
		_stackedSpinnerIds.Clear();
		foreach (SpinnerData datum in data)
		{
			_spinnerIds.Add(datum.InstanceId);
		}
		EnsureSpinners(_spinnerIds);
		PositionWidgets();
		foreach (SpinnerData datum2 in data)
		{
			uint instanceId = datum2.InstanceId;
			SpinnerAnimated spinnerAnimated = _activeSpinners[instanceId];
			spinnerAnimated.MinValue = datum2.Min;
			spinnerAnimated.MaxValue = datum2.Max;
			spinnerAnimated.InstanceId = instanceId;
			spinnerAnimated.name = "Spinner for #" + instanceId;
			spinnerAnimated.UseMin = true;
			spinnerAnimated.UseMax = true;
			spinnerAnimated.InitValue(datum2.Amount);
		}
	}

	private Widget_TextBox CreateTextBox()
	{
		_assetLookupSystem.Blackboard.Clear();
		TextWidgetPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<TextWidgetPrefab>().GetPayload(_assetLookupSystem.Blackboard);
		Widget_TextBox component = _unityObjPool.PopObject(payload.TextWidgetRef.RelativePath).GetComponent<Widget_TextBox>();
		component.transform.SetParent(_root);
		component.transform.ZeroOut();
		return component;
	}

	public Widget_TextBox CreateWorkflowTextBox(uint id)
	{
		if (!_workflowTextBoxes.TryGetValue(id, out var value))
		{
			value = CreateTextBox();
			_workflowTextBoxes[id] = value;
		}
		return value;
	}

	public void Close()
	{
		this.ValueChanged = null;
		if (_activeSpinners.Count != 0)
		{
			EnsureSpinners(Array.Empty<uint>());
		}
		if (_workflowTextBoxes.Count == 0)
		{
			return;
		}
		foreach (uint key in _workflowTextBoxes.Keys)
		{
			_unityObjPool.PushObject(_workflowTextBoxes[key].gameObject);
		}
		_workflowTextBoxes.Clear();
	}

	public void OnUpdate(float time)
	{
		if (_stack == null)
		{
			return;
		}
		DuelScene_CDC duelScene_CDC = _stack.Get()?.GetTopCardOnStack();
		DuelScene_CDC hoveredCard = CardHoverController.HoveredCard;
		DuelScene_CDC duelScene_CDC2 = (HoveredCardIsOnStack(hoveredCard) ? hoveredCard : duelScene_CDC);
		if (duelScene_CDC2 != null)
		{
			uint instanceId = duelScene_CDC2.InstanceId;
			List<TargetSpec> list = ((MtgGameState)_gameStateProvider.CurrentGameState).TargetInfo.FindAll((TargetSpec x) => x.Affector == instanceId && x.Distributions.Count > 0);
			int num = 0;
			IEntityView entityView;
			foreach (TargetSpec item in list)
			{
				for (int num2 = 0; num2 < item.Distributions.Count; num2++)
				{
					if (_viewManager.TryGetEntity(item.Affected[num2], out entityView))
					{
						num++;
					}
				}
			}
			while (_distributionTextBoxes.Count > num)
			{
				_unityObjPool.PushObject(_distributionTextBoxes[0].gameObject);
				_distributionTextBoxes.RemoveAt(0);
			}
			int num3 = 0;
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				for (int num5 = 0; num5 < list[num4].Distributions.Count; num5++)
				{
					if (!_viewManager.TryGetEntity(list[num4].Affected[num5], out entityView))
					{
						num3++;
						continue;
					}
					int num6 = num4 + num5 - num3;
					int value = list[num4].Distributions[num5];
					Widget_TextBox widget_TextBox = null;
					if (num6 < _distributionTextBoxes.Count)
					{
						widget_TextBox = _distributionTextBoxes[num6];
					}
					else
					{
						widget_TextBox = CreateTextBox();
						_distributionTextBoxes.Add(widget_TextBox);
					}
					widget_TextBox.SetValue(value);
					SetWidgetTranform(list[num4].Affected[num5], widget_TextBox.transform);
				}
			}
		}
		else if (_distributionTextBoxes.Count > 0)
		{
			while (_distributionTextBoxes.Count > 0)
			{
				_unityObjPool.PushObject(_distributionTextBoxes[0].gameObject);
				_distributionTextBoxes.RemoveAt(0);
			}
		}
		PositionWidgets();
		StackSpinners();
	}

	private bool HoveredCardIsOnStack(DuelScene_CDC hoveredCard)
	{
		if (hoveredCard == null)
		{
			return false;
		}
		StackCardHolder stackCardHolder = _stack.Get();
		if (stackCardHolder == null)
		{
			return false;
		}
		return stackCardHolder.CardViews.Contains(hoveredCard);
	}

	private void onSpinnerValueChanged(object sender, ValueChangedEventArgs args)
	{
		this.ValueChanged?.Invoke(args.AssociatedInstanceId, (uint)args.NewValue);
	}

	private void EnsureSpinners(IEnumerable<uint> newTargets)
	{
		IEnumerable<uint> enumerable = _activeSpinners.Keys.ToList().Except(newTargets);
		IEnumerable<uint> enumerable2 = newTargets.Except(_activeSpinners.Keys);
		foreach (uint item in enumerable)
		{
			SpinnerAnimated spinnerAnimated = _activeSpinners[item];
			GameObject gameObject = spinnerAnimated.gameObject;
			if ((bool)gameObject)
			{
				gameObject.UpdateActive(active: true);
			}
			_activeSpinners.Remove(item);
			spinnerAnimated.ValueChanged -= onSpinnerValueChanged;
			spinnerAnimated.InstanceId = 0u;
			int minValue = (spinnerAnimated.MaxValue = 0);
			spinnerAnimated.MinValue = minValue;
			_unityObjPool.PushObject(spinnerAnimated.gameObject);
		}
		_assetLookupSystem.Blackboard.Clear();
		SpinnerPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<SpinnerPrefab>().GetPayload(_assetLookupSystem.Blackboard);
		foreach (uint item2 in enumerable2)
		{
			SpinnerAnimated component = _unityObjPool.PopObject(payload.SpinnerRef.RelativePath).GetComponent<SpinnerAnimated>();
			SetSpinnerParent(component, item2);
			component.transform.ZeroOut();
			component.ValueChanged += onSpinnerValueChanged;
			_activeSpinners[item2] = component;
		}
	}

	private void SetSpinnerParent(SpinnerAnimated spinner, uint id)
	{
		if (_viewManager.TryGetCardView(id, out var cardView) && cardView.CurrentCardHolder.CardHolderType == CardHolderType.CardBrowserDefault)
		{
			spinner.transform.SetParent(_browserRoot);
		}
		else
		{
			spinner.transform.SetParent(_root);
		}
	}

	private void StackSpinners()
	{
		foreach (uint spinnerId in _spinnerIds)
		{
			SetupStackedSpinners(spinnerId);
			if (_activeSpinners.TryGetValue(spinnerId, out var value) && (bool)value)
			{
				GameObject gameObject = value.gameObject;
				if ((bool)gameObject)
				{
					gameObject.UpdateActive(SpinnerShouldBeActive(spinnerId));
				}
			}
		}
	}

	private bool SetupStackedSpinners(uint id)
	{
		if (!_viewManager.TryGetCardView(id, out var cardView))
		{
			return false;
		}
		IBattlefieldStack battlefieldStack = _battlefield.Get()?.GetStackForCard(cardView);
		if (battlefieldStack == null)
		{
			return false;
		}
		if (battlefieldStack.StackParent != cardView && battlefieldStack.StackedCards.Count == 1 && battlefieldStack.HasAttachmentOrExile)
		{
			_stackedSpinnerIds.Add(id);
			return true;
		}
		return false;
	}

	private bool SpinnerShouldBeActive(uint id)
	{
		if (_viewManager.TryGetAvatarById(id, out var _))
		{
			return true;
		}
		if (!_viewManager.TryGetCardView(id, out var cardView))
		{
			return false;
		}
		if (cardView.CurrentCardHolder.CardHolderType == CardHolderType.CardBrowserDefault)
		{
			return true;
		}
		IBattlefieldStack battlefieldStack = _battlefield.Get()?.GetStackForCard(cardView);
		if (battlefieldStack == null)
		{
			return false;
		}
		if (battlefieldStack.StackParent != cardView && battlefieldStack.StackedCards.Count == 1 && cardView.VisualModel.Instance.AttachedToId != 0)
		{
			return true;
		}
		if (cardView.IsVisible)
		{
			return battlefieldStack.StackParent == cardView;
		}
		return false;
	}

	private void PositionWidgets()
	{
		foreach (KeyValuePair<uint, SpinnerAnimated> activeSpinner in _activeSpinners)
		{
			Transform transform = activeSpinner.Value.transform;
			SetWidgetTranform(activeSpinner.Key, transform);
		}
		foreach (KeyValuePair<uint, Widget_TextBox> workflowTextBox in _workflowTextBoxes)
		{
			Transform transform2 = workflowTextBox.Value.transform;
			SetWidgetTranform(workflowTextBox.Key, transform2);
		}
	}

	private void SetWidgetTranform(uint id, Transform tranform)
	{
		Vector3 position = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		float num = 0.4f;
		Vector3 vector = Vector3.zero;
		bool flag = false;
		DuelScene_CDC cardView;
		if (_viewManager.TryGetAvatarById(id, out var avatar))
		{
			rotation = avatar.transform.rotation;
			position = avatar.LifeTextTransform.position;
			vector = AvatarOffset(avatar.IsLocalPlayer);
		}
		else if (_viewManager.TryGetCardView(id, out cardView))
		{
			rotation = cardView.Root.rotation;
			position = cardView.Root.position;
			if (cardView.Model.ZoneType == ZoneType.Stack)
			{
				vector = StackCardOffset;
				num = 0.25f;
				flag = true;
			}
			else if (cardView.CurrentCardHolder.CardHolderType == CardHolderType.CardBrowserDefault)
			{
				vector = BrowserCardOffset;
				num = 0.25f;
				flag = true;
			}
			else if (_stackedSpinnerIds.Contains(id))
			{
				vector = BehindCardOffset;
			}
			else if (cardView.Model.ZoneType == ZoneType.Battlefield && cardView.Model.CardTypes.Contains(CardType.Battle))
			{
				num = 0.3f;
				vector = CardOffset;
			}
			else
			{
				vector = CardOffset;
			}
		}
		tranform.rotation = rotation;
		position += tranform.forward * vector.z;
		position += tranform.up * vector.y;
		position += tranform.right * vector.x;
		if (!flag)
		{
			Vector3 vector2 = _camera.WorldToScreenPoint(position);
			float magnitude = (_camera.transform.position - _root.position).magnitude;
			position = _camera.ScreenToWorldPoint(new Vector3(vector2.x, vector2.y, magnitude));
		}
		tranform.position = position;
		tranform.localScale = Vector3.one * num;
	}

	private Vector3 AvatarOffset(bool isLocalPlayer)
	{
		if (PlatformUtils.IsHandheld())
		{
			if (!isLocalPlayer)
			{
				return new Vector3(4f, -3.5f, 0f);
			}
			return new Vector3(3f, 3.5f, 0f);
		}
		return new Vector3(2.5f, 1.75f, 0f);
	}

	public void Dispose()
	{
		_browserRoot = null;
		_stack = null;
		_battlefield = null;
		foreach (KeyValuePair<uint, SpinnerAnimated> activeSpinner in _activeSpinners)
		{
			SpinnerAnimated value = activeSpinner.Value;
			value.ValueChanged -= onSpinnerValueChanged;
			value.InstanceId = 0u;
			int minValue = (value.MaxValue = 0);
			value.MinValue = minValue;
			uint key = activeSpinner.Key;
			GameObject gameObject = value.gameObject;
			if ((bool)gameObject)
			{
				gameObject.UpdateActive(active: true);
				_unityObjPool.PushObject(gameObject);
			}
			_activeSpinners[key] = null;
		}
		_activeSpinners.Clear();
		_genericPool.PushObject(_activeSpinners);
		foreach (KeyValuePair<uint, Widget_TextBox> workflowTextBox in _workflowTextBoxes)
		{
			uint key2 = workflowTextBox.Key;
			_unityObjPool.PushObject(workflowTextBox.Value.gameObject);
			_workflowTextBoxes[key2] = null;
		}
		_workflowTextBoxes.Clear();
		_genericPool.PushObject(_workflowTextBoxes);
		while (_distributionTextBoxes.Count > 0)
		{
			Widget_TextBox widget_TextBox = _distributionTextBoxes[0];
			_genericPool.PushObject(widget_TextBox.gameObject);
			_distributionTextBoxes.Remove(widget_TextBox);
		}
		_genericPool.PushObject(_distributionTextBoxes);
		_spinnerIds.Clear();
		_genericPool.PushObject(_spinnerIds);
		_stackedSpinnerIds.Clear();
		_genericPool.PushObject(_stackedSpinnerIds);
	}
}
