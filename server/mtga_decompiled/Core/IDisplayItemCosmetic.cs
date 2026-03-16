using System;

public interface IDisplayItemCosmetic<T>
{
	void SetOnOpenCallback(Action onOpen);

	void SetOnCosmeticSelected(Action<T> onCosmeticSelected);

	void SetOnCloseCallback(Action onClose);
}
