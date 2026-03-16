using System;
using System.Collections.Generic;
using Wotc.Mtga.DuelScene;
using Wotc.Mtgo.Gre.External.Messaging;

[Serializable]
public struct PlayWindows
{
	[Serializable]
	public struct PlayWindow
	{
		public TurnInformation.ActivePlayer activePlayer;

		public Phase phase;

		public Step step;
	}

	public List<PlayWindow> playWindowList;
}
