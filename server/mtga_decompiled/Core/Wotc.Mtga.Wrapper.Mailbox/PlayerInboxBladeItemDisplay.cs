using System;
using System.Collections.Generic;
using UnityEngine;
using Wizards.Models;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.Mailbox;

public class PlayerInboxBladeItemDisplay : MonoBehaviour
{
	private static readonly int LetterStateAnimatorIntegerHash = Animator.StringToHash("LetterState");

	private static readonly int AttachmentAnimatorBoolHash = Animator.StringToHash("Attachment");

	private Action<PlayerInboxBladeItemDisplay, bool, Guid> _onLetterSelected;

	[SerializeField]
	private Localize _titleText;

	[SerializeField]
	private Localize _alertText;

	[SerializeField]
	private Localize _date;

	[SerializeField]
	private Animator _letterDisplayAnimator;

	[SerializeField]
	private GameObject _readCheckmark;

	public ClientLetterViewModel _clientBladeItemViewModel { get; private set; }

	public void Initialize(Action<PlayerInboxBladeItemDisplay, bool, Guid> onLetterSelected)
	{
		_onLetterSelected = onLetterSelected;
	}

	public void SetLetterViewModel(ClientLetterViewModel clientLetter)
	{
		_clientBladeItemViewModel = clientLetter;
		_titleText.SetText(_clientBladeItemViewModel.Title, null, clientLetter.FallbackTitle);
		_alertText.SetText("PlayerInbox/NewAlert");
		_date.SetText("PlayerInbox/DateFormat", new Dictionary<string, string>
		{
			{
				"Month",
				clientLetter.CreationDate.ToString("MM")
			},
			{
				"Day",
				clientLetter.CreationDate.ToString("dd")
			},
			{
				"Year",
				clientLetter.CreationDate.ToString("yyyy")
			}
		});
		_readCheckmark.SetActive(_clientBladeItemViewModel.IsRead);
		_setReadAndClaimedView();
	}

	private void _setReadAndClaimedView()
	{
		if (_clientBladeItemViewModel == null)
		{
			return;
		}
		if (!_clientBladeItemViewModel.IsClaimed)
		{
			List<TreasureItem> attachments = _clientBladeItemViewModel.Attachments;
			if (attachments != null && attachments.Count > 0)
			{
				_letterDisplayAnimator.SetBool(AttachmentAnimatorBoolHash, value: true);
				goto IL_0058;
			}
		}
		_letterDisplayAnimator.SetBool(AttachmentAnimatorBoolHash, value: false);
		goto IL_0058;
		IL_0058:
		if (!_clientBladeItemViewModel.IsRead)
		{
			_letterDisplayAnimator.SetInteger(LetterStateAnimatorIntegerHash, 0);
			return;
		}
		_readCheckmark.SetActive(value: true);
		_letterDisplayAnimator.SetInteger(LetterStateAnimatorIntegerHash, 2);
	}

	public void SetSelectedView()
	{
		_letterDisplayAnimator.SetInteger(LetterStateAnimatorIntegerHash, 1);
	}

	public void OnClick()
	{
		_onLetterSelected?.Invoke(this, _clientBladeItemViewModel.IsRead, _clientBladeItemViewModel.Id);
		_clientBladeItemViewModel.IsRead = true;
		_letterDisplayAnimator.SetInteger(LetterStateAnimatorIntegerHash, 1);
	}

	public void DeselectLetter()
	{
		if (_letterDisplayAnimator.GetInteger(LetterStateAnimatorIntegerHash) == 1)
		{
			_readCheckmark.SetActive(_clientBladeItemViewModel.IsRead);
			_letterDisplayAnimator.SetInteger(LetterStateAnimatorIntegerHash, 2);
		}
	}

	public void OnEnable()
	{
		_setReadAndClaimedView();
	}

	public void OnDestroy()
	{
		_onLetterSelected = null;
	}
}
