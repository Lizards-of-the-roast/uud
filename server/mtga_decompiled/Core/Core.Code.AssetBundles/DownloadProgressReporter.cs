using System;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles;

public class DownloadProgressReporter : IProgress<long>
{
	private IProgress<AssetBundleProvisionProgress> upstream;

	public long completedBytes;

	public long expectedBytes;

	public DownloadProgressReporter(IProgress<AssetBundleProvisionProgress> provisionProgress, long expectedBytes)
	{
		upstream = provisionProgress;
		this.expectedBytes = expectedBytes;
	}

	public void Report(long value)
	{
		lock (this)
		{
			completedBytes += value;
			upstream.Report(AssetBundleProvisionStage.DownloadBundles.ToProgress(completedBytes, expectedBytes));
		}
	}
}
