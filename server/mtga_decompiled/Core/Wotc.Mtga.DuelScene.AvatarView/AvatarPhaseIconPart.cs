using UnityEngine;

namespace Wotc.Mtga.DuelScene.AvatarView;

public class AvatarPhaseIconPart : AvatarFramePart
{
	[SerializeField]
	private PhaseIconType _iconType;

	public PhaseIconType IconType => _iconType;
}
