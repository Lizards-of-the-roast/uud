using System;
using System.Collections.Generic;
using Core.Code.Input;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MTGA.KeyboardManager;

public class KeyboardManager
{
	private readonly SortedList<PriorityLevelEnum, List<IKeyUpSubscriber>> _keyUpSubs = new SortedList<PriorityLevelEnum, List<IKeyUpSubscriber>>();

	private readonly SortedList<PriorityLevelEnum, List<IKeyDownSubscriber>> _keyDownSubs = new SortedList<PriorityLevelEnum, List<IKeyDownSubscriber>>();

	private readonly SortedList<PriorityLevelEnum, List<IKeyHeldSubscriber>> _keyHeldSubs = new SortedList<PriorityLevelEnum, List<IKeyHeldSubscriber>>();

	private readonly List<KeyCode> _keyCodes;

	private readonly Dictionary<KeyCode, long> _heldKeys = new Dictionary<KeyCode, long>();

	private Modifiers _mods;

	private GameObject _previousSelection;

	private GameObject _currentSelection;

	private bool _isSelectingInput;

	private readonly List<IKeyUpSubscriber> _keyUpSubsInvocationList = new List<IKeyUpSubscriber>();

	private readonly List<IKeyDownSubscriber> _keyDownSubsInvocationList = new List<IKeyDownSubscriber>();

	private readonly List<IKeyHeldSubscriber> _keyHeldSubsInvocationList = new List<IKeyHeldSubscriber>();

	public KeyboardManager()
	{
		_keyCodes = new List<KeyCode>();
		foreach (object value in EnumHelper.GetValues(typeof(KeyCode)))
		{
			if ((KeyCode)value < KeyCode.RightWindows)
			{
				_keyCodes.Add((KeyCode)value);
				continue;
			}
			break;
		}
	}

	private void UpdatePendingAddRemoves()
	{
		CullNullSubscribers<IKeyDownSubscriber>(_keyDownSubs);
		CullNullSubscribers<IKeyUpSubscriber>(_keyUpSubs);
		static void CullNullSubscribers<T>(SortedList<PriorityLevelEnum, List<T>> sortedList) where T : IKeySubscriber
		{
			for (int num = sortedList.Count - 1; num >= 0; num--)
			{
				List<T> list = sortedList.Values[num];
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					if (list[num2] == null)
					{
						list.RemoveAt(num2);
					}
				}
				if (list.Count == 0)
				{
					sortedList.RemoveAt(num);
				}
			}
		}
	}

	public void Update()
	{
		UpdatePendingAddRemoves();
		if (!Application.isFocused || (_keyUpSubs.Keys.Count == 0 && _keyDownSubs.Keys.Count == 0))
		{
			return;
		}
		if ((bool)EventSystem.current)
		{
			if (_currentSelection != EventSystem.current.currentSelectedGameObject)
			{
				_previousSelection = _currentSelection;
				_currentSelection = EventSystem.current.currentSelectedGameObject;
			}
			if (_currentSelection != _previousSelection)
			{
				_isSelectingInput = (bool)_currentSelection && (bool)_currentSelection.GetComponent<TMP_InputField>();
			}
		}
		else
		{
			_isSelectingInput = false;
		}
		bool flag = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab);
		if (_isSelectingInput && !flag)
		{
			return;
		}
		_mods.Alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		_mods.Ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		_mods.Shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		foreach (KeyCode keyCode in _keyCodes)
		{
			if (keyCode == KeyCode.Escape && ActionSystemFactory.CaptureEscapeFeatureToggle)
			{
				continue;
			}
			if (Input.GetKeyDown(keyCode))
			{
				PublishKeyDown(keyCode);
				_heldKeys[keyCode] = DateTime.Now.Ticks;
				PublishKeyHeld(keyCode, 0f);
			}
			else if (Input.GetKeyUp(keyCode))
			{
				PublishKeyUp(keyCode);
				_heldKeys.Remove(keyCode);
			}
			else if (Input.GetKey(keyCode))
			{
				if (_heldKeys.TryGetValue(keyCode, out var value))
				{
					PublishKeyHeld(keyCode, (float)TimeSpan.FromTicks(DateTime.Now.Ticks - value).TotalSeconds);
				}
			}
			else if (_heldKeys.ContainsKey(keyCode))
			{
				_heldKeys.Remove(keyCode);
			}
		}
	}

	public void Subscribe(IKeySubscriber sub)
	{
		if (sub is IKeyDownSubscriber subscriber)
		{
			AddSubscriber<IKeyDownSubscriber>(subscriber, _keyDownSubs);
		}
		if (sub is IKeyUpSubscriber subscriber2)
		{
			AddSubscriber<IKeyUpSubscriber>(subscriber2, _keyUpSubs);
		}
		if (sub is IKeyHeldSubscriber subscriber3)
		{
			AddSubscriber<IKeyHeldSubscriber>(subscriber3, _keyHeldSubs);
		}
		static void AddSubscriber<T>(T item, SortedList<PriorityLevelEnum, List<T>> priorityToSubscribers) where T : IKeySubscriber
		{
			if (!priorityToSubscribers.ContainsKey(item.Priority) || !priorityToSubscribers[item.Priority].Contains(item))
			{
				if (priorityToSubscribers.TryGetValue(item.Priority, out var value))
				{
					value.Insert(0, item);
				}
				else
				{
					priorityToSubscribers.Add(item.Priority, new List<T> { item });
				}
			}
		}
	}

	public void Unsubscribe(IKeySubscriber sub)
	{
		if (sub is IKeyDownSubscriber keyDownSubscriber && _keyDownSubs.ContainsKey(keyDownSubscriber.Priority))
		{
			_keyDownSubs[keyDownSubscriber.Priority].Remove(keyDownSubscriber);
		}
		if (sub is IKeyUpSubscriber keyUpSubscriber && _keyUpSubs.ContainsKey(keyUpSubscriber.Priority))
		{
			_keyUpSubs[keyUpSubscriber.Priority].Remove(keyUpSubscriber);
		}
		if (sub is IKeyHeldSubscriber keyHeldSubscriber && _keyHeldSubs.ContainsKey(keyHeldSubscriber.Priority))
		{
			_keyHeldSubs[keyHeldSubscriber.Priority].Remove(keyHeldSubscriber);
		}
	}

	private void PublishKeyUp(KeyCode key)
	{
		foreach (var (_, collection) in _keyUpSubs)
		{
			_keyUpSubsInvocationList.Clear();
			_keyUpSubsInvocationList.AddRange(collection);
			foreach (IKeyUpSubscriber keyUpSubsInvocation in _keyUpSubsInvocationList)
			{
				if (keyUpSubsInvocation.HandleKeyUp(key, _mods))
				{
					return;
				}
			}
		}
	}

	private void PublishKeyDown(KeyCode key)
	{
		foreach (var (_, collection) in _keyDownSubs)
		{
			_keyDownSubsInvocationList.Clear();
			_keyDownSubsInvocationList.AddRange(collection);
			foreach (IKeyDownSubscriber keyDownSubsInvocation in _keyDownSubsInvocationList)
			{
				if (keyDownSubsInvocation.HandleKeyDown(key, _mods))
				{
					return;
				}
			}
		}
	}

	private void PublishKeyHeld(KeyCode key, float holdDuration)
	{
		foreach (var (_, collection) in _keyHeldSubs)
		{
			_keyHeldSubsInvocationList.Clear();
			_keyHeldSubsInvocationList.AddRange(collection);
			foreach (IKeyHeldSubscriber keyHeldSubsInvocation in _keyHeldSubsInvocationList)
			{
				keyHeldSubsInvocation.HandleKeyHeld(key, holdDuration);
			}
		}
	}
}
