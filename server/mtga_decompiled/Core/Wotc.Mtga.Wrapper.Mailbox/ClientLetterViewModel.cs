using System;
using System.Collections.Generic;
using Wizards.Models;

namespace Wotc.Mtga.Wrapper.Mailbox;

public class ClientLetterViewModel
{
	public Guid Id;

	public string Title;

	public string Body;

	public string FallbackTitle;

	public string FallbackBody;

	public List<TreasureItem> Attachments;

	public ELetterArtContentType ArtContentType;

	public string ArtContentReferenceId;

	public DateTime CreationDate;

	public DateTime ExpiryDate;

	public ELetterType LetterType;

	public string MoreInfoHyperlink;

	public bool IsRead;

	public bool IsClaimed;

	public DateTime? DeleteDate;
}
