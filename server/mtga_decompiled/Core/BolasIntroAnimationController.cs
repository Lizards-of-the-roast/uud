using UnityEngine;
using Wotc.Mtga.Loc;

public class BolasIntroAnimationController : MonoBehaviour
{
	[SerializeField]
	private Localize _locText;

	[LocTerm]
	[SerializeField]
	private string _term1;

	[LocTerm]
	[SerializeField]
	private string _term2;

	private void PlayBolasWelcomeDialog()
	{
		_locText.SetText(_term1);
		AudioManager.PlayAudio(WwiseEvents.vo_nicolbolas_sep_125_v3, AudioManager.Default);
	}

	private void PlayBolasSecondLine()
	{
		_locText.SetText(_term2);
		AudioManager.PlayAudio(WwiseEvents.vo_nicolbolas_sep_126, AudioManager.Default);
	}
}
