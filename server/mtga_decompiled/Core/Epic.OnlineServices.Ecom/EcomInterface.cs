using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

public sealed class EcomInterface : Handle
{
	private delegate void OnRedeemEntitlementsCallbackInternal(IntPtr messagePtr);

	private delegate void OnCheckoutCallbackInternal(IntPtr messagePtr);

	private delegate void OnQueryOffersCallbackInternal(IntPtr messagePtr);

	private delegate void OnQueryEntitlementsCallbackInternal(IntPtr messagePtr);

	private delegate void OnQueryOwnershipTokenCallbackInternal(IntPtr messagePtr);

	private delegate void OnQueryOwnershipCallbackInternal(IntPtr messagePtr);

	public EcomInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryOwnership(QueryOwnershipOptions options, object clientData, OnQueryOwnershipCallback completionDelegate)
	{
		object obj = default(QueryOwnershipOptions_);
		Helper.CopyProperties(options, obj);
		QueryOwnershipOptions_ options2 = (QueryOwnershipOptions_)obj;
		OnQueryOwnershipCallbackInternal onQueryOwnershipCallbackInternal = OnQueryOwnership;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onQueryOwnershipCallbackInternal);
		EOS_Ecom_QueryOwnership(base.InnerHandle, ref options2, clientDataAddress, onQueryOwnershipCallbackInternal);
		options2.Dispose();
	}

	public void QueryOwnershipToken(QueryOwnershipTokenOptions options, object clientData, OnQueryOwnershipTokenCallback completionDelegate)
	{
		object obj = default(QueryOwnershipTokenOptions_);
		Helper.CopyProperties(options, obj);
		QueryOwnershipTokenOptions_ options2 = (QueryOwnershipTokenOptions_)obj;
		OnQueryOwnershipTokenCallbackInternal onQueryOwnershipTokenCallbackInternal = OnQueryOwnershipToken;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onQueryOwnershipTokenCallbackInternal);
		EOS_Ecom_QueryOwnershipToken(base.InnerHandle, ref options2, clientDataAddress, onQueryOwnershipTokenCallbackInternal);
		options2.Dispose();
	}

	public void QueryEntitlements(QueryEntitlementsOptions options, object clientData, OnQueryEntitlementsCallback completionDelegate)
	{
		object obj = default(QueryEntitlementsOptions_);
		Helper.CopyProperties(options, obj);
		QueryEntitlementsOptions_ options2 = (QueryEntitlementsOptions_)obj;
		OnQueryEntitlementsCallbackInternal onQueryEntitlementsCallbackInternal = OnQueryEntitlements;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onQueryEntitlementsCallbackInternal);
		EOS_Ecom_QueryEntitlements(base.InnerHandle, ref options2, clientDataAddress, onQueryEntitlementsCallbackInternal);
		options2.Dispose();
	}

	public void QueryOffers(QueryOffersOptions options, object clientData, OnQueryOffersCallback completionDelegate)
	{
		object obj = default(QueryOffersOptions_);
		Helper.CopyProperties(options, obj);
		QueryOffersOptions_ options2 = (QueryOffersOptions_)obj;
		OnQueryOffersCallbackInternal onQueryOffersCallbackInternal = OnQueryOffers;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onQueryOffersCallbackInternal);
		EOS_Ecom_QueryOffers(base.InnerHandle, ref options2, clientDataAddress, onQueryOffersCallbackInternal);
		options2.Dispose();
	}

	public void Checkout(CheckoutOptions options, object clientData, OnCheckoutCallback completionDelegate)
	{
		object obj = default(CheckoutOptions_);
		Helper.CopyProperties(options, obj);
		CheckoutOptions_ options2 = (CheckoutOptions_)obj;
		OnCheckoutCallbackInternal onCheckoutCallbackInternal = OnCheckout;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onCheckoutCallbackInternal);
		EOS_Ecom_Checkout(base.InnerHandle, ref options2, clientDataAddress, onCheckoutCallbackInternal);
		options2.Dispose();
	}

	public void RedeemEntitlements(RedeemEntitlementsOptions options, object clientData, OnRedeemEntitlementsCallback completionDelegate)
	{
		object obj = default(RedeemEntitlementsOptions_);
		Helper.CopyProperties(options, obj);
		RedeemEntitlementsOptions_ options2 = (RedeemEntitlementsOptions_)obj;
		OnRedeemEntitlementsCallbackInternal onRedeemEntitlementsCallbackInternal = OnRedeemEntitlements;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onRedeemEntitlementsCallbackInternal);
		EOS_Ecom_RedeemEntitlements(base.InnerHandle, ref options2, clientDataAddress, onRedeemEntitlementsCallbackInternal);
		options2.Dispose();
	}

	public uint GetEntitlementsCount(GetEntitlementsCountOptions options)
	{
		object obj = default(GetEntitlementsCountOptions_);
		Helper.CopyProperties(options, obj);
		GetEntitlementsCountOptions_ options2 = (GetEntitlementsCountOptions_)obj;
		uint result = EOS_Ecom_GetEntitlementsCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyEntitlementByIndex(CopyEntitlementByIndexOptions options, out Entitlement outEntitlement)
	{
		object obj = default(CopyEntitlementByIndexOptions_);
		Helper.CopyProperties(options, obj);
		CopyEntitlementByIndexOptions_ options2 = (CopyEntitlementByIndexOptions_)obj;
		outEntitlement = Helper.GetDefault<Entitlement>();
		IntPtr outEntitlement2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyEntitlementByIndex(base.InnerHandle, ref options2, ref outEntitlement2);
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

	public Result CopyEntitlementById(CopyEntitlementByIdOptions options, out Entitlement outEntitlement)
	{
		object obj = default(CopyEntitlementByIdOptions_);
		Helper.CopyProperties(options, obj);
		CopyEntitlementByIdOptions_ options2 = (CopyEntitlementByIdOptions_)obj;
		outEntitlement = Helper.GetDefault<Entitlement>();
		IntPtr outEntitlement2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyEntitlementById(base.InnerHandle, ref options2, ref outEntitlement2);
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

	public uint GetOfferCount(GetOfferCountOptions options)
	{
		object obj = default(GetOfferCountOptions_);
		Helper.CopyProperties(options, obj);
		GetOfferCountOptions_ options2 = (GetOfferCountOptions_)obj;
		uint result = EOS_Ecom_GetOfferCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyOfferByIndex(CopyOfferByIndexOptions options, out CatalogOffer outOffer)
	{
		object obj = default(CopyOfferByIndexOptions_);
		Helper.CopyProperties(options, obj);
		CopyOfferByIndexOptions_ options2 = (CopyOfferByIndexOptions_)obj;
		outOffer = Helper.GetDefault<CatalogOffer>();
		IntPtr outOffer2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyOfferByIndex(base.InnerHandle, ref options2, ref outOffer2);
		options2.Dispose();
		if (outOffer2 != IntPtr.Zero)
		{
			CatalogOffer_ catalogOffer_ = Marshal.PtrToStructure<CatalogOffer_>(outOffer2);
			outOffer = new CatalogOffer();
			Helper.CopyProperties(catalogOffer_, outOffer);
			EOS_Ecom_CatalogOffer_Release(outOffer2);
			catalogOffer_.Dispose();
		}
		return result;
	}

	public Result CopyOfferById(CopyOfferByIdOptions options, out CatalogOffer outOffer)
	{
		object obj = default(CopyOfferByIdOptions_);
		Helper.CopyProperties(options, obj);
		CopyOfferByIdOptions_ options2 = (CopyOfferByIdOptions_)obj;
		outOffer = Helper.GetDefault<CatalogOffer>();
		IntPtr outOffer2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyOfferById(base.InnerHandle, ref options2, ref outOffer2);
		options2.Dispose();
		if (outOffer2 != IntPtr.Zero)
		{
			CatalogOffer_ catalogOffer_ = Marshal.PtrToStructure<CatalogOffer_>(outOffer2);
			outOffer = new CatalogOffer();
			Helper.CopyProperties(catalogOffer_, outOffer);
			EOS_Ecom_CatalogOffer_Release(outOffer2);
			catalogOffer_.Dispose();
		}
		return result;
	}

	public uint GetOfferItemCount(GetOfferItemCountOptions options)
	{
		object obj = default(GetOfferItemCountOptions_);
		Helper.CopyProperties(options, obj);
		GetOfferItemCountOptions_ options2 = (GetOfferItemCountOptions_)obj;
		uint result = EOS_Ecom_GetOfferItemCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyOfferItemByIndex(CopyOfferItemByIndexOptions options, out CatalogItem outItem)
	{
		object obj = default(CopyOfferItemByIndexOptions_);
		Helper.CopyProperties(options, obj);
		CopyOfferItemByIndexOptions_ options2 = (CopyOfferItemByIndexOptions_)obj;
		outItem = Helper.GetDefault<CatalogItem>();
		IntPtr outItem2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyOfferItemByIndex(base.InnerHandle, ref options2, ref outItem2);
		options2.Dispose();
		if (outItem2 != IntPtr.Zero)
		{
			CatalogItem_ catalogItem_ = Marshal.PtrToStructure<CatalogItem_>(outItem2);
			outItem = new CatalogItem();
			Helper.CopyProperties(catalogItem_, outItem);
			EOS_Ecom_CatalogItem_Release(outItem2);
			catalogItem_.Dispose();
		}
		return result;
	}

	public Result CopyItemById(CopyItemByIdOptions options, out CatalogItem outItem)
	{
		object obj = default(CopyItemByIdOptions_);
		Helper.CopyProperties(options, obj);
		CopyItemByIdOptions_ options2 = (CopyItemByIdOptions_)obj;
		outItem = Helper.GetDefault<CatalogItem>();
		IntPtr outItem2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyItemById(base.InnerHandle, ref options2, ref outItem2);
		options2.Dispose();
		if (outItem2 != IntPtr.Zero)
		{
			CatalogItem_ catalogItem_ = Marshal.PtrToStructure<CatalogItem_>(outItem2);
			outItem = new CatalogItem();
			Helper.CopyProperties(catalogItem_, outItem);
			EOS_Ecom_CatalogItem_Release(outItem2);
			catalogItem_.Dispose();
		}
		return result;
	}

	public uint GetItemImageInfoCount(GetItemImageInfoCountOptions options)
	{
		object obj = default(GetItemImageInfoCountOptions_);
		Helper.CopyProperties(options, obj);
		GetItemImageInfoCountOptions_ options2 = (GetItemImageInfoCountOptions_)obj;
		uint result = EOS_Ecom_GetItemImageInfoCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyItemImageInfoByIndex(CopyItemImageInfoByIndexOptions options, out KeyImageInfo outImageInfo)
	{
		object obj = default(CopyItemImageInfoByIndexOptions_);
		Helper.CopyProperties(options, obj);
		CopyItemImageInfoByIndexOptions_ options2 = (CopyItemImageInfoByIndexOptions_)obj;
		outImageInfo = Helper.GetDefault<KeyImageInfo>();
		IntPtr outImageInfo2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyItemImageInfoByIndex(base.InnerHandle, ref options2, ref outImageInfo2);
		options2.Dispose();
		if (outImageInfo2 != IntPtr.Zero)
		{
			KeyImageInfo_ keyImageInfo_ = Marshal.PtrToStructure<KeyImageInfo_>(outImageInfo2);
			outImageInfo = new KeyImageInfo();
			Helper.CopyProperties(keyImageInfo_, outImageInfo);
			EOS_Ecom_KeyImageInfo_Release(outImageInfo2);
			keyImageInfo_.Dispose();
		}
		return result;
	}

	public uint GetItemReleaseCount(GetItemReleaseCountOptions options)
	{
		object obj = default(GetItemReleaseCountOptions_);
		Helper.CopyProperties(options, obj);
		GetItemReleaseCountOptions_ options2 = (GetItemReleaseCountOptions_)obj;
		uint result = EOS_Ecom_GetItemReleaseCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyItemReleaseByIndex(CopyItemReleaseByIndexOptions options, out CatalogRelease outRelease)
	{
		object obj = default(CopyItemReleaseByIndexOptions_);
		Helper.CopyProperties(options, obj);
		CopyItemReleaseByIndexOptions_ options2 = (CopyItemReleaseByIndexOptions_)obj;
		outRelease = Helper.GetDefault<CatalogRelease>();
		IntPtr outRelease2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyItemReleaseByIndex(base.InnerHandle, ref options2, ref outRelease2);
		options2.Dispose();
		if (outRelease2 != IntPtr.Zero)
		{
			CatalogRelease_ catalogRelease_ = Marshal.PtrToStructure<CatalogRelease_>(outRelease2);
			outRelease = new CatalogRelease();
			Helper.CopyProperties(catalogRelease_, outRelease);
			EOS_Ecom_CatalogRelease_Release(outRelease2);
			catalogRelease_.Dispose();
		}
		return result;
	}

	public uint GetTransactionCount(GetTransactionCountOptions options)
	{
		object obj = default(GetTransactionCountOptions_);
		Helper.CopyProperties(options, obj);
		GetTransactionCountOptions_ options2 = (GetTransactionCountOptions_)obj;
		uint result = EOS_Ecom_GetTransactionCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyTransactionByIndex(CopyTransactionByIndexOptions options, out Transaction outTransaction)
	{
		object obj = default(CopyTransactionByIndexOptions_);
		Helper.CopyProperties(options, obj);
		CopyTransactionByIndexOptions_ options2 = (CopyTransactionByIndexOptions_)obj;
		outTransaction = Helper.GetDefault<Transaction>();
		IntPtr outTransaction2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyTransactionByIndex(base.InnerHandle, ref options2, ref outTransaction2);
		options2.Dispose();
		outTransaction = ((outTransaction2 == IntPtr.Zero) ? null : new Transaction(outTransaction2));
		return result;
	}

	public Result CopyTransactionById(CopyTransactionByIdOptions options, out Transaction outTransaction)
	{
		object obj = default(CopyTransactionByIdOptions_);
		Helper.CopyProperties(options, obj);
		CopyTransactionByIdOptions_ options2 = (CopyTransactionByIdOptions_)obj;
		outTransaction = Helper.GetDefault<Transaction>();
		IntPtr outTransaction2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyTransactionById(base.InnerHandle, ref options2, ref outTransaction2);
		options2.Dispose();
		outTransaction = ((outTransaction2 == IntPtr.Zero) ? null : new Transaction(outTransaction2));
		return result;
	}

	[MonoPInvokeCallback]
	private static void OnRedeemEntitlements(IntPtr messageAddress)
	{
		RedeemEntitlementsCallbackInfo_ redeemEntitlementsCallbackInfo_ = Marshal.PtrToStructure<RedeemEntitlementsCallbackInfo_>(messageAddress);
		RedeemEntitlementsCallbackInfo redeemEntitlementsCallbackInfo = new RedeemEntitlementsCallbackInfo();
		Helper.CopyProperties(redeemEntitlementsCallbackInfo_, redeemEntitlementsCallbackInfo);
		IntPtr clientDataAddress = redeemEntitlementsCallbackInfo_.ClientDataAddress;
		redeemEntitlementsCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, redeemEntitlementsCallbackInfo) as OnRedeemEntitlementsCallback)(redeemEntitlementsCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnCheckout(IntPtr messageAddress)
	{
		CheckoutCallbackInfo_ checkoutCallbackInfo_ = Marshal.PtrToStructure<CheckoutCallbackInfo_>(messageAddress);
		CheckoutCallbackInfo checkoutCallbackInfo = new CheckoutCallbackInfo();
		Helper.CopyProperties(checkoutCallbackInfo_, checkoutCallbackInfo);
		IntPtr clientDataAddress = checkoutCallbackInfo_.ClientDataAddress;
		checkoutCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, checkoutCallbackInfo) as OnCheckoutCallback)(checkoutCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnQueryOffers(IntPtr messageAddress)
	{
		QueryOffersCallbackInfo_ queryOffersCallbackInfo_ = Marshal.PtrToStructure<QueryOffersCallbackInfo_>(messageAddress);
		QueryOffersCallbackInfo queryOffersCallbackInfo = new QueryOffersCallbackInfo();
		Helper.CopyProperties(queryOffersCallbackInfo_, queryOffersCallbackInfo);
		IntPtr clientDataAddress = queryOffersCallbackInfo_.ClientDataAddress;
		queryOffersCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, queryOffersCallbackInfo) as OnQueryOffersCallback)(queryOffersCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnQueryEntitlements(IntPtr messageAddress)
	{
		QueryEntitlementsCallbackInfo_ queryEntitlementsCallbackInfo_ = Marshal.PtrToStructure<QueryEntitlementsCallbackInfo_>(messageAddress);
		QueryEntitlementsCallbackInfo queryEntitlementsCallbackInfo = new QueryEntitlementsCallbackInfo();
		Helper.CopyProperties(queryEntitlementsCallbackInfo_, queryEntitlementsCallbackInfo);
		IntPtr clientDataAddress = queryEntitlementsCallbackInfo_.ClientDataAddress;
		queryEntitlementsCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, queryEntitlementsCallbackInfo) as OnQueryEntitlementsCallback)(queryEntitlementsCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnQueryOwnershipToken(IntPtr messageAddress)
	{
		QueryOwnershipTokenCallbackInfo_ queryOwnershipTokenCallbackInfo_ = Marshal.PtrToStructure<QueryOwnershipTokenCallbackInfo_>(messageAddress);
		QueryOwnershipTokenCallbackInfo queryOwnershipTokenCallbackInfo = new QueryOwnershipTokenCallbackInfo();
		Helper.CopyProperties(queryOwnershipTokenCallbackInfo_, queryOwnershipTokenCallbackInfo);
		IntPtr clientDataAddress = queryOwnershipTokenCallbackInfo_.ClientDataAddress;
		queryOwnershipTokenCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, queryOwnershipTokenCallbackInfo) as OnQueryOwnershipTokenCallback)(queryOwnershipTokenCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnQueryOwnership(IntPtr messageAddress)
	{
		QueryOwnershipCallbackInfo_ queryOwnershipCallbackInfo_ = Marshal.PtrToStructure<QueryOwnershipCallbackInfo_>(messageAddress);
		QueryOwnershipCallbackInfo queryOwnershipCallbackInfo = new QueryOwnershipCallbackInfo();
		Helper.CopyProperties(queryOwnershipCallbackInfo_, queryOwnershipCallbackInfo);
		IntPtr clientDataAddress = queryOwnershipCallbackInfo_.ClientDataAddress;
		queryOwnershipCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, queryOwnershipCallbackInfo) as OnQueryOwnershipCallback)(queryOwnershipCallbackInfo);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_CatalogRelease_Release(IntPtr catalogRelease);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_KeyImageInfo_Release(IntPtr keyImageInfo);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_CatalogOffer_Release(IntPtr catalogOffer);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_CatalogItem_Release(IntPtr catalogItem);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_Entitlement_Release(IntPtr entitlement);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_CopyTransactionById(IntPtr handle, ref CopyTransactionByIdOptions_ options, ref IntPtr outTransaction);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_CopyTransactionByIndex(IntPtr handle, ref CopyTransactionByIndexOptions_ options, ref IntPtr outTransaction);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern uint EOS_Ecom_GetTransactionCount(IntPtr handle, ref GetTransactionCountOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_CopyItemReleaseByIndex(IntPtr handle, ref CopyItemReleaseByIndexOptions_ options, ref IntPtr outRelease);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern uint EOS_Ecom_GetItemReleaseCount(IntPtr handle, ref GetItemReleaseCountOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_CopyItemImageInfoByIndex(IntPtr handle, ref CopyItemImageInfoByIndexOptions_ options, ref IntPtr outImageInfo);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern uint EOS_Ecom_GetItemImageInfoCount(IntPtr handle, ref GetItemImageInfoCountOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_CopyItemById(IntPtr handle, ref CopyItemByIdOptions_ options, ref IntPtr outItem);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_CopyOfferItemByIndex(IntPtr handle, ref CopyOfferItemByIndexOptions_ options, ref IntPtr outItem);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern uint EOS_Ecom_GetOfferItemCount(IntPtr handle, ref GetOfferItemCountOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_CopyOfferById(IntPtr handle, ref CopyOfferByIdOptions_ options, ref IntPtr outOffer);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_CopyOfferByIndex(IntPtr handle, ref CopyOfferByIndexOptions_ options, ref IntPtr outOffer);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern uint EOS_Ecom_GetOfferCount(IntPtr handle, ref GetOfferCountOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_CopyEntitlementById(IntPtr handle, ref CopyEntitlementByIdOptions_ options, ref IntPtr outEntitlement);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Ecom_CopyEntitlementByIndex(IntPtr handle, ref CopyEntitlementByIndexOptions_ options, ref IntPtr outEntitlement);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern uint EOS_Ecom_GetEntitlementsCount(IntPtr handle, ref GetEntitlementsCountOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_RedeemEntitlements(IntPtr handle, ref RedeemEntitlementsOptions_ options, IntPtr clientData, OnRedeemEntitlementsCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_Checkout(IntPtr handle, ref CheckoutOptions_ options, IntPtr clientData, OnCheckoutCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_QueryOffers(IntPtr handle, ref QueryOffersOptions_ options, IntPtr clientData, OnQueryOffersCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_QueryEntitlements(IntPtr handle, ref QueryEntitlementsOptions_ options, IntPtr clientData, OnQueryEntitlementsCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_QueryOwnershipToken(IntPtr handle, ref QueryOwnershipTokenOptions_ options, IntPtr clientData, OnQueryOwnershipTokenCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Ecom_QueryOwnership(IntPtr handle, ref QueryOwnershipOptions_ options, IntPtr clientData, OnQueryOwnershipCallbackInternal completionDelegate);
}
