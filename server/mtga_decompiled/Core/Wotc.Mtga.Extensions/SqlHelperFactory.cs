namespace Wotc.Mtga.Extensions;

public static class SqlHelperFactory
{
	public static ISqlHelper Create()
	{
		return new UnitySqlHelper();
	}
}
