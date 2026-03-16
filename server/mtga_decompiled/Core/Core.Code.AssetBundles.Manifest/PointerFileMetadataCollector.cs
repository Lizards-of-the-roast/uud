using System;
using System.Collections.Generic;
using System.IO;
using Assets.Core.Code.AssetBundles;
using Core.BI;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles.Manifest;

public class PointerFileMetadataCollector : IManifestMetadataCollector
{
	private static Logger Logger => ManifestUtils.Logger;

	public Promise<IEnumerable<AssetBundleManifestMetadata>> Collect()
	{
		IAssetBundleSource currentSource = Pantry.Get<AssetBundleSourcesModel>().CurrentSource;
		if (currentSource == null)
		{
			return new SimplePromise<IEnumerable<AssetBundleManifestMetadata>>(new List<AssetBundleManifestMetadata>());
		}
		string path = currentSource.EndpointHashId + ".mtga";
		Uri pointerUri = currentSource.GetBundleUrl(path);
		ManifestUtils.Logger.Info($"GET manifest pointer from {pointerUri} ...");
		if (pointerUri.AbsoluteUri.StartsWith("file://"))
		{
			SimplePromise<IEnumerable<AssetBundleManifestMetadata>> simplePromise = new SimplePromise<IEnumerable<AssetBundleManifestMetadata>>();
			Promise<IEnumerable<AssetBundleManifestMetadata>> result = simplePromise.IfError(delegate(Promise<IEnumerable<AssetBundleManifestMetadata>> p)
			{
				HandleError(p, pointerUri);
			});
			if (ManifestUtils.DoesLocalFileExist(pointerUri))
			{
				Logger.Info("AbsoluteUri contains file prefix, reading from local path");
				List<AssetBundleManifestMetadata> list = ManifestUtils.ParsePointerData(File.ReadAllText(pointerUri.LocalPath));
				Logger.Info($"Found {list.Count} manifest metadata entries from pointer file.");
				simplePromise.SetResult(list);
				return result;
			}
			simplePromise.SetError(new Error(404, "File not found"));
			return result;
		}
		return ((Promise<string>)WebPromise.Get(pointerUri.AbsoluteUri, new Dictionary<string, string>())).Convert((Func<string, IEnumerable<AssetBundleManifestMetadata>>)delegate(string r)
		{
			List<AssetBundleManifestMetadata> list2 = ManifestUtils.ParsePointerData(r);
			Logger.Info($"Found {list2.Count} manifest metadata entries from pointer file.");
			return list2;
		}).IfError(delegate(Promise<IEnumerable<AssetBundleManifestMetadata>> p)
		{
			HandleError(p, pointerUri);
		});
	}

	private static void HandleError(Promise<IEnumerable<AssetBundleManifestMetadata>> p, Uri pointerUri)
	{
		Error error = p.Error;
		int code = error.Code;
		string text;
		if (code >= 0)
		{
			if (code == 404)
			{
				return;
			}
			text = $"Unexpected HTTP response when fetching manifest pointer file from {pointerUri}: ({error.Code}) {error.Message}";
		}
		else
		{
			string arg = ((error.Exception == null) ? string.Empty : $"{error.Exception.GetType()} {error.Exception.Message}");
			text = $"Exception encountered when fetching manifest pointer file from {pointerUri}: {arg}";
		}
		BIEventType.AssetBundleManifestPointerDownloadFailure.SendWithDefaults(("Uri", pointerUri.AbsoluteUri), ("Error", text));
		p.SetError(new Error(error.Code, text, error.Exception));
	}
}
