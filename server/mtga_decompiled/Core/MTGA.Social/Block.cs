using HasbroGo.Social.Models;

namespace MTGA.Social;

public class Block
{
	public string BlockId { get; private set; }

	public SocialEntity BlockedPlayer { get; private set; }

	public Block(SocialEntity blockedPlayer)
	{
		BlockedPlayer = blockedPlayer;
	}

	public static Block BlockBySocialEntity(SocialEntity socialEntity)
	{
		return new Block(socialEntity);
	}

	public Block(string blockedPlayerFullName)
	{
		BlockedPlayer = new SocialEntity(blockedPlayerFullName);
	}

	public void SetBlockedPlayer(SocialEntity blockedPlayer)
	{
		BlockedPlayer = blockedPlayer;
	}

	public Block(BlockedUser platformBlock)
	{
		SetPlatformBlock(platformBlock);
	}

	public void SetPlatformBlock(BlockedUser platformBlock)
	{
		BlockId = platformBlock.BlockId;
		BlockedPlayer = new SocialEntity(platformBlock);
	}
}
