using System;
using GreClient.Rules;

public interface IHoverableZone
{
	event Action<MtgZone> Hovered;
}
