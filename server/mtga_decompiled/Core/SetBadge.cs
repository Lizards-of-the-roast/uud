using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Wrapper;

public class SetBadge : MonoBehaviour
{
	[SerializeField]
	private Image _setIcon;

	[SerializeField]
	private Image _completionMeter1x;

	[SerializeField]
	private Image _completionMeter4x;

	[SerializeField]
	private TMP_Text _percentageText;

	[SerializeField]
	private TooltipTrigger _tooltip;

	[SerializeField]
	private Image _alchemyIcon;

	public CollationMapping _expansionCode;

	public DateTime _releaseDate;

	private Action<SetBadge> _badgeOnClick;

	private Animator _animator;

	public bool _isStandard;

	public bool _isAlchemy;

	public bool _isHistoric;

	public bool _isUniversesBeyond;

	private static readonly int SELECTED_BOOL = Animator.StringToHash("Selected");

	private static readonly int COMPLETIONSTATE_INT = Animator.StringToHash("Completion");

	private static readonly int SET_TYPE_INT = Animator.StringToHash("SetType");

	public void Init(CollationMapping expansionCode, DateTime releaseDate, Action<SetBadge> badgeOnClick, bool isAlchemySet, bool isUniversesBeyondSet)
	{
		base.gameObject.SetActive(value: true);
		_animator = GetComponent<Animator>();
		_expansionCode = expansionCode;
		_releaseDate = releaseDate;
		_badgeOnClick = badgeOnClick;
		_isAlchemy = isAlchemySet;
		_isUniversesBeyond = isUniversesBeyondSet;
		string text = (isAlchemySet ? expansionCode.ToString().Replace('_', '-') : expansionCode.ToString());
		_tooltip.TooltipData.Text = "General/Sets/" + text;
		_animator.keepAnimatorStateOnDisable = true;
	}

	public void Init()
	{
		_animator = GetComponent<Animator>();
	}

	public void UpdateUI(Sprite setIcon, int numOwned, int numAvailable, bool useFourOf = false)
	{
		base.gameObject.SetActive(value: true);
		_setIcon.sprite = setIcon;
		if (!useFourOf)
		{
			_completionMeter1x.fillAmount = (float)numOwned / (float)numAvailable;
			if (numOwned > 0)
			{
				_animator.SetInteger(COMPLETIONSTATE_INT, 1);
			}
			if (numOwned >= numAvailable)
			{
				_animator.SetInteger(COMPLETIONSTATE_INT, 2);
			}
			_percentageText.text = $"{Math.Floor(_completionMeter1x.fillAmount * 100f)}%";
		}
		else
		{
			_completionMeter1x.fillAmount = 1f;
			_completionMeter4x.fillAmount = (float)numOwned / (float)numAvailable;
			_animator.SetInteger(COMPLETIONSTATE_INT, 2);
			if (numOwned >= numAvailable)
			{
				_animator.SetInteger(COMPLETIONSTATE_INT, 3);
			}
			_percentageText.text = $"{Math.Floor(_completionMeter4x.fillAmount * 100f)}%";
		}
	}

	public void SetSelected(bool selected)
	{
		_animator.SetBool(SELECTED_BOOL, selected);
	}

	public void SetAlchemyIcon(Sprite alchemyIcon)
	{
		_alchemyIcon.sprite = alchemyIcon;
		_animator.SetInteger(SET_TYPE_INT, 1);
	}

	public void OnClick()
	{
		_badgeOnClick?.Invoke(this);
	}
}
