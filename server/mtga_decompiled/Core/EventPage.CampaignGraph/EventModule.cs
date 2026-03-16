using AssetLookupTree;
using Core.Code.Input;
using MTGA.KeyboardManager;
using UnityEngine;
using Wizards.MDN;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

namespace EventPage.CampaignGraph;

public abstract class EventModule : MonoBehaviour
{
	protected EventTemplate _parentTemplate;

	protected KeyboardManager _keyboardManager;

	protected IActionSystem _actionSystem;

	protected AssetLookupSystem _assetLookupSystem;

	protected CardDatabase _cardDatabase;

	protected CardViewBuilder _cardViewBuilder;

	protected CardMaterialBuilder _cardMaterialBuilder;

	protected Animator _transitionAnimator;

	private static readonly int Outro = Animator.StringToHash("Outro");

	private static readonly int Intro = Animator.StringToHash("Intro");

	protected virtual Animator Animator
	{
		get
		{
			if (_transitionAnimator == null)
			{
				_transitionAnimator = base.gameObject.GetComponent<Animator>();
			}
			return _transitionAnimator;
		}
	}

	protected EventContext EventContext => _parentTemplate.EventContext;

	public virtual void Init(EventTemplate parentTemplate, KeyboardManager keyboardManager, IActionSystem actionSystem, AssetLookupSystem assetLookupSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_cardMaterialBuilder = cardViewBuilder.CardMaterialBuilder;
		_parentTemplate = parentTemplate;
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		_assetLookupSystem = assetLookupSystem;
	}

	public abstract void Show();

	public abstract void UpdateModule();

	public virtual void LateUpdateModule()
	{
	}

	public abstract void Hide();

	public virtual void PlayAnimation(EventTemplateAnimation anim)
	{
		if (!(Animator != null) || !Animator.isActiveAndEnabled)
		{
			return;
		}
		switch (anim)
		{
		case EventTemplateAnimation.ModuleIntro:
			if (Animator.ContainsParameter(Intro))
			{
				Animator.SetTrigger(Intro);
			}
			break;
		case EventTemplateAnimation.ModuleOutro:
			if (Animator.ContainsParameter(Outro))
			{
				Animator.SetTrigger(Outro);
			}
			break;
		}
	}
}
