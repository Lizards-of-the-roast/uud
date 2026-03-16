using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace Wizards.Mtga.Store;

public class IAPCatalogRequest : CatalogRequest
{
	public List<Product> products;

	public IAPCatalogRequest()
	{
		base.MessageKey = "Processing";
		base.State = OperationState.InProgress;
	}
}
