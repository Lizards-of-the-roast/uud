using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class AvatarPhaseIcon : PhaseLadderButton
{
	[SerializeField]
	private GameObject _fillObj;

	[SerializeField]
	private GameObject _stopObj;

	protected override void SetActive(bool active)
	{
		_fillObj.UpdateActive(active);
	}

	public override void SetStop(SettingScope stopScope, SettingStatus stopState)
	{
		base.SetStop(stopScope, stopState);
		_stopObj.UpdateActive(stopState == SettingStatus.Set);
	}

	protected override void SetHighlight(bool highlight)
	{
		if (_animator != null && _animator.isActiveAndEnabled)
		{
			_animator.SetBool("Highlight", highlight);
		}
	}
}
