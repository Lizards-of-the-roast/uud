namespace Epic.OnlineServices.Logging;

public enum LogCategory
{
	Core = 0,
	Auth = 1,
	Friends = 2,
	Presence = 3,
	UserInfo = 4,
	HttpSerialization = 5,
	Ecom = 6,
	P2P = 7,
	Sessions = 8,
	RateLimiter = 9,
	Analytics = 11,
	Messaging = 12,
	Connect = 13,
	Overlay = 14,
	AllCategories = int.MaxValue
}
