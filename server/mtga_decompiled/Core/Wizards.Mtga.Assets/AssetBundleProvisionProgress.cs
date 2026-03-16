using System;

namespace Wizards.Mtga.Assets;

public readonly struct AssetBundleProvisionProgress
{
	public AssetBundleProvisionStage Stage { get; }

	public long Completed { get; }

	public long Total { get; }

	public Exception Exception { get; }

	public bool IsCompleted
	{
		get
		{
			if (Total == Completed)
			{
				return Completed > 0;
			}
			return false;
		}
	}

	public AssetBundleProvisionProgress(AssetBundleProvisionStage stage, long completed, long total)
	{
		Stage = stage;
		Completed = completed;
		Total = total;
		Exception = null;
	}

	public AssetBundleProvisionProgress(Exception exception)
		: this(AssetBundleProvisionStage.Error, 0L, 0L)
	{
		Exception = exception;
	}
}
