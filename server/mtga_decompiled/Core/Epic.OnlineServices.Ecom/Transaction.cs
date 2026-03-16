using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices.Ecom;

public sealed class Transaction : Handle
{
	public Transaction(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result GetTransactionId(StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_Ecom_Transaction_GetTransactionId(base.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public uint GetEntitlementsCount(TransactionGetEntitlementsCountOptions options)
	{
		object obj = default(TransactionGetEntitlementsCountOptions_);
		Helper.CopyProperties(options, obj);
		TransactionGetEntitlementsCountOptions_ options2 = (TransactionGetEntitlementsCountOptions_)obj;
		uint result = EOS_Ecom_Transaction_GetEntitlementsCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyEntitlementByIndex(TransactionCopyEntitlementByIndexOptions options, out Entitlement outEntitlement)
	{
		object obj = default(TransactionCopyEntitlementByIndexOptions_);
		Helper.CopyProperties(options, obj);
		TransactionCopyEntitlementByIndexOptions_ options2 = (TransactionCopyEntitlementByIndexOptions_)obj;
		outEntitlement = Helper.GetDefault<Entitlement>();
		IntPtr outEntitlement2 = IntPtr.Zero;
		Result result = EOS_Ecom_Transaction_CopyEntitlementByIndex(base.InnerHandle, ref options2, ref outEntitlement2);
		options2.Dispose();
		if (outEntitlement2 != IntPtr.Zero)
		{
			Entitlement_ entitlement_ = Marshal.PtrToStructure<Entitlement_>(outEntitlement2);
			outEntitlement = new Entitlement();
			Helper.CopyProperties(entitlement_, outEntitlement);
			EOS_Ecom_Entitlement_Release(outEntitlement2);
			entitlement_.Dispose();
		}
		return result;
	}

	public void Release()
	{
		EOS_Ecom_Transaction_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_Entitlement_Release(IntPtr entitlement);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_Transaction_Release(IntPtr transaction);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_Transaction_CopyEntitlementByIndex(IntPtr handle, ref TransactionCopyEntitlementByIndexOptions_ options, ref IntPtr outEntitlement);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern uint EOS_Ecom_Transaction_GetEntitlementsCount(IntPtr handle, ref TransactionGetEntitlementsCountOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_Transaction_GetTransactionId(IntPtr handle, StringBuilder outBuffer, ref int inOutBufferLength);
}
