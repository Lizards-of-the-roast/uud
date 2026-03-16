public readonly struct CardDeckUsage
{
	public readonly uint Id;

	public readonly string StyleCode;

	public readonly uint QuantityAvailable;

	public readonly uint QuantityUsed;

	public CardDeckUsage(uint id, string styleCode, uint quantityAvailable, uint quantityUsed)
	{
		Id = id;
		StyleCode = styleCode;
		QuantityAvailable = quantityAvailable;
		QuantityUsed = quantityUsed;
	}

	public void Deconstruct(out uint id, out string styleCode, out uint available, out uint used)
	{
		id = Id;
		styleCode = StyleCode;
		available = QuantityAvailable;
		used = QuantityUsed;
	}
}
