using System;

namespace Wizards.Mtga.Assets;

public class AssetSignatureMismatch : AssetException
{
	public string Name { get; }

	public long ExpectedLength { get; }

	public long ActualLength { get; }

	public byte[]? ExpectedHash { get; }

	public byte[]? ActualHash { get; }

	public bool? HashMismatch { get; }

	public AssetSignatureMismatch(string name)
		: base("Asset signature mismatch: " + name)
	{
		Name = name;
	}

	public AssetSignatureMismatch(string name, long actualLength, long expectedLength)
		: base($"Asset signature mismatch: {name} is {actualLength} bytes, but expected {expectedLength}")
	{
		Name = name;
		ExpectedLength = expectedLength;
		ActualLength = actualLength;
	}

	public AssetSignatureMismatch(string name, byte[] actualHash, byte[] expectedHash)
		: base("Asset signature mismatch: " + name + " has signature " + Convert.ToBase64String(actualHash) + ", but expected " + Convert.ToBase64String(expectedHash))
	{
		Name = name;
		ActualHash = actualHash;
		ExpectedHash = expectedHash;
	}
}
