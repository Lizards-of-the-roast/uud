using System;
using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code.CardFilters;
using Wotc.Mtga.Cards.Database;

public class CardMatcher
{
	public class CardMatcherMetadata
	{
		public IReadOnlyDictionary<uint, int> TitleIdsToNumberOwned;
	}

	private readonly List<Token> _tokenStream;

	private readonly ICardMatcherReq _astRoot;

	private static readonly Dictionary<string, CardPropertyFilter.PropertyType> _propertyMap;

	private CardMatcherMetadata _metadata;

	private List<string> _allAnyTexts;

	private readonly ICardDatabaseAdapter _cardDatabase = NullCardDatabaseAdapter.Default;

	private readonly List<string> _errors = new List<string>();

	public bool Successful => _errors.Count == 0;

	static CardMatcher()
	{
		_propertyMap = new Dictionary<string, CardPropertyFilter.PropertyType>();
		_propertyMap.Add("E", CardPropertyFilter.PropertyType.ExpansionCode);
		_propertyMap.Add("EDITION", CardPropertyFilter.PropertyType.ExpansionCode);
		_propertyMap.Add("SET", CardPropertyFilter.PropertyType.ExpansionCode);
		_propertyMap.Add("S", CardPropertyFilter.PropertyType.ExpansionCode);
		_propertyMap.Add("C", CardPropertyFilter.PropertyType.Color);
		_propertyMap.Add("COLOR", CardPropertyFilter.PropertyType.Color);
		_propertyMap.Add("ID", CardPropertyFilter.PropertyType.ColorIdentity);
		_propertyMap.Add("IDENTITY", CardPropertyFilter.PropertyType.ColorIdentity);
		_propertyMap.Add("T", CardPropertyFilter.PropertyType.Type);
		_propertyMap.Add("TYPE", CardPropertyFilter.PropertyType.Type);
		_propertyMap.Add("O", CardPropertyFilter.PropertyType.Text);
		_propertyMap.Add("ORACLE", CardPropertyFilter.PropertyType.Text);
		_propertyMap.Add("TEXT", CardPropertyFilter.PropertyType.Text);
		_propertyMap.Add("LOY", CardPropertyFilter.PropertyType.Loyalty);
		_propertyMap.Add("LOYALTY", CardPropertyFilter.PropertyType.Loyalty);
		_propertyMap.Add("POW", CardPropertyFilter.PropertyType.Power);
		_propertyMap.Add("POWER", CardPropertyFilter.PropertyType.Power);
		_propertyMap.Add("TOU", CardPropertyFilter.PropertyType.Toughness);
		_propertyMap.Add("TOUGHNESS", CardPropertyFilter.PropertyType.Toughness);
		_propertyMap.Add("M", CardPropertyFilter.PropertyType.ManaCost);
		_propertyMap.Add("MANA", CardPropertyFilter.PropertyType.ManaCost);
		_propertyMap.Add("R", CardPropertyFilter.PropertyType.Rarity);
		_propertyMap.Add("RARITY", CardPropertyFilter.PropertyType.Rarity);
		_propertyMap.Add("CMC", CardPropertyFilter.PropertyType.CMC);
		_propertyMap.Add("MV", CardPropertyFilter.PropertyType.CMC);
		_propertyMap.Add("TITLE", CardPropertyFilter.PropertyType.Title);
		_propertyMap.Add("NAME", CardPropertyFilter.PropertyType.Title);
		_propertyMap.Add("Q", CardPropertyFilter.PropertyType.Owned);
		_propertyMap.Add("OWNED", CardPropertyFilter.PropertyType.Owned);
		_propertyMap.Add("A", CardPropertyFilter.PropertyType.Artist);
		_propertyMap.Add("ART", CardPropertyFilter.PropertyType.Artist);
		_propertyMap.Add("ARTIST", CardPropertyFilter.PropertyType.Artist);
		_propertyMap.Add("F", CardPropertyFilter.PropertyType.Flavor);
		_propertyMap.Add("FLAVOR", CardPropertyFilter.PropertyType.Flavor);
		_propertyMap.Add("TRAIT", CardPropertyFilter.PropertyType.Trait);
		_propertyMap.Add("REBALANCED", CardPropertyFilter.PropertyType.Rebalanced);
	}

	public CardMatcher(string input)
	{
		try
		{
			_tokenStream = Tokenize(input);
			_astRoot = Parse();
		}
		catch (Exception ex)
		{
			Error("Exception: " + ex);
		}
	}

	public CardMatcher(string input, ICardDatabaseAdapter cardDatabase)
	{
		_cardDatabase = cardDatabase;
		try
		{
			_tokenStream = Tokenize(input);
			_astRoot = Parse();
		}
		catch (Exception ex)
		{
			Error("Exception: " + ex);
		}
	}

	public void SetMetadata(CardMatcherMetadata metadata)
	{
		_metadata = metadata;
	}

	public CardFilterGroup Matches(CardFilterGroup cards)
	{
		return _astRoot.Evaluate(cards, _metadata);
	}

	private void Error(string error)
	{
		_errors.Add(error);
	}

	private List<Token> Tokenize(string input)
	{
		input = input.ToUpper();
		bool flag = false;
		int num = -1;
		List<Token> list = new List<Token>();
		bool flag2 = true;
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			bool flag3 = char.IsLetter(c) || char.IsDigit(c) || c == '+' || c == '\'' || (!flag2 && c == '-');
			char cNext = ((i + 1 < input.Length) ? input[i + 1] : '\0');
			flag2 = char.IsWhiteSpace(c);
			if (c == '"')
			{
				if (flag)
				{
					string word = input.Substring(num, i - num);
					list.Add(OrIdToken(word));
					flag = false;
				}
				num = i + 1;
				int num2 = input.IndexOf('"', num);
				if (num2 == -1)
				{
					num2 = input.Length;
				}
				string value = input.Substring(num, num2 - num);
				list.Add(new Token(TokenType.Id, value));
				i = num2 + 1;
				continue;
			}
			if (flag3)
			{
				if (!flag)
				{
					flag = true;
					num = i;
				}
				continue;
			}
			if (flag)
			{
				string word2 = input.Substring(num, i - num);
				list.Add(OrIdToken(word2));
				flag = false;
			}
			var (token, num3, text) = OpToken(c, cNext);
			if (token.HasValue)
			{
				list.Add(token.Value);
			}
			i += num3;
			if (text != null)
			{
				_errors.Add(text);
			}
		}
		if (flag)
		{
			string word3 = input.Substring(num, input.Length - num);
			list.Add(OrIdToken(word3));
		}
		return list;
	}

	private static Token OrIdToken(string word)
	{
		if (!(word == "OR"))
		{
			return new Token(TokenType.Id, word);
		}
		return new Token(TokenType.Or);
	}

	private void Match(TokenType t)
	{
		if (_tokenStream.Count == 0 || _tokenStream[0].Type != t)
		{
			Error("Expected " + t);
		}
		else
		{
			_tokenStream.RemoveAt(0);
		}
	}

	private void Consume()
	{
		_tokenStream.RemoveAt(0);
	}

	private bool NextIs(params TokenType[] tokens)
	{
		if (_tokenStream.Count > 0)
		{
			return tokens.Contains(_tokenStream[0].Type);
		}
		return false;
	}

	private Token GetCurrentToken()
	{
		if (_tokenStream.Count <= 0)
		{
			return new Token(TokenType.None);
		}
		return _tokenStream[0];
	}

	private ICardMatcherReq Parse()
	{
		ICardMatcherReq result = TopLevelReqs();
		if (_tokenStream.Count != 0)
		{
			Error("Still tokens remaining");
		}
		return result;
	}

	private ICardMatcherReq TopLevelReqs()
	{
		if (NextIs(TokenType.LParen, TokenType.Id, TokenType.Minus, TokenType.Trait))
		{
			AndReqs andReqs = AndReqs();
			OrReqs orReqs = JoinedMultiWordNickname(_allAnyTexts, _cardDatabase);
			if (orReqs != null)
			{
				orReqs.Reqs.Add(new ReqTerm
				{
					AndList = andReqs
				});
				return orReqs;
			}
			return andReqs;
		}
		return new AndReqs();
	}

	private AndReqs AndReqs()
	{
		if (NextIs(TokenType.LParen, TokenType.Id, TokenType.Minus, TokenType.Trait))
		{
			OrReqs item = OrReqs();
			AndReqs andReqs = AndReqs();
			andReqs.Reqs.Insert(0, item);
			return andReqs;
		}
		return new AndReqs();
	}

	private OrReqs OrReqs()
	{
		if (NextIs(TokenType.LParen, TokenType.Id, TokenType.Minus, TokenType.Trait))
		{
			ReqTerm item = ReqTerm();
			OrReqs orReqs = ReqTail();
			orReqs.Reqs.Insert(0, item);
			return orReqs;
		}
		Error("OrReqs expects ID or LParen");
		return null;
	}

	private ReqTerm ReqTerm()
	{
		if (NextIs(TokenType.LParen))
		{
			Match(TokenType.LParen);
			AndReqs andList = AndReqs();
			Match(TokenType.RParen);
			return new ReqTerm
			{
				AndList = andList
			};
		}
		if (NextIs(TokenType.Id, TokenType.Minus, TokenType.Trait))
		{
			CardPropertyFilter req = ParseReq();
			return new ReqTerm
			{
				Req = req
			};
		}
		Error("Req expects T or LParen");
		return null;
	}

	private OrReqs ReqTail()
	{
		if (NextIs(TokenType.Or))
		{
			Match(TokenType.Or);
			return OrReqs();
		}
		return new OrReqs();
	}

	private CardPropertyFilter ParseReq()
	{
		bool negate = false;
		if (NextIs(TokenType.Minus))
		{
			negate = true;
			Match(TokenType.Minus);
		}
		string text = null;
		TokenType tokenType = TokenType.None;
		string text2;
		if (NextIs(TokenType.Trait))
		{
			text2 = GetCurrentToken().Type.ToString();
			Match(TokenType.Trait);
			text = GetCurrentToken().Value;
			Match(TokenType.Id);
		}
		else
		{
			text2 = GetCurrentToken().Value;
			Match(TokenType.Id);
			if (NextIs(TokenType.Colon, TokenType.Equals, TokenType.GreaterThan, TokenType.GreaterThanOrEqual, TokenType.LessThan, TokenType.LessThanOrEqual, TokenType.NotEqual))
			{
				tokenType = GetCurrentToken().Type;
				Consume();
				text = GetCurrentToken().Value;
				Match(TokenType.Id);
			}
		}
		if (text == null)
		{
			if (_allAnyTexts == null)
			{
				_allAnyTexts = new List<string>();
			}
			_allAnyTexts.Add(text2);
			return new StringFilter
			{
				Property = CardPropertyFilter.PropertyType.AnyText,
				Value = new UnlocalizedMTGAString
				{
					Key = text2
				},
				CardDatabase = _cardDatabase,
				Negate = negate
			};
		}
		_propertyMap.TryGetValue(text2.ToUpper(), out var value);
		switch (value)
		{
		case CardPropertyFilter.PropertyType.CMC:
		case CardPropertyFilter.PropertyType.Power:
		case CardPropertyFilter.PropertyType.Toughness:
		case CardPropertyFilter.PropertyType.Loyalty:
		case CardPropertyFilter.PropertyType.Owned:
			return new NumericFilter
			{
				Property = value,
				Value = int.Parse(text),
				Negate = negate,
				Operator = tokenType
			};
		case CardPropertyFilter.PropertyType.Rarity:
		{
			CardRarity cardRarity = RarityStringToRarity(text);
			if (cardRarity != CardRarity.None)
			{
				return new NumericFilter
				{
					Property = value,
					Value = (int)cardRarity,
					Negate = negate,
					Operator = tokenType
				};
			}
			Error("Unrecognized Rarity: " + text);
			return null;
		}
		case CardPropertyFilter.PropertyType.ManaCost:
			text = ReorderManaCosts(text);
			return new StringFilter
			{
				Property = value,
				Value = new UnlocalizedMTGAString
				{
					Key = text
				},
				Negate = negate,
				CardDatabase = _cardDatabase
			};
		case CardPropertyFilter.PropertyType.Title:
		case CardPropertyFilter.PropertyType.ExpansionCode:
		case CardPropertyFilter.PropertyType.Type:
		case CardPropertyFilter.PropertyType.Text:
		case CardPropertyFilter.PropertyType.Artist:
		case CardPropertyFilter.PropertyType.Flavor:
			return new StringFilter
			{
				Property = value,
				Value = new UnlocalizedMTGAString
				{
					Key = text
				},
				Negate = negate,
				CardDatabase = _cardDatabase
			};
		case CardPropertyFilter.PropertyType.Color:
		case CardPropertyFilter.PropertyType.ColorIdentity:
			return new ColorFilter(text)
			{
				Property = value,
				Negate = negate,
				Operator = tokenType
			};
		case CardPropertyFilter.PropertyType.Trait:
			return new TraitFilter(text)
			{
				Negate = negate
			};
		default:
			Error("Unsupported Property: " + value);
			return null;
		}
	}

	private static CardRarity RarityStringToRarity(string str)
	{
		return str switch
		{
			"C" => CardRarity.Common, 
			"COMMON" => CardRarity.Common, 
			"U" => CardRarity.Uncommon, 
			"UNCOMMON" => CardRarity.Uncommon, 
			"R" => CardRarity.Rare, 
			"RARE" => CardRarity.Rare, 
			"M" => CardRarity.MythicRare, 
			"MR" => CardRarity.MythicRare, 
			"MYTHIC" => CardRarity.MythicRare, 
			_ => CardRarity.None, 
		};
	}

	private static string ReorderManaCosts(string str)
	{
		str = str.Replace("\\", "/");
		str = str.Replace("U/W", "W/U");
		str = str.Replace("B/W", "W/B");
		str = str.Replace("B/U", "U/B");
		str = str.Replace("R/U", "U/R");
		str = str.Replace("R/B", "B/R");
		str = str.Replace("G/B", "B/G");
		str = str.Replace("G/R", "R/G");
		str = str.Replace("W/R", "R/W");
		str = str.Replace("W/G", "G/W");
		str = str.Replace("U/G", "G/U");
		return str;
	}

	private static (Token? token, int extraStep, string error) OpToken(char c, char cNext)
	{
		switch (c)
		{
		case '(':
			return (token: new Token(TokenType.LParen), extraStep: 0, error: null);
		case ')':
			return (token: new Token(TokenType.RParen), extraStep: 0, error: null);
		case ':':
			return (token: new Token(TokenType.Colon), extraStep: 0, error: null);
		case '-':
			return (token: new Token(TokenType.Minus), extraStep: 0, error: null);
		case '=':
			return (token: new Token(TokenType.Equals), extraStep: 0, error: null);
		case '?':
			return (token: new Token(TokenType.Trait), extraStep: 0, error: null);
		case '>':
			if (cNext == '=')
			{
				return (token: new Token(TokenType.GreaterThanOrEqual), extraStep: 1, error: null);
			}
			return (token: new Token(TokenType.GreaterThan), extraStep: 0, error: null);
		case '<':
			if (cNext == '=')
			{
				return (token: new Token(TokenType.LessThanOrEqual), extraStep: 1, error: null);
			}
			return (token: new Token(TokenType.LessThan), extraStep: 0, error: null);
		case '!':
			if (cNext == '=')
			{
				return (token: new Token(TokenType.NotEqual), extraStep: 1, error: null);
			}
			break;
		}
		if (!char.IsWhiteSpace(c))
		{
			return (token: null, extraStep: 0, error: "Unrecognized character during tokenization: " + c);
		}
		return (token: null, extraStep: 0, error: null);
	}

	private static OrReqs JoinedMultiWordNickname(IReadOnlyList<string> anyTexts, ICardDatabaseAdapter cardDatabase)
	{
		if (anyTexts != null && anyTexts.Count >= 2)
		{
			string key = string.Join(" ", anyTexts);
			StringFilter req = new StringFilter
			{
				Property = CardPropertyFilter.PropertyType.Nickname,
				Value = new UnlocalizedMTGAString(key),
				Negate = false,
				CardDatabase = cardDatabase
			};
			return new OrReqs(new ReqTerm
			{
				Req = req
			});
		}
		return null;
	}
}
