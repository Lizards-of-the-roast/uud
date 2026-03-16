public class PlayBladeHelper_Mobile : PlayBladeHelper_Base
{
	private PlayBladeController _playBlade;

	public override void Init(PlayBladeController playBlade)
	{
		base.Init(playBlade);
		_playBlade = playBlade;
		_playBlade.UnifiedChallengeBladeWidget.UEvOnShow.AddListener(OnShowFriendChallenge);
		_playBlade.DeckSelector.UEvOnBackgroundClicked.AddListener(OnClickBehindDeckSelector);
	}

	private void OnShowFriendChallenge()
	{
	}

	private void OnClickBehindDeckSelector()
	{
		_playBlade.Hide();
	}
}
