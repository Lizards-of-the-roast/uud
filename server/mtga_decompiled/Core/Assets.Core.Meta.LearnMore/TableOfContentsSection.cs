using System;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.LearnMore;
using Wotc.Mtga.Loc;

namespace Assets.Core.Meta.LearnMore;

public class TableOfContentsSection : MonoBehaviour
{
	private static readonly int DisplayStateHash = Animator.StringToHash("DisplayState");

	[SerializeField]
	private Localize title;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private CustomButton button;

	[SerializeField]
	private LearnToPlayClickIntent buttonClickIntent;

	[SerializeField]
	private GameObject childAnchor;

	[SerializeField]
	private GameObject unreadIndicator;

	[SerializeField]
	private Image sectionIcon;

	[SerializeField]
	private LearnMoreSection section;

	private bool _displayStateOn;

	private bool _showNewFlag;

	private RectTransformCopyInfo? _originalTransform;

	private bool _initialized;

	private string _sectionId = string.Empty;

	public GameObject ChildAnchor => childAnchor;

	public bool ShowNewFlag
	{
		get
		{
			return _showNewFlag;
		}
		set
		{
			if (unreadIndicator != null)
			{
				unreadIndicator.SetActive(value);
			}
			_showNewFlag = value;
		}
	}

	public event Action<string, LearnToPlayClickIntent> Clicked;

	private void Awake()
	{
		if (button != null)
		{
			button.OnClick.AddListener(OnClick);
		}
	}

	private void OnClick()
	{
		this.Clicked?.Invoke(_sectionId, buttonClickIntent);
	}

	public void Init(SectionObjectReferences refs)
	{
		if (_initialized)
		{
			Debug.LogError("LTP: " + refs.Path + ": TOC.Init: already initialized");
			return;
		}
		RectTransform component = GetComponent<RectTransform>();
		if (!_originalTransform.HasValue)
		{
			_originalTransform = RectTransformCopyInfo.FromTransform(component);
		}
		else
		{
			_originalTransform.Value.ApplyToTransform(component);
		}
		_initialized = true;
		_sectionId = refs.Id;
		section = refs.SectionInfo;
		if (title != null)
		{
			title.SetText(section.Title, null, section.Title);
		}
		if (sectionIcon != null)
		{
			sectionIcon.sprite = section.Icon;
		}
		ShowNewFlag = refs.ShowNewFlag;
		base.gameObject.SetActive(refs.Ancestors.Length < 2);
	}

	public void SetDisplayState(bool on)
	{
		if (_displayStateOn != on)
		{
			_displayStateOn = on;
			if (_animator != null)
			{
				int value = (_displayStateOn ? 1 : 0);
				_animator.SetInteger(DisplayStateHash, value);
			}
		}
	}

	public void DeInit()
	{
		_sectionId = string.Empty;
		_initialized = false;
		this.Clicked = null;
	}

	private void OnDestroy()
	{
		DeInit();
		if (button != null)
		{
			button.OnClick.RemoveAllListeners();
		}
	}
}
