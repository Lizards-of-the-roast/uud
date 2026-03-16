using DG.Tweening;
using UnityEngine;

public class StripController : MonoBehaviour
{
	public static float FadeTime = 0.7f;

	public void ShowUp()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			componentsInChildren[0].DOFade(1f, FadeTime);
		}
	}

	public void PeaceOut()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			componentsInChildren[0].DOFade(0f, FadeTime);
		}
	}
}
