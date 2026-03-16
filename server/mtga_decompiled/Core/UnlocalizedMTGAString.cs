public class UnlocalizedMTGAString : MTGALocalizedString
{
	public UnlocalizedMTGAString()
	{
	}

	public UnlocalizedMTGAString(string key)
	{
		Key = key;
	}

	public override string ToString()
	{
		return Key;
	}
}
