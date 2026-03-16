using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UI;

[RequireComponent(typeof(Animator))]
public class PlayerDesignationWidget : MonoBehaviour, IPlayerInfoSlotItem
{
	[SerializeField]
	private TextMeshProUGUI _label;

	[SerializeField]
	private EventTrigger _eventTrigger;

	[SerializeField]
	private GameObject _introVfx;

	[SerializeField]
	private Image _goldIdentityIcon;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _buttonImage;

	private Animator _animator;

	private const string ANIMPROPERTY_STATE = "State";

	private const int ANIMSTATE_NONE = 0;

	private const int ANIMSTATE_HOVER = 1;

	private const int ANIMSTATE_MOUSEDOWN = 2;

	private bool _goldIdentityEnabled;

	public bool GoldIdentityIconEnabled
	{
		set
		{
			if ((bool)_goldIdentityIcon)
			{
				_goldIdentityIcon.gameObject.SetActive(value);
				_goldIdentityEnabled = value;
			}
		}
	}

	public event Action PointerEntered;

	public event Action PointerExited;

	public event Action Clicked;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		base.gameObject.UpdateActive(active: false);
		if ((bool)_goldIdentityIcon)
		{
			_goldIdentityIcon.gameObject.SetActive(_goldIdentityEnabled);
		}
		EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
		triggerEvent.AddListener(onPointerEnter);
		_eventTrigger.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerEnter,
			callback = triggerEvent
		});
		EventTrigger.TriggerEvent triggerEvent2 = new EventTrigger.TriggerEvent();
		triggerEvent2.AddListener(onPointerExit);
		_eventTrigger.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerExit,
			callback = triggerEvent2
		});
		EventTrigger.TriggerEvent triggerEvent3 = new EventTrigger.TriggerEvent();
		triggerEvent3.AddListener(onPointerDown);
		_eventTrigger.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerDown,
			callback = triggerEvent3
		});
		EventTrigger.TriggerEvent triggerEvent4 = new EventTrigger.TriggerEvent();
		triggerEvent4.AddListener(onClick);
		_eventTrigger.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerClick,
			callback = triggerEvent4
		});
		void onClick(BaseEventData eventData)
		{
			_animator.SetInteger("State", 1);
			this.Clicked?.Invoke();
		}
		void onPointerDown(BaseEventData eventData)
		{
			_animator.SetInteger("State", 2);
		}
		void onPointerEnter(BaseEventData eventData)
		{
			_animator.SetInteger("State", 1);
			this.PointerEntered?.Invoke();
		}
		void onPointerExit(BaseEventData eventData)
		{
			_animator.SetInteger("State", 0);
			this.PointerExited?.Invoke();
		}
	}

	private void OnDisable()
	{
		if (_introVfx != null && _introVfx.activeSelf)
		{
			UnityEngine.Object.Destroy(_introVfx.gameObject);
		}
	}

	private void OnDestroy()
	{
		this.PointerEntered = null;
		this.PointerExited = null;
		this.Clicked = null;
		List<EventTrigger.Entry> triggers = _eventTrigger.triggers;
		foreach (EventTrigger.Entry item in triggers)
		{
			item.callback.RemoveAllListeners();
		}
		triggers.Clear();
	}

	public void SetText(string text)
	{
		base.gameObject.UpdateActive(!string.IsNullOrEmpty(text));
		_label.SetText(text);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.gameObject.GetComponent<RectTransform>());
	}

	public void PlayVFX()
	{
		_introVfx?.SetActive(value: true);
	}

	public void FadeOut(float duration)
	{
		_label.CrossFadeAlpha(0f, duration, ignoreTimeScale: false);
		_icon.CrossFadeAlpha(0f, duration, ignoreTimeScale: false);
		_goldIdentityIcon.CrossFadeAlpha(0f, duration, ignoreTimeScale: false);
		_buttonImage.raycastTarget = false;
	}

	public void FadeIn(float duration)
	{
		_label.CrossFadeAlpha(1f, duration, ignoreTimeScale: false);
		_icon.CrossFadeAlpha(1f, duration, ignoreTimeScale: false);
		_goldIdentityIcon.CrossFadeAlpha(1f, duration, ignoreTimeScale: false);
		_buttonImage.raycastTarget = true;
	}
}
