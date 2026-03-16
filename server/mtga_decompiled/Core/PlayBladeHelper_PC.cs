public class PlayBladeHelper_PC : PlayBladeHelper_Base
{
	private PlayBladeController _playBlade;

	public override void Init(PlayBladeController playBlade)
	{
		base.Init(playBlade);
		_playBlade = playBlade;
	}

	private void OnEventTileClicked()
	{
		_playBlade.HideDeckSelector();
	}
}
