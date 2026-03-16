using UnityEngine;

namespace Wotc.Mtga.DuelScene.AvatarView;

public class AvatarFramePart : AvatarColorOverridePart
{
	[SerializeField]
	private AvatarFramePartType _type;

	public AvatarFramePartType Type => _type;
}
