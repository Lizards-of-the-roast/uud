using Wotc.Mtgo.Gre.External.Messaging;

public static class CounterTypeUtil
{
	public static bool IsKeywordCounter(CounterType counterType)
	{
		if (counterType != CounterType.Deathtouch && counterType != CounterType.FirstStrike && counterType != CounterType.Flying && counterType != CounterType.Hexproof && counterType != CounterType.Lifelink && counterType != CounterType.Menace && counterType != CounterType.Reach && counterType != CounterType.Trample && counterType != CounterType.Vigilance && counterType != CounterType.Indestructible && counterType != CounterType.Exalted)
		{
			return counterType == CounterType.DoubleStrike;
		}
		return true;
	}
}
