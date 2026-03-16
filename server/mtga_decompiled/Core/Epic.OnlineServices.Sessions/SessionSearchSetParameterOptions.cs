namespace Epic.OnlineServices.Sessions;

public class SessionSearchSetParameterOptions
{
	public int ApiVersion => 1;

	public AttributeData Parameter { get; set; }

	public OnlineComparisonOp ComparisonOp { get; set; }
}
