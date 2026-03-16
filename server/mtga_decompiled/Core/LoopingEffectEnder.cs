using UnityEngine;
using Wotc.Mtga.DuelScene.VFX;

public class LoopingEffectEnder : MonoBehaviour
{
	[SerializeField]
	private string _loopingKey = string.Empty;

	private void OnEnable()
	{
		EndLoop();
	}

	private void OnTransformParentChanged()
	{
		EndLoop();
	}

	private void EndLoop()
	{
		if (base.gameObject.activeSelf && !string.IsNullOrWhiteSpace(_loopingKey) && (bool)base.transform && (bool)base.transform.parent)
		{
			LoopingAnimationManager.RemoveLoopingEffect(base.transform.parent, _loopingKey);
		}
	}
}
