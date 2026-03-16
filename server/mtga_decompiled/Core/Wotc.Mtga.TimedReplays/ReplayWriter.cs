using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Wizards.Mtga.IO;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.TimedReplays;

public class ReplayWriter : IDisposable
{
	public static readonly string VERSION_2 = "#Version2";

	private readonly StreamWriter _writer;

	private readonly Stopwatch _stopwatch = new Stopwatch();

	public static ReplayWriter CreateFromPath(string path, MatchManager.PlayerInfo local, MatchManager.PlayerInfo opp, string battlefieldId)
	{
		return new ReplayWriter(new StreamWriter(WindowsSafePath.OpenFile(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read | FileShare.Delete)), local, opp, battlefieldId);
	}

	public ReplayWriter(StreamWriter writer, MatchManager.PlayerInfo local, MatchManager.PlayerInfo opp, string battlefieldId)
	{
		_stopwatch.Start();
		_writer = writer;
		CosmeticReplayData value = new CosmeticReplayData
		{
			Local = new PlayerCosmetics(local),
			Opponent = new PlayerCosmetics(opp),
			BattlefieldId = battlefieldId
		};
		_writer.WriteLine(VERSION_2);
		_writer.WriteLine(JsonConvert.SerializeObject(value));
		_writer.Flush();
	}

	public ReplayWriter(StreamWriter writer)
	{
		_stopwatch.Start();
		_writer = writer;
	}

	public void WriteMessage(GREToClientMessage msg)
	{
		ReplayEntry replayEntry = new ReplayEntry(msg, _stopwatch.ElapsedMilliseconds);
		_writer.WriteLine(replayEntry.Serialize());
		_writer.Flush();
	}

	public void WriteMessage(ClientToGREMessage msg)
	{
		ReplayEntry replayEntry = new ReplayEntry(msg, _stopwatch.ElapsedMilliseconds);
		_writer.WriteLine(replayEntry.Serialize());
		_writer.Flush();
	}

	public void Dispose()
	{
		_writer.Dispose();
	}
}
