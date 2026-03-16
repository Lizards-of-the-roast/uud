using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles;

public class ConcurrentDownloadQueue : IAssetBundleDownloadQueue
{
	private readonly ConcurrentQueue<AssetFileInfo> _queue;

	public long RemainingBytes { get; private set; }

	public int Count { get; private set; }

	public ConcurrentDownloadQueue(IReadOnlyCollection<AssetFileInfo> toDownload)
	{
		_queue = new ConcurrentQueue<AssetFileInfo>(toDownload);
		RemainingBytes = toDownload.Sum((AssetFileInfo x) => x.GetDownloadBytes());
		Count = toDownload.Count;
	}

	public bool TryDequeuePendingDownload(out AssetFileInfo bundleInfo)
	{
		bool flag = _queue.TryDequeue(out bundleInfo);
		if (flag)
		{
			lock (this)
			{
				RemainingBytes -= bundleInfo.GetDownloadBytes();
				int count = Count - 1;
				Count = count;
			}
		}
		return flag;
	}

	public void RequeuePendingDownload(AssetFileInfo bundleInfo)
	{
		_queue.Enqueue(bundleInfo);
		lock (this)
		{
			RemainingBytes += bundleInfo.GetDownloadBytes();
			int count = Count + 1;
			Count = count;
		}
	}
}
