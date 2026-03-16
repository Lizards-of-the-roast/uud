using UnityEngine;

namespace Wotc.Mtga.Duel;

public class TimerViewAndTimeoutViewReferences : MonoBehaviour
{
	[SerializeField]
	public MatchTimer LocalTimerView;

	[SerializeField]
	public PlayerTimeoutDisplay LocalTimeoutView;

	[SerializeField]
	public MatchTimer OpponentTimerView;

	[SerializeField]
	public PlayerTimeoutDisplay OpponentTimeoutView;
}
