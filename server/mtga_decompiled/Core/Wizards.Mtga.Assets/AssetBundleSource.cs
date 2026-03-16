using System;
using System.Linq;

namespace Wizards.Mtga.Assets;

public class AssetBundleSource : IAssetBundleSource
{
	private const string AudioCategory = "Audio";

	private readonly string _description;

	private Uri BaseUri { get; }

	private Uri CategorizedUri { get; }

	public string EndpointHashId { get; }

	public AssetBundleSourceType HostingSource { get; }

	public AssetBundleSource(Uri baseUri, AssetBundleSourceType hostingSource, string endpointHashId, string description = null, Uri categorizedLocation = null)
	{
		HostingSource = hostingSource;
		BaseUri = BuildUriFromLocation(baseUri);
		CategorizedUri = BuildUriFromLocation(categorizedLocation) ?? BaseUri;
		EndpointHashId = endpointHashId;
		string text = ((CategorizedUri == BaseUri) ? string.Empty : CategorizedUri.AbsoluteUri);
		_description = description ?? $"[{HostingSource}] {BaseUri.AbsoluteUri} {text} {EndpointHashId}";
	}

	private Uri BuildUriFromLocation(Uri location)
	{
		if (location == null)
		{
			return null;
		}
		string text = location.GetLeftPart(UriPartial.Path);
		if (!text.EndsWith("/"))
		{
			text += "/";
		}
		return new Uri(text, UriKind.Absolute);
	}

	public override string ToString()
	{
		return _description;
	}

	public Uri GetBundleUrl(string path)
	{
		return new Uri(BaseUri, path);
	}

	public Uri GetBundleUrl(AssetBundleManifestMetadata metadata)
	{
		if (!string.IsNullOrEmpty(metadata.Category))
		{
			return new Uri(CategorizedUri, metadata.Filename);
		}
		return new Uri(BaseUri, metadata.Filename);
	}

	public Uri GetBundleUrl(string path, IAssetFileInfo fileInfo)
	{
		if (!(fileInfo.AssetType == "Audio"))
		{
			return new Uri(BaseUri, path);
		}
		return new Uri(CategorizedUri, path);
	}

	public static string GetManifestPointerName(string partition, IClientVersionInfo version)
	{
		string version2 = (version.IsDevBuild() ? $"{version.ContentVersion}-DEV-{version.Platform}" : (version.GetFullVersionString() + "-" + version.Platform));
		return GetManifestPointerName(partition, version2);
	}

	public static string GetManifestPointerName(string partition, string version, bool escapeVersionString = false)
	{
		if (escapeVersionString)
		{
			version = string.Concat(from c in version
				where char.IsLetterOrDigit(c) || c == '.'
				select (c != '.') ? c : '_');
		}
		return partition + "_" + version;
	}
}
