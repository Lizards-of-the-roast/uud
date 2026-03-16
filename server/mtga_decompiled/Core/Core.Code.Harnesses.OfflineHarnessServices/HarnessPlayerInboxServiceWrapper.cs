using System;
using System.Collections.Generic;
using Core.Shared.Code.Network;
using Newtonsoft.Json;
using Wizards.Arena.Enums.Player;
using Wizards.Arena.Promises;
using Wizards.Models;
using Wizards.Unification.Models.Player;

namespace Core.Code.Harnesses.OfflineHarnessServices;

public class HarnessPlayerInboxServiceWrapper : IPlayerInboxServiceWrapper
{
	private GetPlayerInboxResp _mockInbox;

	private const string MOCK_LETTER_PREFIX = "Client Mock: ";

	private string _mockLetter = "{\"Id\":\"5d021537-bfcc-44e7-8a6b-2df3e754db6f\",\"LetterType\":0,\"CreatedDate\":\"2022-08-11T21:14:45.3524413-07:00\",\"UpdatedDate\":null,\"ExpiryDate\":\"2022-08-18T21:14:45.3524413-07:00\",\"ChangeSource\":\"Letter\",\"Content\":{\"Title\":\"\",\"Body\":\"\",\"FallbackTitle\":\"Letter Title\",\"FallbackBody\":\"Letter Body text\",\"Attachments\":[],\"ArtContentType\":0,\"ArtContentReferenceId\":null},\"State\":{}}";

	private string _mockLetterWithAttachement = "{\"Id\":\"df0a4e82-3284-47cd-95cf-8913665423e3\",\"LetterType\":1,\"CreatedDate\":\"2022-08-12T16:47:00.1271939Z\",\"ExpiryDate\":\"2022-08-15T09:46:00\",\"ChangeSource\":\"Letter\",\"Content\":{\"Title\":\"\",\"Body\":\"\",\"FallbackTitle\":\"Letter With Attachment Title\",\"FallbackBody\":\"Letter Body text\",\"Attachments\":[{\"TreasureType\":\"Gold\",\"ReferenceId\":\"Gold\",\"Quantity\":1000}],\"ArtContentType\":1,\"ArtContentReferenceId\":\"\"},\"State\":{}}";

	public Promise<GetPlayerInboxResp> GetPlayerInbox()
	{
		if (_mockInbox == null)
		{
			_mockInbox = GenerateMockPlayerLetters();
		}
		return new SimplePromise<GetPlayerInboxResp>(_mockInbox);
	}

	public Promise<MarkLetterReadResp> MarkLetterRead(Guid letterId)
	{
		foreach (Letter item in _mockInbox.Inbox)
		{
			if (item.Id == letterId)
			{
				item.State.ReadDate = DateTime.Now;
				return new SimplePromise<MarkLetterReadResp>(new MarkLetterReadResp
				{
					ModifiedLetter = item
				});
			}
		}
		return new SimplePromise<MarkLetterReadResp>(null);
	}

	public Promise<ClaimAttachmentResp> ClaimLetterAttachment(Guid letterId)
	{
		foreach (Letter item in _mockInbox.Inbox)
		{
			if (item.Id == letterId && item.Content.Attachments.Count > 0 && !item.State.ClaimDate.HasValue)
			{
				item.State.ClaimDate = DateTime.Now;
				return new SimplePromise<ClaimAttachmentResp>(new ClaimAttachmentResp
				{
					ModifiedLetter = item,
					DTO_InventoryInfo = new InventoryInfo
					{
						Changes = new List<InventoryChange>
						{
							new InventoryChange(InventoryChangeSource.Letter)
							{
								InventoryGold = 1000
							}
						}
					}
				});
			}
		}
		return new SimplePromise<ClaimAttachmentResp>(null);
	}

	public void AddLetterFromJson(string letterJson, bool asMock = true)
	{
		Letter item = LetterFromJson(letterJson, newId: true, asMock);
		if (_mockInbox.Inbox == null)
		{
			_mockInbox.Inbox = new List<Letter>();
		}
		_mockInbox.Inbox.Add(item);
	}

	public void ClearInbox()
	{
		_mockInbox?.Inbox?.Clear();
	}

	public void ResetInbox()
	{
		_mockInbox = null;
	}

	private Letter LetterFromJson(string letterJson, bool newId = true, bool asMock = true)
	{
		Letter letter = JsonConvert.DeserializeObject<Letter>(letterJson);
		if (newId)
		{
			letter.Id = Guid.NewGuid();
		}
		if (asMock)
		{
			letter.CreatedDate = DateTime.Now;
			letter.ExpiryDate = DateTime.Now.AddDays(7.0);
			if (!letter.Content.FallbackTitle.StartsWith("Client Mock: "))
			{
				letter.Content.FallbackTitle = "Client Mock: " + letter.Content.FallbackTitle;
			}
			if (!letter.Content.FallbackBody.StartsWith("Client Mock: "))
			{
				letter.Content.FallbackBody = "Client Mock: " + letter.Content.FallbackBody;
			}
		}
		return letter;
	}

	private GetPlayerInboxResp GenerateMockPlayerLetters()
	{
		GetPlayerInboxResp getPlayerInboxResp = new GetPlayerInboxResp();
		getPlayerInboxResp.Inbox = new List<Letter>();
		getPlayerInboxResp.Inbox.Add(LetterFromJson(_mockLetter, newId: false));
		getPlayerInboxResp.Inbox.Add(LetterFromJson(_mockLetterWithAttachement, newId: false));
		return getPlayerInboxResp;
	}
}
