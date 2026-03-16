using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Wizards.Mtga.Storage;

namespace Wizards.Mtga.Assets;

public interface IAssetFileChecker
{
	Task CheckExistingAssetFiles(Func<string, IAssetFileSignature> getAssetFileSignature, ConcurrentDictionary<string, string> renamableAssets, Action<FileInfo, AssetFileCheckResult> resultCallback, IStorageContext storageContext, IAssetFileValidationDetector validationDetector, IProgress<AssetBundleProvisionProgress> progress = null);
}
