public readonly struct Token
{
	public readonly TokenType Type;

	public readonly string Value;

	public Token(TokenType type, string value = null)
	{
		Type = type;
		Value = value;
	}
}
