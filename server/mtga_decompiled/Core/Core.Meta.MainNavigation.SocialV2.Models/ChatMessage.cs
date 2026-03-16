using Google.Protobuf.WellKnownTypes;

namespace Core.Meta.MainNavigation.SocialV2.Models;

public class ChatMessage
{
	public uint MessageId;

	public Timestamp CreatedOn;

	public string AvatarId;

	public string AuthorId;

	public string AuthorName;

	public string MessageContent;
}
