using System;
using UnityEngine;
using Wizards.Arena.Models.Network;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Cosmetics;

public class TitleListViewItem : MonoBehaviour
{
	[SerializeField]
	private Localize titleTextField;

	[SerializeField]
	private Animator animator;

	private bool _isSelected;

	private Action<TitleListViewItem> _changeSelection;

	private static readonly int Disabled = Animator.StringToHash("IsLocked");

	private static readonly int Selected = Animator.StringToHash("Selected");

	public CosmeticTitleEntry TitleData { get; set; }

	public bool IsOwned { get; set; }

	public string LocalizedText { get; private set; }

	public void Initialize(CosmeticTitleEntry titleData, bool isOwned, bool isPreferredTitle, Action<TitleListViewItem> changeSelection)
	{
		TitleData = titleData;
		if (!string.IsNullOrWhiteSpace(titleData.LocKey))
		{
			titleTextField.SetText(titleData.LocKey, null, titleData.LocKey);
		}
		else
		{
			titleTextField.SetText("Titles/Core/NoTitle");
		}
		LocalizedText = titleTextField.TextTarget.LocalizedString;
		IsOwned = isOwned;
		_isSelected = isPreferredTitle;
		_changeSelection = changeSelection;
		animator.SetBool(Disabled, !IsOwned);
		animator.SetBool(Selected, _isSelected);
	}

	public void OnClick()
	{
		_changeSelection?.Invoke(this);
	}

	public void SetSelected(bool selected)
	{
		_isSelected = selected;
		animator.SetBool(Selected, selected);
	}
}
