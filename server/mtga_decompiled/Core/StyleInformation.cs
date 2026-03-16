public readonly struct StyleInformation
{
	public readonly string StyleCode;

	public readonly bool IsOwnedStyle;

	public StyleInformation(string styleCode, bool isOwnedStyle)
	{
		StyleCode = styleCode;
		IsOwnedStyle = isOwnedStyle;
	}
}
