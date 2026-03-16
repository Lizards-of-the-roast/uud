using System;
using System.IO;
using Newtonsoft.Json;
using Wizards.Mtga.IO;

namespace Wotc.Mtga.TimedReplays;

public class ReplayReader : IDisposable
{
	public class Result
	{
		public enum ResultTypes
		{
			Success,
			DeserializeException,
			EndOfStream
		}

		public ResultTypes ResultType;

		public string FailMessage;
	}

	private readonly StreamReader _stream;

	private readonly CosmeticReplayData _cosmetics;

	private readonly ReplayMessageFilter _filter;

	private ReplayEntry _currentMessage;

	private static Result SuccessResult = new Result
	{
		ResultType = Result.ResultTypes.Success
	};

	public int LineCount { get; private set; }

	public static Result TryCreateReplayFromPath(string file, out ReplayReader replayReader, ReplayMessageFilter filter = null)
	{
		return TryCreateReplay(new StreamReader(WindowsSafePath.OpenFile(file, FileMode.Open, FileAccess.Read, FileShare.Read)), out replayReader, filter);
	}

	public static Result TryCreateReplay(StreamReader stream, out ReplayReader replayReader, ReplayMessageFilter filter = null)
	{
		CosmeticReplayData cosmetics = null;
		int num = 0;
		if (stream.Peek() == 35)
		{
			string text = stream.ReadLine();
			num++;
			if (!(text == ReplayWriter.VERSION_2))
			{
				replayReader = null;
				return new Result
				{
					ResultType = Result.ResultTypes.DeserializeException,
					FailMessage = "Unrecognized format"
				};
			}
			string text2 = stream.ReadLine();
			num++;
			if (text2 != null)
			{
				cosmetics = JsonConvert.DeserializeObject<CosmeticReplayData>(text2);
			}
		}
		replayReader = new ReplayReader(stream, num, cosmetics, filter);
		return replayReader.TryMoveToNextMessage();
	}

	private ReplayReader(StreamReader stream, int startingLineCount = 0, CosmeticReplayData cosmetics = null, ReplayMessageFilter filter = null)
	{
		LineCount = startingLineCount;
		_cosmetics = cosmetics;
		_stream = stream;
		_filter = filter;
	}

	public (PlayerCosmetics local, PlayerCosmetics opp) GetPlayerInfo()
	{
		return (local: _cosmetics?.Local ?? new PlayerCosmetics(), opp: _cosmetics?.Opponent ?? new PlayerCosmetics());
	}

	public string GetBattlefield()
	{
		return _cosmetics?.BattlefieldId;
	}

	public Result TryMoveToNextMessage()
	{
		_currentMessage = null;
		while (_currentMessage == null)
		{
			if (_stream.EndOfStream)
			{
				return new Result
				{
					ResultType = Result.ResultTypes.EndOfStream
				};
			}
			string text = _stream.ReadLine();
			LineCount++;
			if (!string.IsNullOrEmpty(text))
			{
				ReplayEntry replayEntry = null;
				try
				{
					replayEntry = ReplayEntry.Deserialize(text);
				}
				catch (Exception ex)
				{
					return new Result
					{
						ResultType = Result.ResultTypes.DeserializeException,
						FailMessage = ex.Message
					};
				}
				ReplayMessageFilter filter = _filter;
				if (filter == null || !filter.ShouldIgnore(replayEntry))
				{
					_currentMessage = replayEntry;
				}
			}
		}
		return SuccessResult;
	}

	public ReplayEntry GetCurrentMessage()
	{
		return _currentMessage;
	}

	public bool IsPassedEnd()
	{
		if (_currentMessage == null)
		{
			return _stream.EndOfStream;
		}
		return false;
	}

	public void Dispose()
	{
		_stream.Dispose();
	}
}
