using System;
using System.Net;

namespace Wizards.Mtga.Assets;

public class ManifestNotFound : AssetException
{
	public Uri Uri { get; }

	public HttpStatusCode? StatusCode { get; }

	public ManifestNotFound(Uri uri, HttpStatusCode? statusCode = null)
		: base($"Could not retrieve bundle manifest: {uri} ({statusCode})")
	{
		Uri = uri;
		StatusCode = statusCode;
	}
}
