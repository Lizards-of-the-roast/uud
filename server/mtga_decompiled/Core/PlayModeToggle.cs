using UnityEngine;

public class PlayModeToggle : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private TooltipTrigger _tooltip;

	[SerializeField]
	private GameObject _toggleEffects;

	private bool _allPlayModes;

	[HideInInspector]
	public HomePageContentController _homePageController;

	private void Awake()
	{
		_homePageController = Object.FindObjectOfType<HomePageContentController>();
	}

	private void OnEnable()
	{
		_allPlayModes = MDNPlayerPrefs.AllPlayModesToggle;
		RefreshPlayModesTooltip();
		SetupWithSelectedPlayMode();
	}

	private void RefreshPlayModesTooltip()
	{
		if (_allPlayModes)
		{
			_tooltip.LocString = "MainNav/homepage/PlayModeSliderOn";
		}
		else
		{
			_tooltip.LocString = "MainNav/homepage/PlayModeSliderOff";
		}
	}

	public void TogglePlayMode()
	{
		_allPlayModes = !_allPlayModes;
		MDNPlayerPrefs.AllPlayModesToggle = _allPlayModes;
		RefreshPlayModesTooltip();
		if (!MDNPlayerPrefs.PlayerHasToggledForTheFirstTime)
		{
			MDNPlayerPrefs.PlayerHasToggledForTheFirstTime = true;
		}
		SetupWithSelectedPlayMode();
	}

	private void SetupWithSelectedPlayMode()
	{
		_animator.SetBool("RightMode", _allPlayModes);
		if (MDNPlayerPrefs.PlayerHasToggledForTheFirstTime)
		{
			_toggleEffects.SetActive(value: false);
		}
	}
}
