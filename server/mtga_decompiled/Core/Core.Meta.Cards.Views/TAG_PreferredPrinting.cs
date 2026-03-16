using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Meta.Cards.Views;

public class TAG_PreferredPrinting : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Toggle _toggle;

	[SerializeField]
	private GameObject _container_Fave;

	private Action<PagesMetaCardView> _onExpansionToggled;

	private Action<PagesMetaCardView, bool> _onPreferredPrintingToggleClicked;

	private PagesMetaCardView _parentCardView;

	private PagesMetaCardView.ExpandedDisplayStyle _expandStyle;

	private bool _isStyle;

	private bool _isPreferredPrinting;

	private bool _areClickHandlersSet;

	private bool _isSelectable;

	private bool _containsNewCards;

	private static readonly int NewPrintingAlert = Animator.StringToHash("NewPrintingAlert");

	private static readonly int Favorite = Animator.StringToHash("Favorite");

	private static readonly int ExpandState = Animator.StringToHash("ExpandState");

	private static readonly int VisualType = Animator.StringToHash("VisualType");

	public void SetClickHandlers(Action<PagesMetaCardView> onExpansionToggled, Action<PagesMetaCardView, bool> onPreferredPrintingToggleClicked, PagesMetaCardView parentCardView)
	{
		_onExpansionToggled = (Action<PagesMetaCardView>)Delegate.Combine(_onExpansionToggled, onExpansionToggled);
		_onPreferredPrintingToggleClicked = (Action<PagesMetaCardView, bool>)Delegate.Combine(_onPreferredPrintingToggleClicked, onPreferredPrintingToggleClicked);
		_parentCardView = parentCardView;
		_areClickHandlersSet = true;
	}

	public void SetViewData(PagesMetaCardView.ExpandedDisplayStyle expandStyle, bool isStyle, bool isPreferredPrinting, bool isSelectable, bool containsNewCards, bool isCollapsing)
	{
		_expandStyle = expandStyle;
		_isStyle = isStyle;
		_isPreferredPrinting = isPreferredPrinting;
		_isSelectable = isSelectable;
		_containsNewCards = containsNewCards;
		SetAnimatorState(_expandStyle, _isPreferredPrinting, isCollapsing);
	}

	private void OnEnable()
	{
		SetAnimatorState(_expandStyle, _isPreferredPrinting, isCollapsing: false);
	}

	public bool GetAreClickHandlersSet()
	{
		return _areClickHandlersSet;
	}

	private void SetAnimatorState(PagesMetaCardView.ExpandedDisplayStyle style, bool isPreferredPrinting, bool isCollapsing)
	{
		_isPreferredPrinting = isPreferredPrinting;
		_expandStyle = style;
		_container_Fave.SetActive(_isSelectable);
		if (_animator.isInitialized && style != PagesMetaCardView.ExpandedDisplayStyle.Solo)
		{
			if (isCollapsing)
			{
				StartOffscreenCollapsedAnimation();
			}
			_animator.SetInteger(ExpandState, ConvertExpandedDisplayStyleToAnimatorState(style));
			_animator.SetInteger(VisualType, ConvertVisualTypeToAnimatorState(_isStyle));
			_animator.SetBool(Favorite, isPreferredPrinting && _isSelectable);
			_animator.SetBool(NewPrintingAlert, _containsNewCards);
			_toggle.SetIsOnWithoutNotify(isPreferredPrinting && _isSelectable);
		}
	}

	public void StartOffscreenCollapsedAnimation()
	{
		_animator.Play("Transition");
	}

	public bool GetToggleValue()
	{
		return _toggle.isOn;
	}

	private int ConvertExpandedDisplayStyleToAnimatorState(PagesMetaCardView.ExpandedDisplayStyle style)
	{
		switch (style)
		{
		case PagesMetaCardView.ExpandedDisplayStyle.Stacked:
			return 0;
		case PagesMetaCardView.ExpandedDisplayStyle.Expanded_First:
			return 1;
		case PagesMetaCardView.ExpandedDisplayStyle.Expanded_Mid:
			return 2;
		case PagesMetaCardView.ExpandedDisplayStyle.Expanded_Last:
			return 3;
		case PagesMetaCardView.ExpandedDisplayStyle.Solo:
			throw new ArgumentException("Solo is not a valid ExpandedDisplayStyle for TAG_PreferredPrinting.");
		default:
			Debug.LogError("Unimplemented ExpandedDisplayStyle encountered in TAG_PreferredPrinting");
			return 0;
		}
	}

	private int ConvertVisualTypeToAnimatorState(bool isStyle)
	{
		if (!isStyle)
		{
			return 0;
		}
		return 1;
	}

	public void OnExpansionToggled()
	{
		_onExpansionToggled?.Invoke(_parentCardView);
	}

	public void OnPreferredPrintingToggleClicked()
	{
		_isPreferredPrinting = GetToggleValue() && _isSelectable;
		SetAnimatorState(_expandStyle, _isPreferredPrinting, isCollapsing: false);
		if (_isSelectable)
		{
			_onPreferredPrintingToggleClicked?.Invoke(_parentCardView, _isPreferredPrinting);
		}
	}

	public void OnDestroy()
	{
		_onExpansionToggled = null;
		_onPreferredPrintingToggleClicked = null;
		_parentCardView = null;
		_areClickHandlersSet = false;
	}

	public void OnDisable()
	{
		_onExpansionToggled = null;
		_onPreferredPrintingToggleClicked = null;
		_areClickHandlersSet = false;
	}
}
