using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RewardDisplayCardSleeve : MonoBehaviour
{
	public Transform CardAnchor;

	private string _sleeveID;

	private CDCMetaCardView _cardView;

	private Action<string> _onObjectClicked;

	[SerializeField]
	public CustomButton ApplySleeveButton;

	[SerializeField]
	public Animator ApplySleeveButtonAnimator;

	public void Init(string sleeveID, CDCMetaCardView view, Action<string> onObjectClicked)
	{
		_sleeveID = sleeveID;
		_cardView = view;
		_onObjectClicked = onObjectClicked;
	}

	public void OnApplyButtonPressed()
	{
		_onObjectClicked?.Invoke(_sleeveID);
	}

	public void UnregisterOnApplyButtonPressed(Action<string> onObjectClicked)
	{
		_onObjectClicked = (Action<string>)Delegate.Remove(_onObjectClicked, onObjectClicked);
	}

	public void OnButtonPointerDown()
	{
		_cardView.OnPointerDown(new PointerEventData(null));
	}

	public void OnButtonPointerUp()
	{
		_cardView.OnPointerUp(new PointerEventData(null));
	}
}
