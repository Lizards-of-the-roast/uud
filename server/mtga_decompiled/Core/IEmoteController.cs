using System;

public interface IEmoteController : IDisposable
{
	bool Hovered { get; set; }

	void Open();

	void Close();

	void Toggle();
}
