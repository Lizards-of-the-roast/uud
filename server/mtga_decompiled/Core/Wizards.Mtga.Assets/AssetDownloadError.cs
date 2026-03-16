using System;
using System.Net;

namespace Wizards.Mtga.Assets;

public class AssetDownloadError : AssetException
{
	public Uri Uri { get; }

	public HttpStatusCode? StatusCode { get; }

	public AssetDownloadError(Uri uri, HttpStatusCode? statusCode = null, Exception? exception = null)
		: base($"Unexpected error while fetching {uri}: ({statusCode}) {exception?.Message}", exception)
	{
		Uri = uri;
		StatusCode = statusCode;
	}
}
