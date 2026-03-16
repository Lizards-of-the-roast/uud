using AssetLookupTree.Payloads.Ability.Metadata;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

public class BadgeEntryView : MonoBehaviour
{
	[SerializeField]
	protected Image _backgroundImage;

	[SerializeField]
	private Image _highlightImage;

	[SerializeField]
	protected Color _defaultColor = Color.grey;

	[SerializeField]
	protected Color _addedColor = Color.cyan;

	[SerializeField]
	protected Color _specialColor = Color.yellow;

	[SerializeField]
	protected Color _perpetualColor = Color.magenta;

	[SerializeField]
	protected Color _otherColor = Color.grey;

	[SerializeField]
	private Transform _vfxTransform;

	private BadgeEntrySubView[] _subViews;

	[Header("Badge Data Views")]
	[SerializeField]
	private IconView _iconView = new IconView();

	[SerializeField]
	private CountView _countView = new CountView();

	[SerializeField]
	private ThresholdView _thresholdView = new ThresholdView();

	[SerializeField]
	private DomainView _domainView = new DomainView();

	[SerializeField]
	private VisualCountView _visualCountView = new VisualCountView();

	[SerializeField]
	private VividView _vividView = new VividView();

	public Transform VfxTransform => _vfxTransform;

	public bool IsNumeric
	{
		get
		{
			if (!_countView.Active && !_thresholdView.Active && !_domainView.Active && !_visualCountView.Active)
			{
				return _vividView.Active;
			}
			return true;
		}
	}

	public virtual void Init(BadgeEntryStatus badgeEntryStatus, bool isActive = false)
	{
		if (_subViews == null)
		{
			_subViews = new BadgeEntrySubView[6] { _iconView, _countView, _thresholdView, _domainView, _visualCountView, _vividView };
		}
		if ((bool)_backgroundImage)
		{
			switch (badgeEntryStatus)
			{
			case BadgeEntryStatus.Normal:
				_backgroundImage.color = _defaultColor;
				break;
			case BadgeEntryStatus.Added:
				_backgroundImage.color = _addedColor;
				break;
			case BadgeEntryStatus.Special:
				_backgroundImage.color = _specialColor;
				break;
			case BadgeEntryStatus.Perpetual:
				_backgroundImage.color = _perpetualColor;
				break;
			case BadgeEntryStatus.Other:
				_backgroundImage.color = _otherColor;
				break;
			default:
				_backgroundImage.color = Color.magenta;
				break;
			}
		}
		if ((bool)_highlightImage)
		{
			_highlightImage.enabled = isActive;
		}
	}

	public virtual void Cleanup()
	{
		for (int i = 0; i < _vfxTransform.childCount; i++)
		{
			Transform child = _vfxTransform.GetChild(i);
			if ((bool)child)
			{
				SelfCleanup component = child.GetComponent<SelfCleanup>();
				if ((bool)component)
				{
					component.ImmediateCleanup();
				}
			}
		}
		if ((bool)_highlightImage)
		{
			_highlightImage.enabled = false;
		}
		BadgeEntrySubView[] subViews = _subViews;
		foreach (BadgeEntrySubView badgeEntrySubView in subViews)
		{
			if (badgeEntrySubView.Active)
			{
				badgeEntrySubView.Cleanup();
			}
		}
	}

	public void InitDataViews(IBadgeEntryData data, NumericBadgeCalculatorInput? numericInput = null, BadgeActivationCalculatorInput? activatorInput = null)
	{
		BadgeEntrySubView[] subViews = _subViews;
		foreach (BadgeEntrySubView badgeEntrySubView in subViews)
		{
			if (badgeEntrySubView.Active)
			{
				badgeEntrySubView.Init(data, numericInput, activatorInput);
			}
		}
	}
}
