using System;
using AssetLookupTree;
using UnityEngine;
using Wotc.Mtga.Loc;

public class EmoteView : MonoBehaviour
{
	[SerializeField]
	private string _emoteId;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Localize _localizeComponent;

	[SerializeField]
	private CustomButton _emoteButton;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	private readonly int CLICK_ANIM_PARAMETER = Animator.StringToHash("Pressed");

	private readonly int HOVER_ANIM_PARAMETER = Animator.StringToHash("Highlighted");

	private readonly int EQUIPPED_ANIM_PARAMETER = Animator.StringToHash("Equipped");

	private readonly int ISDISPLAYONLY_ANIM_PARAMETER = Animator.StringToHash("DisplayOnly");

	private readonly int ISFLIPPED_ANIM_PARAMETER = Animator.StringToHash("Flipped");

	private readonly int FADEIN_ANIM_PARAMETER = Animator.StringToHash("FadeIn");

	private readonly int FADEOUT_ANIM_PARAMETER = Animator.StringToHash("FadeOut");

	private readonly int FIRST_FRAME_SKIP_PARAMETER = Animator.StringToHash("FirstFrameSkip");

	private SfxData _sfxData;

	private string _emoteText;

	private bool _isEquipped;

	public string Id => _emoteId;

	public event Action<string> OnClick;

	public event Action<string> OnHover;

	private void Awake()
	{
		_emoteButton.OnClick.AddListener(_onclick);
		_emoteButton.OnMouseover.AddListener(_onMouseOn);
		_emoteButton.OnMouseoff.AddListener(_onMouseOff);
	}

	private void OnEnable()
	{
		SetEquipped(_isEquipped);
	}

	private void OnDestroy()
	{
		if (_sfxData != null)
		{
			if ((bool)base.gameObject && !_sfxData.isGlobalOnly())
			{
				AudioManager.StopSFX(base.gameObject);
			}
			_sfxData = null;
		}
		_emoteButton.OnClick.RemoveListener(_onclick);
		_emoteButton.OnMouseover.RemoveListener(_onMouseOn);
		_emoteButton.OnMouseoff.RemoveListener(_onMouseOff);
	}

	public void Init(string emoteId, string locKey = "", SfxData sfxData = null)
	{
		_emoteId = emoteId;
		_sfxData = sfxData;
		SetLocalizationKey(locKey);
		_emoteButton.enabled = true;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetSfx(SfxData sfxData)
	{
		_sfxData = sfxData;
	}

	public void SetLocalizationKey(string locKey)
	{
		if (!(_localizeComponent == null))
		{
			_emoteText = (string.IsNullOrWhiteSpace(locKey) ? "MainNav/General/Empty_String" : locKey);
			_localizeComponent.SetText(_emoteText);
		}
	}

	public void SetLocalizedText(string localizedString)
	{
		if (!(_localizeComponent == null))
		{
			_emoteText = localizedString;
			_localizeComponent.enabled = false;
			_localizeComponent.TextTarget.OverrideText(_emoteText);
		}
	}

	public void SetScale(Vector3 scale)
	{
		base.transform.localScale = scale;
	}

	public void SetEquipped(bool isEquipped)
	{
		_isEquipped = isEquipped;
		_animator.SetBool(EQUIPPED_ANIM_PARAMETER, _isEquipped);
	}

	public void SetDisplayOnly(bool isDisplayOnly, bool isFlipped = false)
	{
		SetEquipped(isEquipped: true);
		_animator.SetBool(EQUIPPED_ANIM_PARAMETER, value: true);
		_animator.SetBool(ISDISPLAYONLY_ANIM_PARAMETER, isDisplayOnly);
		_animator.SetBool(ISFLIPPED_ANIM_PARAMETER, isFlipped);
	}

	public void ProfileSkipFirstFrameFade()
	{
		_animator.SetTrigger(FIRST_FRAME_SKIP_PARAMETER);
	}

	public void SetClickable(bool isClickable)
	{
		_emoteButton.OnClick.RemoveListener(_onclick);
		if (isClickable)
		{
			_emoteButton.OnClick.AddListener(_onclick);
		}
	}

	public void SetHoverable(bool isHoverable)
	{
		_emoteButton.OnMouseover.RemoveListener(_onMouseOn);
		_emoteButton.OnMouseoff.RemoveListener(_onMouseOff);
		if (isHoverable)
		{
			_emoteButton.OnMouseover.AddListener(_onMouseOn);
			_emoteButton.OnMouseoff.AddListener(_onMouseOff);
		}
	}

	public void PlaySfx()
	{
		AudioManager.StopSFX(base.gameObject);
		if (_sfxData != null)
		{
			AudioManager.PlayAudio(_sfxData.AudioEvents, base.gameObject);
		}
	}

	public void FadeIn()
	{
		_animator.SetTrigger(FADEIN_ANIM_PARAMETER);
		PlaySfx();
	}

	public void FadeOut()
	{
		_animator.SetTrigger(FADEOUT_ANIM_PARAMETER);
	}

	private void _onclick()
	{
		_animator.SetTrigger(CLICK_ANIM_PARAMETER);
		this.OnClick?.Invoke(_emoteId);
	}

	private void _onMouseOn()
	{
		_animator.SetBool(HOVER_ANIM_PARAMETER, value: true);
		this.OnHover?.Invoke(_emoteId);
	}

	private void _onMouseOff()
	{
		_animator.SetBool(HOVER_ANIM_PARAMETER, value: false);
	}
}
