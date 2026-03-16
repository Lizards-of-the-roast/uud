using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class NPEPrompt : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Localize _locText;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private Animator _animator;

	private static readonly int Outro = Animator.StringToHash("Outro");

	private static readonly int IntroFade = Animator.StringToHash("IntroFade");

	private static readonly int IntroPop = Animator.StringToHash("IntroPop");

	public GameManager InjectedGameManager { private get; set; }

	public virtual void ShowPop(MTGALocalizedString text)
	{
		base.gameObject.SetActive(value: false);
		_locText.SetText(text);
		_canvasGroup.interactable = true;
		_canvasGroup.blocksRaycasts = true;
		base.gameObject.SetActive(value: true);
		_animator.SetTrigger(IntroPop);
	}

	public virtual void ShowFade(MTGALocalizedString text)
	{
		base.gameObject.SetActive(value: false);
		_locText.SetText(text);
		_canvasGroup.interactable = true;
		_canvasGroup.blocksRaycasts = true;
		base.gameObject.SetActive(value: true);
		_animator.SetTriggerIfContains(IntroFade);
	}

	public virtual void Hide()
	{
		_canvasGroup.interactable = false;
		_canvasGroup.blocksRaycasts = false;
		if (_animator.isActiveAndEnabled)
		{
			_animator.SetTrigger(Outro);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Hide();
		if (InjectedGameManager != null)
		{
			InjectedGameManager.NpeDirector?.ClearNPEUXPrompts();
		}
	}

	private void OnDestroy()
	{
		InjectedGameManager = null;
	}
}
