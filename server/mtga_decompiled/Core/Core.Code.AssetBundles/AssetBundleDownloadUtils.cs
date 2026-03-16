using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles;

public static class AssetBundleDownloadUtils
{
	private const int DEFAULT_TIMEOUT_MS = 20000;

	private static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler
	{
		Proxy = WebRequest.DefaultWebProxy
	})
	{
		Timeout = TimeSpan.FromMilliseconds(20000.0)
	};

	public static async Task<Stream> GetWrappedBundleStream(AssetFileInfo fileInfo, IProgress<long> progressReporter = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		Stream stream = await GetRawBundleDataStream(fileInfo, cancellationToken);
		if (progressReporter != null)
		{
			stream = new ReportingStream(stream, progressReporter);
		}
		if (fileInfo.WrapperType == AssetFileWrapperType.Gzip)
		{
			stream = new GZipStream(stream, CompressionMode.Decompress);
		}
		return stream;
	}

	public static async Task<Stream> GetRawBundleDataStream(AssetFileInfo fileInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = ((fileInfo.WrapperType != AssetFileWrapperType.Gzip) ? fileInfo.Name : (fileInfo.Name + ".gz"));
		string path = text;
		Uri bundleUri = AssetBundleProvisioner.Source.GetBundleUrl(path, fileInfo);
		try
		{
			if (bundleUri.AbsoluteUri.StartsWith("file://"))
			{
				string localPath = bundleUri.LocalPath;
				if (!File.Exists(localPath))
				{
					throw new AssetDownloadError(bundleUri, HttpStatusCode.NotFound);
				}
				return File.OpenRead(localPath);
			}
			HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(bundleUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			cancellationToken.ThrowIfCancellationRequested();
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				throw new AssetDownloadError(bundleUri, httpResponseMessage.StatusCode);
			}
			return await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (AssetDownloadError)
		{
			throw;
		}
		catch (Exception)
		{
			throw new AssetDownloadError(bundleUri);
		}
	}

	public static async Task<byte[]> CopyAndHashAsync(Stream source, FileStream target, HashAlgorithm hasher, CancellationToken cancellationToken = default(CancellationToken))
	{
		using HashingStream hashingStream = new HashingStream(source, hasher);
		await hashingStream.CopyToAsync(target, 81920, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return hashingStream.Hash;
	}

	public static bool DoesHashMatchBundle(AssetFileInfo bundleInfo, byte[] hash)
	{
		return ((IStructuralEquatable)hash).Equals((object)bundleInfo.Sha256Hash, (IEqualityComparer)EqualityComparer<byte>.Default);
	}
}
