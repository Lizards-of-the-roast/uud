using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Metrics;

public sealed class MetricsInterface : Handle
{
	public MetricsInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result BeginPlayerSession(BeginPlayerSessionOptions options)
	{
		object obj = default(BeginPlayerSessionOptions_);
		Helper.CopyProperties(options, obj);
		BeginPlayerSessionOptions_ options2 = (BeginPlayerSessionOptions_)obj;
		Result result = EOS_Metrics_BeginPlayerSession(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result EndPlayerSession(EndPlayerSessionOptions options)
	{
		object obj = default(EndPlayerSessionOptions_);
		Helper.CopyProperties(options, obj);
		EndPlayerSessionOptions_ options2 = (EndPlayerSessionOptions_)obj;
		Result result = EOS_Metrics_EndPlayerSession(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Metrics_EndPlayerSession(IntPtr handle, ref EndPlayerSessionOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Metrics_BeginPlayerSession(IntPtr handle, ref BeginPlayerSessionOptions_ options);
}
