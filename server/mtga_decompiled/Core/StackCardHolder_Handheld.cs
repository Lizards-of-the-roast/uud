using UnityEngine;
using UnityEngine.UI;

public class StackCardHolder_Handheld : StackCardHolder
{
	[SerializeField]
	private Button _undockButton;

	[SerializeField]
	private Animator _canvasAnimator;

	[SerializeField]
	private float _overrideOverlapRotation = -5f;

	protected override void Awake()
	{
		_undockButton.onClick.AddListener(base.OnDockButtonClicked);
		_undockButton.gameObject.SetActive(value: false);
		_undockButton.transform.localPosition = _endPosition;
		base.Awake();
		_fanLayout.OverlapRotation = _overrideOverlapRotation;
		base.Layout = _fanLayout;
	}

	protected override void OnDestroy()
	{
		_undockButton.onClick.RemoveAllListeners();
		base.OnDestroy();
	}

	public override void Dock(DockStatus dockStatus)
	{
		_dockStatus = dockStatus;
		_canvasAnimator.SetBool("Docked", base.IsDocked);
		base.Dock(dockStatus);
	}

	protected override void OnPreLayout()
	{
		if (base.CardViews.Count == 0)
		{
			_undockButton.gameObject.SetActive(value: false);
		}
		else
		{
			_undockButton.gameObject.SetActive(base.IsDocked);
			_canvasAnimator.SetBool("Docked", base.IsDocked);
		}
		base.OnPreLayout();
	}

	public override void OnBrowserHidden(BrowserBase browser)
	{
		Dock(base.IsDocked ? DockStatus.Docked : DockStatus.NotDocked);
		base.OnBrowserHidden(browser);
	}
}
