using System;

namespace Core.Meta.MainNavigation;

public class HomePageCompassGuide : WrapperCompassGuide
{
	private readonly PostMatchContext _postMatchContext;

	private readonly PlayBladeController.PlayBladeVisualStates _initialBladeState;

	private readonly Guid _challengeId;

	private readonly bool _openVault;

	private readonly bool _cameFromNPE;

	private readonly string _afkDraft;

	private readonly bool _forceReload;

	public PostMatchContext PostMatchContext => _postMatchContext;

	public PlayBladeController.PlayBladeVisualStates InitialBladeState => _initialBladeState;

	public Guid ChallengeId => _challengeId;

	public bool OpenVault => _openVault;

	public bool CameFromNpe => _cameFromNPE;

	public string AfkDraft => _afkDraft;

	public bool ForceReload => _forceReload;

	public HomePageCompassGuide(PostMatchContext postMatchContext = null, PlayBladeController.PlayBladeVisualStates initialBladeState = PlayBladeController.PlayBladeVisualStates.Hidden, Guid challengeId = default(Guid), bool openVault = false, bool cameFromNpe = false, string afkDraft = null, bool forceReload = false)
	{
		_postMatchContext = postMatchContext;
		_initialBladeState = initialBladeState;
		_challengeId = challengeId;
		_openVault = openVault;
		_cameFromNPE = cameFromNpe;
		_afkDraft = afkDraft;
		_forceReload = forceReload;
	}

	public HomePageCompassGuide(HomePageContext homePageContext)
	{
		_postMatchContext = homePageContext.PostMatchContext;
		_initialBladeState = homePageContext.InitialBladeState;
		_challengeId = homePageContext.ChallengeId;
		_openVault = homePageContext.OpenVault;
		_cameFromNPE = homePageContext.CameFromNPE;
		_afkDraft = homePageContext.AFKDraft;
		_forceReload = homePageContext.ForceReload;
	}

	public static implicit operator HomePageContext(HomePageCompassGuide compassGuide)
	{
		if (compassGuide == null)
		{
			return null;
		}
		return new HomePageContext
		{
			PostMatchContext = compassGuide._postMatchContext,
			InitialBladeState = compassGuide._initialBladeState,
			ChallengeId = compassGuide._challengeId,
			OpenVault = compassGuide.OpenVault,
			CameFromNPE = compassGuide._cameFromNPE,
			AFKDraft = compassGuide._afkDraft,
			ForceReload = compassGuide._forceReload
		};
	}
}
