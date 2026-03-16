using System;
using UnityEngine;
using Wotc.Mtga.Loc;

public class RewardDisplayTitle : MonoBehaviour
{
	[SerializeField]
	private CustomButton setDefaultButton;

	[SerializeField]
	private Animator setDefaultButtonAnimator;

	[SerializeField]
	private Localize titleLocText;

	private string _titleId;

	private static readonly int Disabled = Animator.StringToHash("Disabled");

	private event Action<string> OnDefaultClicked;

	public void Init(string titleId, string titleLocKey, Action<string> onDefaultClicked)
	{
		_titleId = titleId;
		titleLocText.SetText(titleLocKey, null, titleLocKey);
		this.OnDefaultClicked = onDefaultClicked;
	}

	public void OnApplyButtonPressed()
	{
		this.OnDefaultClicked?.Invoke(_titleId);
		setDefaultButtonAnimator.SetBool(Disabled, value: true);
		setDefaultButton.Interactable = false;
	}
}
