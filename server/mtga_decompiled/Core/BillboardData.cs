using Wizards.MDN;
using Wizards.Unification.Models.Events;

public class BillboardData
{
	public EBillboardType BillboardType;

	public EventContext BillboardEvent;

	public ClientDynamicFilterTag BillboardDynamicFilterTag;

	public BillboardData(EBillboardType billboardType, EventContext billboardEvent, ClientDynamicFilterTag dynamicFilterTag = null)
	{
		BillboardType = billboardType;
		BillboardEvent = billboardEvent;
		BillboardDynamicFilterTag = dynamicFilterTag;
	}
}
