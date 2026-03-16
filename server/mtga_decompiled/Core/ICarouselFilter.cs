using System.Threading.Tasks;
using Wotc.Mtga.Network.ServiceWrappers;

internal interface ICarouselFilter
{
	Task<bool> checkVisible(Client_CarouselItem item);
}
