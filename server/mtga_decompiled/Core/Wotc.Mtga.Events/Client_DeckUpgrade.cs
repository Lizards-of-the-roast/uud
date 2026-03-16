using System;
using System.Collections.Generic;
using Wizards.Unification.Models.Graph;

namespace Wotc.Mtga.Events;

public class Client_DeckUpgrade
{
	public Guid DeckId;

	public string DeckDescription;

	public List<uint> CardsAdded;

	public List<uint> CardsToRemove;

	public Client_DeckUpgrade(DTO_UpgradePacketConfig awsPacket)
	{
		DeckId = awsPacket.DeckId;
		CardsAdded = awsPacket.CardsAdded;
		CardsToRemove = awsPacket.CardsRemoved;
	}
}
