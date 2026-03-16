using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public delegate BrowserCardHeader.BrowserCardHeaderData GetModalBrowserCardHeaderDelegate(ICardDataAdapter cardModel, Action action);
