using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Wotc.Mtga.Loc;

public class Toggle_Slider : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private Localize _toggleNameLoc;

	[FormerlySerializedAs("_optionASelectedLoc")]
	[SerializeField]
	private Localize _optionASelectedLabel;

	[FormerlySerializedAs("_optionBSelectedLoc")]
	[SerializeField]
	private Localize _optionBSelectedLabel;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private float _toggleDelay;

	private DateTime _lastToggleTime = DateTime.Now;

	public UnityEvent OnHover;

	public UnityEvent OnEndHover;

	public UnityEvent OnSelectA;

	public UnityEvent OnSelectB;

	private Action _callback_OnSelect_OptionA;

	private Action _callback_OnSelect_OptionB;

	private string _id_optionA;

	private string _id_optionB;

	private bool _currentSelectionIsOptionB;

	public void Init(string toggleNameKey, string optionAId, string optionALocKey, Action callbackOptionA, string optionBId, string optionBLocKey, Action callbackOptionB, string selectedOptionId)
	{
		_callback_OnSelect_OptionA = callbackOptionA;
		_callback_OnSelect_OptionB = callbackOptionB;
		_toggleNameLoc.SetText(toggleNameKey);
		_optionASelectedLabel?.SetText(optionALocKey);
		_optionBSelectedLabel?.SetText(optionBLocKey);
		_id_optionA = optionAId;
		_id_optionB = optionBId;
		_currentSelectionIsOptionB = selectedOptionId == optionBId;
		UpdateToggle();
	}

	public void SelectOption(string optionId)
	{
		if (optionId == _id_optionA)
		{
			_currentSelectionIsOptionB = false;
			_animator.SetTrigger("SelectChoiceA");
			_animator.ResetTrigger("SelectChoiceB");
			return;
		}
		if (optionId == _id_optionB)
		{
			_currentSelectionIsOptionB = true;
			_animator.SetTrigger("SelectChoiceB");
			_animator.ResetTrigger("SelectChoiceA");
			return;
		}
		Debug.LogError("[Toggle_Slider] Tried to select option " + optionId + " which didn't match available ids (A: " + _id_optionA + " or B: " + _id_optionB + ")");
	}

	public void SelectOption(bool rightOption)
	{
		string optionId = (rightOption ? _id_optionB : _id_optionA);
		SelectOption(optionId);
	}

	private void OnEnable()
	{
		UpdateToggle();
	}

	private void OnDestroy()
	{
		_callback_OnSelect_OptionA = null;
		_callback_OnSelect_OptionB = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if ((DateTime.Now - _lastToggleTime).TotalSeconds > (double)_toggleDelay)
		{
			_lastToggleTime = DateTime.Now;
			_currentSelectionIsOptionB = !_currentSelectionIsOptionB;
			UpdateToggle();
			if (_currentSelectionIsOptionB)
			{
				_callback_OnSelect_OptionB();
			}
			else
			{
				_callback_OnSelect_OptionA();
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnHover.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnEndHover.Invoke();
	}

	private void UpdateToggle()
	{
		if (base.isActiveAndEnabled)
		{
			if (_currentSelectionIsOptionB)
			{
				OnSelectB.Invoke();
			}
			else
			{
				OnSelectA.Invoke();
			}
		}
	}
}
