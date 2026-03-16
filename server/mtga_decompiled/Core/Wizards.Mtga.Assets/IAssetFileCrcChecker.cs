using System;
using System.Threading.Tasks;

namespace Wizards.Mtga.Assets;

public interface IAssetFileCrcChecker : IDisposable
{
	Task<bool> CheckAssetCrc(string assetPath, uint crc);
}
