using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ButtonPhaseIcon : PhaseLadderButton
{
	public GameObject PlayerStopIcon;

	public GameObject OpponentStopIcon;

	protected override void SetActive(bool active)
	{
		if (_animator != null && _animator.isActiveAndEnabled)
		{
			_animator.SetBool("Active", active);
		}
	}

	protected override void SetHighlight(bool highlight)
	{
		if (_animator != null && _animator.isActiveAndEnabled)
		{
			_animator.SetBool("Highlight", highlight);
		}
	}

	public override void SetActivePlayer(GREPlayerNum activePlayer)
	{
		base.SetActivePlayer(activePlayer);
		if (_animator != null && _animator.isActiveAndEnabled)
		{
			_animator.SetBool("Blue", activePlayer == GREPlayerNum.Opponent);
		}
	}

	public override void SetStop(SettingScope stopScope, SettingStatus stopState)
	{
		base.SetStop(stopScope, stopState);
		if (stopState == SettingStatus.None || stopState == SettingStatus.Clear)
		{
			PlayerStopIcon.UpdateActive(active: false);
			OpponentStopIcon.UpdateActive(active: false);
			return;
		}
		switch (stopScope)
		{
		case SettingScope.AnyPlayer:
		case SettingScope.Opponents:
			PlayerStopIcon.UpdateActive(active: false);
			OpponentStopIcon.UpdateActive(active: true);
			break;
		case SettingScope.Team:
			PlayerStopIcon.UpdateActive(active: true);
			OpponentStopIcon.UpdateActive(active: false);
			break;
		}
	}
}
