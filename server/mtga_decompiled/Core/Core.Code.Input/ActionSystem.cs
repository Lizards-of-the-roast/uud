using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wizards.Arena.Client.Logging;

namespace Core.Code.Input;

public class ActionSystem : IActionSystem, IDisposable, IEnumerable<(IActionSystem.Priority, object)>, IEnumerable
{
	private readonly SortedList<IActionSystem.Priority, Stack<object>> _priorityStacks = new SortedList<IActionSystem.Priority, Stack<object>>();

	private readonly IInputHandler _inputs;

	private readonly ILogger _logger;

	private bool _logsDisabled;

	public IActionSystem.Debug DebugActions { get; } = new IActionSystem.Debug();

	public ActionSystem(IInputHandler inputHandler, ILogger logger, bool captureEscape)
	{
		_logger = logger;
		_inputs = inputHandler;
		_inputs.Navigate += delegate(Direction dir)
		{
			GetFocusedHandler<INavigateActionHandler>()?.OnNavigate(dir);
		};
		_inputs.Accept += delegate
		{
			GetFocusedHandler<IAcceptActionHandler>()?.OnAccept();
		};
		_inputs.Next += delegate
		{
			GetFocusedHandler<INextActionHandler>()?.OnNext();
		};
		_inputs.Previous += delegate
		{
			GetFocusedHandler<IPreviousActionHandler>()?.OnPrevious();
		};
		_inputs.Find += delegate
		{
			GetFocusedHandler<IFindActionHandler>()?.OnFind();
		};
		_inputs.AltViewClose += delegate
		{
			GetFocusedHandler<IAltViewActionHandler>()?.OnCloseAltView();
		};
		_inputs.AltViewOpen += delegate
		{
			GetFocusedHandler<IAltViewActionHandler>()?.OnOpenAltView();
		};
		if (captureEscape)
		{
			_inputs.Back += delegate
			{
				ProcessHandlers(delegate(IBackActionHandler handler, ActionContext context)
				{
					handler.OnBack(context);
				});
			};
		}
		_inputs.TextInput += delegate(char c)
		{
			GetFocusedHandler<ITextActionHandler>()?.OnTextInput(c);
		};
		_inputs.DebugClose += delegate
		{
			DebugActions.DebugClose?.Invoke();
		};
		_inputs.DebugOpen += delegate
		{
			DebugActions.DebugOpen?.Invoke();
		};
		_inputs.DebugToggle += delegate
		{
			DebugActions.DebugToggle?.Invoke();
		};
	}

	public void Update()
	{
		_inputs.Update();
	}

	public void DisableLogs()
	{
		_logsDisabled = true;
	}

	public void Dispose()
	{
		_inputs.Dispose();
	}

	public void PushFocus(object obj, IActionSystem.Priority priority = IActionSystem.Priority.Normal)
	{
		if (!_priorityStacks.ContainsKey(priority))
		{
			_priorityStacks[priority] = new Stack<object>();
		}
		_priorityStacks[priority].Push(obj);
	}

	public void PopFocus(object objToPop)
	{
		foreach (var (_, stack2) in _priorityStacks)
		{
			if (stack2.Contains(objToPop))
			{
				if (stack2.Peek() == objToPop)
				{
					stack2.Pop();
				}
				else
				{
					RemoveFromStack(stack2, objToPop);
				}
				break;
			}
		}
	}

	public bool IsCurrentFocus(object obj)
	{
		foreach (var (key, stack2) in _priorityStacks)
		{
			if (stack2.Contains(obj))
			{
				return _priorityStacks[key].Peek() == obj;
			}
		}
		return false;
	}

	private static void RemoveFromStack<T>(Stack<T> stack, T obj)
	{
		Stack<T> stack2 = new Stack<T>();
		while (stack.Count > 0 && !stack.Peek().Equals(obj))
		{
			stack2.Push(stack.Pop());
		}
		if (stack.Count != 0)
		{
			stack.Pop();
		}
		while (stack2.Count > 0)
		{
			stack.Push(stack2.Pop());
		}
	}

	private void CullNullHandlers()
	{
		foreach (var (_, stack2) in _priorityStacks)
		{
			if (stack2.All((object o) => o != null && !o.Equals(null)))
			{
				continue;
			}
			Stack<object> stack3 = new Stack<object>();
			while (stack2.Count > 0)
			{
				object obj = stack2.Pop();
				if (obj != null && !obj.Equals(null))
				{
					stack3.Push(obj);
				}
			}
			while (stack3.Count > 0)
			{
				stack2.Push(stack3.Pop());
			}
		}
	}

	private T? GetFocusedHandler<T>() where T : class
	{
		CullNullHandlers();
		foreach (KeyValuePair<IActionSystem.Priority, Stack<object>> priorityStack in _priorityStacks)
		{
			priorityStack.Deconstruct(out var _, out var value);
			foreach (object item in value)
			{
				if (item is T result)
				{
					return result;
				}
				if (item is IActionBlocker)
				{
					return null;
				}
			}
		}
		return null;
	}

	private void ProcessHandlers<T>(Action<T, ActionContext> process) where T : class
	{
		CullNullHandlers();
		ActionContext actionContext = new ActionContext();
		foreach (KeyValuePair<IActionSystem.Priority, Stack<object>> priorityStack in _priorityStacks)
		{
			priorityStack.Deconstruct(out var _, out var value);
			object[] array = value.ToArray();
			foreach (object obj in array)
			{
				if (obj is T arg)
				{
					actionContext.Used = true;
					process(arg, actionContext);
					if (actionContext.Used)
					{
						return;
					}
				}
				if (obj is IActionBlocker)
				{
					return;
				}
			}
		}
	}

	private IEnumerator<(IActionSystem.Priority, object)> Iterate()
	{
		foreach (var (pri, stack2) in _priorityStacks)
		{
			foreach (object item in stack2)
			{
				yield return (pri, item);
			}
		}
	}

	public IEnumerator<(IActionSystem.Priority, object)> GetEnumerator()
	{
		return Iterate();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Iterate();
	}
}
