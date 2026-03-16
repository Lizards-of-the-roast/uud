namespace Epic.OnlineServices.Ecom;

public class Entitlement
{
	public int ApiVersion => 2;

	public string Id { get; set; }

	public string InstanceId { get; set; }

	public string CatalogItemId { get; set; }

	public int ServerIndex { get; set; }

	public bool Redeemed { get; set; }

	public long EndTimestamp { get; set; }
}
