using System;

namespace Wizards.Mtga.AssetBundles.Watcher;

public readonly struct AssetBundleLog
{
	private static ulong _nextId;

	public readonly ulong Id;

	public readonly ABOperation Operation;

	public readonly string AssetBundleName;

	public readonly int TotalRefCount;

	public readonly string AssetName;

	public readonly int AssetCount;

	public readonly double OperationTimeMS;

	public readonly DateTime TimeStamp;

	public string Header => Operation switch
	{
		ABOperation.Loaded => "LOADED " + AssetBundleName, 
		ABOperation.Unloaded => "UNLOADED " + AssetBundleName, 
		ABOperation.RefAdded => "ADDED reference to " + AssetBundleName, 
		ABOperation.RefRemoved => "REMOVED reference from " + AssetBundleName, 
		_ => "OPERATION NOT HANDLED", 
	};

	public AssetBundleLog(ABOperation operation, string abName, int abCount, string assetName, int assetCount, double operationTimeMS)
	{
		Operation = operation;
		AssetBundleName = abName;
		TotalRefCount = abCount;
		AssetName = assetName;
		AssetCount = assetCount;
		OperationTimeMS = operationTimeMS;
		Id = _nextId++;
		TimeStamp = DateTime.Now;
	}

	public static string GenerateCSVHeader()
	{
		return "TimeStamp, Operation, AssetBundleName, TotalRefCount, AssetName, AssetCount, OperationTimeMS\n";
	}

	public string GenerateCSVLine()
	{
		return $"{TimeStamp}, {Enum.GetName(typeof(ABOperation), Operation)}, {AssetBundleName}, {TotalRefCount}, {AssetName}, {AssetCount}, {OperationTimeMS}\n";
	}
}
