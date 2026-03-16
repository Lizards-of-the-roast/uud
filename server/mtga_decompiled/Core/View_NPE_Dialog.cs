using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class View_NPE_Dialog : MonoBehaviour
{
	[SerializeField]
	public CanvasGroup Container;

	[SerializeField]
	public Image Portrait;

	[SerializeField]
	public View_NPE_TextWidget DialogBox;

	public void Show(string text, bool immediate = false)
	{
		float duration = (immediate ? 0f : 1f);
		Container.gameObject.SetActive(value: true);
		Container.DOKill();
		Container.DOFade(1f, duration);
		DialogBox.Show(text);
	}

	public void Hide(bool immediate = false)
	{
		float duration = (immediate ? 0f : 1f);
		Container.DOKill();
		Container.DOFade(0f, duration);
	}
}
