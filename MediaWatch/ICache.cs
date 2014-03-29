namespace MediaWatch
{
	public interface ICache
	{
		int CacheHits { get; set; }

		int CacheMisses { get; set; }

		string StatsMessage();
	}
}
