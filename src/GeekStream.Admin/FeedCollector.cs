using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.ServiceModel.Syndication;
using System.Xml;
using GeekStream.Admin;
using System.Threading.Tasks;
using System.Reflection;

namespace GeekStream.Admin
{
    public class FeedCollector<T>
    {
		
        public class SyndicationItemEventArgs : EventArgs
        {
            public readonly SyndicationItem SyndicationItem;
            public readonly SyndicationFeed SyndicationFeed;
            public readonly T Source;

            public SyndicationItemEventArgs(SyndicationItem item, SyndicationFeed feed, T source)
            {
                SyndicationItem = item;
                SyndicationFeed = feed;
                Source = source;
            }
        }

        public class SourceCollectedEventArgs : EventArgs
        {
            public readonly T Source;
			public readonly List<SyndicationItemEventArgs> Items;

            public SourceCollectedEventArgs(T source, List<SyndicationItemEventArgs> items)
            {
	            Source = source;
	            Items = items;
            }
        }

        public EventHandler<SyndicationItemEventArgs> ItemCollected = delegate { };
		
        public EventHandler<SourceCollectedEventArgs> SourceCollected = delegate { };

        private T[] _sources;
        private Func<T, String> _urlSelector;
	    ParallelOptions _parallelOptions = new ParallelOptions();

		public FeedCollector(IEnumerable<T> sources, Func<T, string> urlSelector, int maxDegreeOfParallelism)
		{
			_parallelOptions.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            _sources = sources.ToArray();
            _urlSelector = urlSelector;
        }

        public void Collect()
        {
	        Parallel.ForEach(_sources, _parallelOptions, CollectSingleSource);
        }

        public static SyndicationFeed GetFeed(string url)
        {
            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            var reader = XmlReader.Create(url, settings);
            using (reader)
            {
                return SyndicationFeed.Load(reader);
            }

        }

        public void CollectSingleSource(T source)
        {        
			var items = new List<SyndicationItemEventArgs>();

			try
            {
                var syndicationFeed = GetFeed(_urlSelector.Invoke(source));
                if (syndicationFeed != null)
                {
                        foreach (var item in syndicationFeed.Items)
                        {
	                        var eventArgs = new SyndicationItemEventArgs(item, syndicationFeed, source);
							ItemCollected.Invoke(this, eventArgs);
							items.Add(eventArgs);
                        }
                }
            }
            catch (Exception ex)
            {
	            Console.WriteLine("!");
            }
            finally
            {
				SourceCollected.Invoke(this, new SourceCollectedEventArgs(source, items));
            }
        }
    }
}
