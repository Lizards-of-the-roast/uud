using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class ConfirmWidget : MonoBehaviour
{
	public struct Option
	{
		public string Text;

		public string IconPath;
	}

	[Header("Rects")]
	[SerializeField]
	private RectTransform _rootRect;

	[SerializeField]
	private Transform _widgetRoot;

	[SerializeField]
	private RectTransform _tailRect;

	[Header("Event Area")]
	[SerializeField]
	private EventTrigger _cancelEventTrigger;

	[Header("Confirm Buttons")]
	[SerializeField]
	private ConfirmWidgetButton _buttonPrefab;

	[SerializeField]
	private RectTransform _buttonRoot;

	[Header("Placement Settings")]
	[SerializeField]
	private float _maxTailOffset = 72f;

	[SerializeField]
	private float _screenEdgePadding = 15f;

	private Animator _animator;

	private Camera _camera;

	private List<ConfirmWidgetButton> _buttons = new List<ConfirmWidgetButton>();

	private List<Option> _options = new List<Option>();

	public bool IsOpen { get; private set; }

	public IReadOnlyList<ConfirmWidgetButton> Buttons => _buttons;

	public event Action<Option> OptionSelected;

	public event Action Cancelled;

	public event Action Opened;

	public event Action Closed;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void Init(Camera camera)
	{
		_camera = camera;
		EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
		triggerEvent.AddListener(onCancelClick);
		_cancelEventTrigger.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerClick,
			callback = triggerEvent
		});
		void onCancelClick(BaseEventData evtData)
		{
			Cancel();
		}
	}

	private void OnDestroy()
	{
		if ((bool)_cancelEventTrigger)
		{
			List<EventTrigger.Entry> triggers = _cancelEventTrigger.triggers;
			while (triggers.Count > 0)
			{
				EventTrigger.Entry entry = triggers[0];
				triggers.RemoveAt(0);
				entry.callback.RemoveAllListeners();
			}
		}
		this.OptionSelected = null;
		this.Cancelled = null;
		this.Opened = null;
		this.Closed = null;
	}

	public void Open(DuelScene_CDC cdc, params Option[] options)
	{
		Open(cdc.Root, options);
	}

	public void Open(Transform root, params Option[] options)
	{
		Vector2 screenPos = _camera.WorldToScreenPoint(root.position);
		Open(screenPos, options);
	}

	public void Open(Vector2 screenPos, params Option[] options)
	{
		IsOpen = true;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_rootRect, screenPos, _camera, out var localPoint);
		float num = _buttonRoot.rect.width * 0.5f;
		float num2 = _rootRect.rect.width * 0.5f - _screenEdgePadding;
		Vector2 vector = _tailRect.localPosition;
		if (localPoint.x - num < 0f - num2)
		{
			float num3 = 0f - num2 + num;
			vector.x = Mathf.Max(0f - num + _maxTailOffset, localPoint.x - num3);
			localPoint.x = num3;
		}
		else if (localPoint.x + num > num2)
		{
			float num4 = num2 - num;
			vector.x = Mathf.Min(num - _maxTailOffset, localPoint.x - num4);
			localPoint.x = num4;
		}
		else
		{
			vector.x = 0f;
		}
		_tailRect.localPosition = vector;
		_widgetRoot.localPosition = localPoint;
		_options.Clear();
		_options.AddRange(options);
		for (int i = 0; i < options.Length; i++)
		{
			Option option = options[i];
			ConfirmWidgetButton confirmWidgetButton = UnityEngine.Object.Instantiate(_buttonPrefab, _buttonRoot);
			confirmWidgetButton.SetText(option.Text);
			confirmWidgetButton.SetSprite(option.IconPath);
			confirmWidgetButton.Clicked += OnButtonClicked;
			_buttons.Add(confirmWidgetButton);
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_combat_manawheel_open, AudioManager.Default);
		_camera.GetComponent<BaseRaycaster>().enabled = false;
		_animator.SetBool("Open", value: true);
		this.Opened?.Invoke();
	}

	public void Cancel()
	{
		this.Cancelled?.Invoke();
	}

	public void Close()
	{
		IsOpen = false;
		while (_buttons.Count > 0)
		{
			ConfirmWidgetButton confirmWidgetButton = _buttons[0];
			_buttons.RemoveAt(0);
			confirmWidgetButton.Clicked -= OnButtonClicked;
			UnityEngine.Object.Destroy(confirmWidgetButton.gameObject);
		}
		_options.Clear();
		AudioManager.PlayAudio(WwiseEvents.sfx_combat_manawheel_close, AudioManager.Default);
		_camera.GetComponent<BaseRaycaster>().enabled = true;
		_animator.SetBool("Open", value: false);
		this.Closed?.Invoke();
		this.Opened = null;
		this.Closed = null;
	}

	private void OnButtonClicked(ConfirmWidgetButton confirmWidgetButton)
	{
		int num = _buttons.IndexOf(confirmWidgetButton);
		if (-1 < num && num < _options.Count)
		{
			Option obj = _options[num];
			this.OptionSelected?.Invoke(obj);
		}
	}
}
