using System;
using HasbroGo.Social.Models;

namespace HasbroGo;

public class UpdatePresenceEventArgs : EventArgs
{
	public Presence Presence { get; private set; }

	public UpdatePresenceEventArgs(Presence presence)
	{
		Presence = presence;
	}
}
