using System;
using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;
using Wotc.Mtga.DuelScene.UI;
using Wotc.Mtgo.Gre.External.Messaging;

public class ManaColorSelector : MonoBehaviour
{
	public struct ManaProducedData
	{
		public ManaColor PrimaryColor;

		public uint CountOfColor;
	}

	public struct ManaColorSelectorConfig
	{
		public ICardDataAdapter Model;

		public bool CanCancel;

		public string PromptText;

		public bool OnTheStack;

		public StackCardHolder Stack;

		public bool CanHoverCards;

		public ManaColorSelectorConfig(ICardDataAdapter model, bool canCancel, string promptText, StackCardHolder stack, bool canHoverCards = false)
		{
			Model = model;
			CanCancel = canCancel;
			PromptText = promptText;
			OnTheStack = false;
			Stack = stack;
			CanHoverCards = canHoverCards;
		}

		public ManaColorSelectorConfig(ICardDataAdapter model = null)
			: this(model, canCancel: true, null, null)
		{
		}

		public ManaColorSelectorConfig(bool canCancel, string promptText)
			: this(null, canCancel, promptText, null)
		{
		}

		public void Cleanup()
		{
			Model = null;
			CanCancel = true;
			PromptText = null;
			OnTheStack = false;
			Stack = null;
		}
	}

	[SerializeField]
	private CanvasGroup OverlayCanvas;

	[SerializeField]
	private TextMeshProUGUI PromptLabel;

	[SerializeField]
	private CanvasGroup PromptCanvas;

	[SerializeField]
	private bool _promptEnabled;

	[SerializeField]
	private Vector3 _stackPositionOffset = Vector3.zero;

	protected RectTransform _parentRect;

	protected RectTransform _overlayRect;

	protected ManaColorSelectorConfig _config;

	protected IManaSelectorProvider _selectionProvider;

	private Action<IReadOnlyCollection<ManaColor>> _callback;

	private Transform _followTransform;

	private ManaColor _previouslyHovered;

	protected Camera _camera;

	protected IUnityObjectPool _objectPool;

	protected IObjectPool _genericPool;

	protected AssetLookupSystem _assetLookupSystem;

	public bool IsOpen { get; private set; }

	public bool CanHoverCards => _config.CanHoverCards;

	public void Init(Camera camera, IUnityObjectPool objectPool, IObjectPool genericPool, AssetLookupSystem assetLookupSystem)
	{
		_camera = camera;
		_objectPool = objectPool;
		_genericPool = genericPool;
		_assetLookupSystem = assetLookupSystem;
	}

	protected virtual void Start()
	{
		OverlayCanvas.alpha = 0f;
		OverlayCanvas.blocksRaycasts = false;
		OverlayCanvas.interactable = false;
		_parentRect = base.gameObject.GetComponent<RectTransform>();
		_overlayRect = OverlayCanvas.gameObject.GetComponent<RectTransform>();
	}

	private void Cancel()
	{
		if (_callback != null)
		{
			_callback(null);
		}
		CloseSelector();
		_previouslyHovered = ManaColor.None;
	}

	private void Update()
	{
		if (!IsOpen)
		{
			return;
		}
		if (_followTransform != null)
		{
			Vector2 screenPos = _camera.WorldToScreenPoint(_followTransform.position);
			MoveToScreenPoint(screenPos);
		}
		Vector2 screenPoint = Input.mousePosition;
		if (RectTransformUtility.RectangleContainsScreenPoint(_overlayRect, screenPoint, _camera))
		{
			if (_config.CanCancel && Input.GetMouseButtonDown(1))
			{
				Cancel();
			}
			else
			{
				InWheelUpdate();
			}
		}
		else if (_config.CanCancel && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
		{
			Cancel();
		}
		else
		{
			OutOfWheelUpdate();
		}
	}

	protected virtual void InWheelUpdate()
	{
	}

	protected virtual void OutOfWheelUpdate()
	{
	}

	protected void SelectColor(ManaColor color)
	{
		_selectionProvider.Select(color);
		if (_selectionProvider.AllSelectionsComplete)
		{
			_callback?.Invoke(_selectionProvider.SelectedColors);
			CloseSelector();
		}
		else
		{
			Setup();
		}
	}

	public void OpenSelector(ManaSelectionFlow flow, Transform followTransform, ManaColorSelectorConfig config, Action<IReadOnlyCollection<ManaColor>> callback)
	{
		IManaSelectorProvider provider = new ManaSelectionFlowProvider(flow, config.Model);
		OpenSelector(provider, followTransform, config, callback);
	}

	public void OpenSelector(IReadOnlyList<ManaColor> options, uint maxSelection, SelectionValidationType validationType, DuelScene_CDC topOfStack, ManaColorSelectorConfig config, Action<IReadOnlyCollection<ManaColor>> callback)
	{
		IManaSelectorProvider provider = new ManaColorListProvider(options, maxSelection, validationType, config.Model);
		Vector3 position = topOfStack.Root.position + topOfStack.Root.up * (0f - topOfStack.Collider.size.y);
		Vector3 vector = _camera.WorldToScreenPoint(position);
		vector += _stackPositionOffset;
		config.OnTheStack = true;
		OpenSelector(provider, vector, config, callback);
	}

	public void OpenSelector(IReadOnlyList<ManaColor> options, uint maxSelection, SelectionValidationType validationType, Transform followTransform, ManaColorSelectorConfig config, Action<IReadOnlyCollection<ManaColor>> callback)
	{
		IManaSelectorProvider provider = new ManaColorListProvider(options, maxSelection, validationType, config.Model);
		OpenSelector(provider, followTransform, config, callback);
	}

	public void OpenSelector(IReadOnlyList<ManaColor> options, uint maxSelection, SelectionValidationType validationType, Vector2 screenPosition, ManaColorSelectorConfig config, Action<IReadOnlyCollection<ManaColor>> callback)
	{
		IManaSelectorProvider provider = new ManaColorListProvider(options, maxSelection, validationType, config.Model);
		OpenSelector(provider, screenPosition, config, callback);
	}

	private void OpenSelector(IManaSelectorProvider provider, Transform transform, ManaColorSelectorConfig config, Action<IReadOnlyCollection<ManaColor>> callback)
	{
		Vector2 screenPosition = _camera.WorldToScreenPoint(transform.position);
		OpenSelector(provider, screenPosition, config, callback);
		_followTransform = transform;
	}

	protected virtual void OpenSelector(IManaSelectorProvider provider, Vector2 screenPosition, ManaColorSelectorConfig config, Action<IReadOnlyCollection<ManaColor>> callback)
	{
		Cleanup();
		IsOpen = true;
		_config = config;
		_callback = callback;
		_selectionProvider = provider;
		MoveToScreenPoint(screenPosition);
		AudioManager.PlayAudio(WwiseEvents.sfx_combat_manawheel_open, AudioManager.Default);
		Setup();
	}

	private void MoveToScreenPoint(Vector2 screenPos)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, screenPos, _camera, out var localPoint);
		float num = _overlayRect.rect.width * 0.5f;
		float num2 = _overlayRect.rect.height * 0.5f;
		DuelScene_CDC topCard = null;
		if (_config.Stack != null && _config.Stack.TryGetTopCardOnStack(out topCard) && !_config.OnTheStack)
		{
			Vector3 position = topCard.Root.position + -topCard.Root.right * topCard.Collider.size.x;
			Vector2 vector = new Vector2(num, num2) + localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(screenPoint: _camera.WorldToScreenPoint(position), rect: _parentRect, cam: _camera, localPoint: out var localPoint2);
			if (vector.x > localPoint2.x)
			{
				localPoint.x = localPoint2.x - num;
			}
		}
		float min = _parentRect.rect.xMin + num;
		float max = _parentRect.rect.xMax - num;
		float min2 = _parentRect.rect.yMin + num2;
		float max2 = _parentRect.rect.yMax - num2;
		localPoint.x = Mathf.Clamp(localPoint.x, min, max);
		localPoint.y = Mathf.Clamp(localPoint.y, min2, max2);
		_overlayRect.localPosition = localPoint;
	}

	public virtual void CloseSelector()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_combat_manawheel_close, AudioManager.Default);
		Cleanup();
		Setup();
	}

	public void TryCloseSelector()
	{
		if (_config.CanCancel)
		{
			if (_callback != null)
			{
				_callback(null);
			}
			CloseSelector();
		}
	}

	protected virtual void Cleanup()
	{
		IsOpen = false;
		_config.Cleanup();
		_callback = null;
		BaseRaycaster baseRaycaster = ((_camera == null) ? null : _camera.GetComponent<BaseRaycaster>());
		if ((bool)baseRaycaster)
		{
			baseRaycaster.enabled = true;
		}
		_selectionProvider?.Cleanup();
		_selectionProvider = null;
		_followTransform = null;
	}

	protected virtual void Setup()
	{
		OverlayCanvas.alpha = (IsOpen ? 1f : 0f);
		OverlayCanvas.blocksRaycasts = IsOpen;
		OverlayCanvas.interactable = IsOpen;
		if (_promptEnabled)
		{
			PromptCanvas.alpha = (string.IsNullOrEmpty(_config.PromptText) ? 0f : 1f);
			PromptLabel.SetText(_config.PromptText);
		}
	}

	protected void OnHover(ManaColor color)
	{
		if (color != _previouslyHovered)
		{
			AudioManager.SetSwitch("color", AudioManager.GetColorKey(color), AudioManager.Default);
			AudioManager.PlayAudio("sfx_combat_manawheel_rollover", AudioManager.Default);
			_previouslyHovered = color;
		}
	}

	protected void OnClicked()
	{
		AudioManager.PlayAudio("sfx_combat_manawheel_click", AudioManager.Default);
	}
}
