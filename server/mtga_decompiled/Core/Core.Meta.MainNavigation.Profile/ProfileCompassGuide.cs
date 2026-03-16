using ProfileUI;

namespace Core.Meta.MainNavigation.Profile;

public class ProfileCompassGuide : WrapperCompassGuide
{
	private ProfileScreenModeEnum _screenMode;

	private RankType _rankType;

	public ProfileScreenModeEnum ScreenMode => _screenMode;

	public RankType RankType => _rankType;

	public ProfileCompassGuide(ProfileScreenModeEnum screenMode, RankType rankType)
	{
		_screenMode = screenMode;
		_rankType = rankType;
	}
}
