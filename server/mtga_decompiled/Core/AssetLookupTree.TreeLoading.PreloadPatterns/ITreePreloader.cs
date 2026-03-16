using System.Threading.Tasks;

namespace AssetLookupTree.TreeLoading.PreloadPatterns;

public interface ITreePreloader
{
	Task PreloadTreesAsync(AssetLookupTreeLoader loader);
}
