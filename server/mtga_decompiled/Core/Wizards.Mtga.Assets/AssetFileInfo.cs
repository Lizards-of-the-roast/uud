using System;
using Newtonsoft.Json;

namespace Wizards.Mtga.Assets;

[JsonConverter(typeof(AssetFileInfoConverter))]
public sealed class AssetFileInfo : IAssetFileInfo, IAssetFileSignature
{
	public const string DEFAULT_ASSET_TYPE = "AssetBundle";

	public const AssetFileWrapperType DEFAULT_WRAPPER_TYPE = AssetFileWrapperType.None;

	public string Name { get; }

	public string AssetType { get; }

	public long Length { get; }

	public AssetFileWrapperType WrapperType { get; }

	public long CompressedLength { get; }

	public AssetPriority Priority { get; set; }

	public byte[] Sha256Hash { get; }

	public uint? Crc32 { get; }

	public string[] IndexedAssets { get; }

	public string[] Dependencies { get; }

	public string OldName { get; }

	public AssetFileInfo(string name, long length, AssetPriority priority, string[] indexedAssets, string[] dependencies = null, string assetType = "AssetBundle", AssetFileWrapperType wrapperType = AssetFileWrapperType.None, long compressedLength = 0L, byte[] sha256Hash = null, uint? crc32 = null, string oldName = null)
	{
		Name = name;
		AssetType = assetType;
		Length = length;
		WrapperType = wrapperType;
		CompressedLength = compressedLength;
		Priority = priority;
		Sha256Hash = sha256Hash;
		Crc32 = crc32;
		IndexedAssets = indexedAssets ?? Array.Empty<string>();
		Dependencies = dependencies ?? Array.Empty<string>();
		OldName = oldName;
	}
}
