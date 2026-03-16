using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Wizards.Arena.Models.Network;

namespace Core.Meta.Social.Tables;

public static class TableUtils
{
	public static Color TablesColorForPlayer(IAccountClient accountClient, LobbyPlayer player)
	{
		if (player?.PlayerId == accountClient?.AccountInformation?.PersonaID)
		{
			return new Color(0.77f, 0.93f, 0.98f);
		}
		MD5 mD = MD5.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(player?.PlayerId ?? "UnknownPlayer");
		byte[] array = mD.ComputeHash(bytes);
		float r = BytesToPastelColorFloat(MemoryExtensions.AsSpan(array).Slice(0, 4));
		float g = BytesToPastelColorFloat(MemoryExtensions.AsSpan(array).Slice(4, 4));
		float b = BytesToPastelColorFloat(MemoryExtensions.AsSpan(array).Slice(8, 4));
		return new Color(r, g, b);
	}

	private static float BytesToPastelColorFloat(ReadOnlySpan<byte> bytes)
	{
		System.Random random = new System.Random(BitConverter.ToInt32(bytes));
		return 0.8f + (float)random.NextDouble() * 0.2f;
	}

	public static bool HistoriesMatch(List<LobbyMessage> history, List<LobbyMessage> otherHistory)
	{
		if (history == null)
		{
			return otherHistory == null;
		}
		if (otherHistory == null)
		{
			return false;
		}
		return history.Zip(otherHistory, (LobbyMessage x, LobbyMessage y) => x.MessageId == y.MessageId).All((bool b) => b);
	}
}
