using System;
using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code.Network;
using Wizards.Arena.Promises;
using Wizards.Models;
using Wizards.Mtga;

namespace Core.Code.PlayerInbox;

public class PlayerInboxDataProvider : IDisposable
{
	public enum LetterDataChange
	{
		Error,
		Partial,
		All
	}

	private readonly IPlayerInboxServiceWrapper _playerInboxServiceWrapper;

	private List<Client_Letter> _letters;

	private DateTime _nextLetterUpdate = DateTime.MinValue;

	private const int LETTER_CACHE_SECONDS = 120;

	private Action<LetterDataChange, List<Client_Letter>> _onLetterDataChanged;

	private bool _enabled;

	public List<Client_Letter> Letters => _letters;

	public static PlayerInboxDataProvider Create()
	{
		return new PlayerInboxDataProvider(Pantry.Get<IPlayerInboxServiceWrapper>());
	}

	public PlayerInboxDataProvider(IPlayerInboxServiceWrapper playerInboxServiceWrapper)
	{
		_playerInboxServiceWrapper = playerInboxServiceWrapper;
	}

	public Promise<List<Client_Letter>> Initialize(bool enabled)
	{
		_enabled = enabled;
		return GetPlayerInbox(forceServiceRequest: true);
	}

	public Promise<List<Client_Letter>> GetPlayerInbox(bool forceServiceRequest = false)
	{
		if (!_enabled)
		{
			return new SimplePromise<List<Client_Letter>>(null);
		}
		if (!forceServiceRequest && _nextLetterUpdate > DateTime.Now && _letters != null)
		{
			return new SimplePromise<List<Client_Letter>>(_letters);
		}
		return _playerInboxServiceWrapper.GetPlayerInbox().Convert(delegate(GetPlayerInboxResp resp)
		{
			SetLetters(resp.Inbox.Select(Client_Letter.ConvertFromDTO).ToList());
			return _letters;
		}).IfError((Action)delegate
		{
			BroadcastLetterDataChange(LetterDataChange.Error, null);
		});
	}

	public Promise<Client_Letter> MarkLetterRead(Guid letterId)
	{
		if (!_enabled)
		{
			return new SimplePromise<Client_Letter>(null);
		}
		return _playerInboxServiceWrapper.MarkLetterRead(letterId).Convert(delegate(MarkLetterReadResp resp)
		{
			Client_Letter client_Letter = Client_Letter.ConvertFromDTO(resp.ModifiedLetter);
			SetLetter(client_Letter);
			return client_Letter;
		});
	}

	public Promise<Client_LetterAttachmentClaimed> ClaimLetterAttachment(Guid letterId)
	{
		if (!_enabled)
		{
			return new SimplePromise<Client_LetterAttachmentClaimed>(null);
		}
		return _playerInboxServiceWrapper.ClaimLetterAttachment(letterId).Convert(delegate(ClaimAttachmentResp resp)
		{
			Client_LetterAttachmentClaimed client_LetterAttachmentClaimed = Client_LetterAttachmentClaimed.ConvertFromDTO(resp);
			SetLetter(client_LetterAttachmentClaimed.letter);
			return client_LetterAttachmentClaimed;
		});
	}

	private void SetLetters(List<Client_Letter> letters)
	{
		_letters = letters;
		_nextLetterUpdate = DateTime.Now.AddSeconds(120.0);
		BroadcastLetterDataChange(LetterDataChange.All, _letters);
	}

	private void SetLetter(Client_Letter updatedLetter)
	{
		for (int i = 0; i < _letters.Count; i++)
		{
			if (updatedLetter.Id == _letters[i].Id)
			{
				_letters[i] = updatedLetter;
				BroadcastLetterDataChange(LetterDataChange.Partial, new List<Client_Letter> { updatedLetter });
				break;
			}
		}
	}

	public void AddLetters(List<Client_Letter> newLetters)
	{
		foreach (Client_Letter letter in newLetters)
		{
			if (_letters.Where((Client_Letter l) => l.Id == letter.Id).Count() == 0)
			{
				_letters.Add(letter);
			}
		}
		BroadcastLetterDataChange(LetterDataChange.All, _letters);
	}

	public void RegisterForLetterChanges(Action<LetterDataChange, List<Client_Letter>> handler)
	{
		_onLetterDataChanged = (Action<LetterDataChange, List<Client_Letter>>)Delegate.Combine(_onLetterDataChanged, handler);
	}

	public void UnRegisterForLetterChanges(Action<LetterDataChange, List<Client_Letter>> handler)
	{
		_onLetterDataChanged = (Action<LetterDataChange, List<Client_Letter>>)Delegate.Remove(_onLetterDataChanged, handler);
	}

	public void BroadcastLetterDataChange(LetterDataChange changeType, List<Client_Letter> letters)
	{
		_onLetterDataChanged?.Invoke(changeType, letters);
	}

	public void Dispose()
	{
		_letters = null;
		_nextLetterUpdate = DateTime.MinValue;
		_onLetterDataChanged = null;
	}
}
