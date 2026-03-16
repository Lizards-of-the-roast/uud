using UnityEngine.Purchasing;

namespace Wizards.Mtga.Store;

public class IAPPurchaseRequest : PurchaseRequest
{
	private Order _order;

	public override string ProductId
	{
		get
		{
			if (Product == null)
			{
				return "Error";
			}
			return Product.definition.id;
		}
	}

	public override string Receipt => Order?.Info.Receipt;

	public override string IsoCurrencyCode => Product?.metadata?.isoCurrencyCode ?? "NA";

	public Product Product { get; internal set; }

	public Order Order { get; internal set; }

	public IAPPurchaseRequest(Product product)
	{
		Product = product;
		base.MessageKey = "Processing";
		base.State = OperationState.InProgress;
	}

	public IAPPurchaseRequest(Product product, Order order)
		: this(product)
	{
		SetOrder(order);
	}

	public static IAPPurchaseRequest Fail(string messageKey, bool isError)
	{
		IAPPurchaseRequest iAPPurchaseRequest = new IAPPurchaseRequest(null);
		iAPPurchaseRequest.SetFail(messageKey, isError);
		return iAPPurchaseRequest;
	}

	public void SetOrder(Order order)
	{
		Order = order;
		if (!(order is PendingOrder) && !(order is ConfirmedOrder))
		{
			if (!(order is FailedOrder))
			{
				if (order is DeferredOrder)
				{
					base.State = OperationState.InProgress;
				}
			}
			else
			{
				base.State = OperationState.Fail;
			}
		}
		else
		{
			base.State = OperationState.Success;
		}
	}
}
