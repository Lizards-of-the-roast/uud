namespace Wotc.Mtga.Wrapper.Draft;

public static class TableDraftQueueFunctions
{
	public const string WAITING_PLAYER_LOC_KEY = "Draft/Draft_Waiting";

	public const string READY_PLAYER_LOC_KEY = "Draft/Draft_Ready";

	public const string LOCAL_PLAYER_READY_CONTEXT_KEY = "MainNav/General/Empty_String";

	public const string LOCAL_PLAYER_NOT_READY_CONTEXT_KEY = "MainNav/General/Empty_String";

	public static AnonymousSeatVisualData[] GetAnonymousSeatVisualDataArray(int numberOfFilledSeats, int totalNumberOfSeats)
	{
		AnonymousSeatVisualData[] array = new AnonymousSeatVisualData[totalNumberOfSeats];
		for (int i = 0; i < array.Length; i++)
		{
			if (i < numberOfFilledSeats)
			{
				array[i] = new AnonymousSeatVisualData(isVisible: true, "Draft/Draft_Waiting");
			}
			else
			{
				array[i] = new AnonymousSeatVisualData(isVisible: false);
			}
		}
		return array;
	}

	public static AnonymousSeatVisualData[] GetReadySeatVisualDataArray(AnonymousSeatVisualData[] seatVisualDataArray, int numberOfReadySeats, bool isLocalPlayerReady)
	{
		if (isLocalPlayerReady)
		{
			numberOfReadySeats--;
		}
		for (int i = 0; i < seatVisualDataArray.Length; i++)
		{
			if (i < numberOfReadySeats)
			{
				seatVisualDataArray[i] = new AnonymousSeatVisualData(isVisible: true, "Draft/Draft_Ready", isReady: true);
			}
		}
		return seatVisualDataArray;
	}

	public static KnownSeatVisualData GetKnownSeatVisualData(PlayerInSeat seat)
	{
		return new KnownSeatVisualData(statusKey: (!seat.IsReady) ? "Draft/Draft_Waiting" : "Draft/Draft_Ready", username: seat.DisplayName, avatarId: seat.AvatarId, isReady: seat.IsReady);
	}

	public static KnownSeatVisualData[] GetKnownSeatVisualDataArray(PlayerInSeat[] seats)
	{
		KnownSeatVisualData[] array = new KnownSeatVisualData[seats.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GetKnownSeatVisualData(seats[i]);
		}
		return array;
	}
}
