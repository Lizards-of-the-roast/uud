using DG.Tweening;
using UnityEngine;
using Wotc.Mtga.Extensions;

public class NPE_Card_Augment : MonoBehaviour
{
	[Space(10f)]
	[Tooltip("Relative position of Power Hanger on Hover")]
	[SerializeField]
	public Vector3 InHover_PositionOffsetFromPT;

	[Tooltip("Scale of Power Hanger on Hover")]
	[SerializeField]
	public Vector3 InHover_Scale;

	[Tooltip("Relative position of Power Hanger on Battlefield")]
	[SerializeField]
	public float InHover_FadeInTime;

	[Space(10f)]
	[SerializeField]
	public Vector3 OnBattlefield_PositionOffsetFromPT;

	[Tooltip("Scale of Power Hanger on Battlefield")]
	[SerializeField]
	public Vector3 OnBattlefield_Scale;

	[SerializeField]
	public float OnBattlefield_FadeTime;

	private const string CARDS_EXAMINE_LAYER_NAME = "CardsExamine";

	public void ShowUp_Hover(Transform t)
	{
		base.transform.parent = t;
		base.transform.ZeroOut();
		base.transform.localScale = InHover_Scale;
		base.transform.localPosition += InHover_PositionOffsetFromPT;
		base.gameObject.SetLayer(LayerMask.NameToLayer("CardsExamine"));
		base.gameObject.SetActive(value: true);
		FadeInHanger(InHover_FadeInTime);
	}

	public void CleanUp_Hover()
	{
		base.transform.parent = null;
		base.transform.ZeroOut();
		base.gameObject.SetActive(value: false);
	}

	public void ShowUp_OnBattlefield(Transform t)
	{
		base.transform.parent = t;
		base.transform.ZeroOut();
		base.transform.localScale = OnBattlefield_Scale;
		base.transform.localPosition += OnBattlefield_PositionOffsetFromPT;
		base.gameObject.SetLayer(LayerMask.NameToLayer("CardsExamine"));
		FadeInHanger(OnBattlefield_FadeTime);
	}

	public float Emphasize()
	{
		float num = 1.5f;
		base.gameObject.transform.DOPunchScale(OnBattlefield_Scale * 2f, num, 3, 0f);
		return num;
	}

	private void FadeInHanger(float time)
	{
		CanvasGroup[] componentsInChildren = GetComponentsInChildren<CanvasGroup>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			componentsInChildren[0].alpha = 0f;
			componentsInChildren[0].DOFade(1f, time);
		}
	}

	public void FadeOut_OnBattlefield()
	{
		CanvasGroup[] componentsInChildren = GetComponentsInChildren<CanvasGroup>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			componentsInChildren[0].alpha = 1f;
			componentsInChildren[0].DOFade(0f, OnBattlefield_FadeTime);
		}
		Object.Destroy(base.gameObject, OnBattlefield_FadeTime);
	}
}
