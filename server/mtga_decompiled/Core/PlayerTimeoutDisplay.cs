using TMPro;
using UnityEngine;

public class PlayerTimeoutDisplay : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _timeoutCountText;

	[SerializeField]
	private Animation _animationComponent;

	private uint _prvTimeoutCount;

	public void SetTimeoutCount(uint timeoutCount)
	{
		_timeoutCountText.text = string.Format("x{0}", timeoutCount.ToString("N0"));
		if (_prvTimeoutCount < timeoutCount && !_animationComponent.isPlaying)
		{
			_animationComponent.Play();
		}
		_prvTimeoutCount = timeoutCount;
	}
}
