using System;
using Wizards.Mtga;

namespace Core.Code.Decks;

public class DeckBuilderContextProvider
{
	private readonly FormatManager _formatManager;

	private DeckBuilderContext _context;

	public DeckBuilderContext Context
	{
		get
		{
			return _context;
		}
		set
		{
			_context = value;
			if (_context.Deck == null)
			{
				_context.Format = null;
			}
			else if (_context.Event == null)
			{
				_context.Format = _formatManager.GetSafeFormat(_context.Deck.format);
			}
			else if (!string.IsNullOrWhiteSpace(_context.Event.PlayerEvent.EventUXInfo.DeckSelectFormat))
			{
				_context.Format = _formatManager.GetSafeFormat(_context.Event.PlayerEvent.EventUXInfo.DeckSelectFormat);
			}
			this.OnContextSet?.Invoke(_context);
		}
	}

	public event Action<DeckBuilderContext> OnContextSet;

	public event Action<DeckFormat> OnDeckFormatSet;

	public static DeckBuilderContextProvider Create()
	{
		return new DeckBuilderContextProvider(Pantry.Get<FormatManager>());
	}

	private DeckBuilderContextProvider(FormatManager formatManager)
	{
		_formatManager = formatManager;
	}

	public void SelectFormat(DeckFormat format)
	{
		DeckBuilderModelProvider deckBuilderModelProvider = Pantry.Get<DeckBuilderModelProvider>();
		Context.Format = format;
		Context.IsAmbiguousFormat = false;
		deckBuilderModelProvider.SetDeckFormat(format);
		this.OnDeckFormatSet?.Invoke(format);
	}
}
