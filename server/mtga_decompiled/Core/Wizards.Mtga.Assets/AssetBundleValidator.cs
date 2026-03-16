using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wizards.Mtga.Assets;

public class AssetBundleValidator : IAssetFileCrcChecker, IDisposable
{
	private readonly BlockingCollection<(string Path, uint Crc, TaskCompletionSource<bool> CompletionSource)> CrcCheckQueue = new BlockingCollection<(string, uint, TaskCompletionSource<bool>)>();

	private readonly UniTaskVoid _processor;

	public AssetBundleValidator()
	{
		_processor = ProcessValidationsAsync();
	}

	public Task<bool> CheckAssetCrc(string bundlePath, uint crc)
	{
		TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
		CrcCheckQueue.Add((bundlePath, crc, taskCompletionSource));
		return taskCompletionSource.Task;
	}

	private async UniTaskVoid ProcessValidationsAsync()
	{
		Queue<(AssetBundleCreateRequest, TaskCompletionSource<bool>)> resultQueue = new Queue<(AssetBundleCreateRequest, TaskCompletionSource<bool>)>();
		while (!CrcCheckQueue.IsCompleted || resultQueue.Count > 0)
		{
			(string, uint, TaskCompletionSource<bool>) item;
			while (CrcCheckQueue.TryTake(out item))
			{
				resultQueue.Enqueue((AssetBundle.LoadFromFileAsync(item.Item1, item.Item2), item.Item3));
			}
			while (resultQueue.Count > 0)
			{
				var (assetBundleCreateRequest, taskCompletionSource) = resultQueue.Peek();
				if (!assetBundleCreateRequest.isDone)
				{
					break;
				}
				if ((object)assetBundleCreateRequest.assetBundle == null)
				{
					taskCompletionSource.SetResult(result: false);
				}
				else
				{
					taskCompletionSource.SetResult(result: true);
					assetBundleCreateRequest.assetBundle.Unload(unloadAllLoadedObjects: false);
				}
				resultQueue.Dequeue();
			}
			await UniTask.Yield();
		}
	}

	public void Dispose()
	{
		CrcCheckQueue.CompleteAdding();
	}
}
