using System;

namespace Core.Meta.MainNavigation.Cosmetics;

public interface ICosmeticSelector<T>
{
	void SetCallbacks(Action<T> OnSelected, Action OnHide);
}
