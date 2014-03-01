using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Threading.Tasks;

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

            public SourceCollectedEventArgs(T source)
            {
                Source = source;
            }
        }



        public EventHandler<SyndicationItemEventArgs> ItemCollected = delegate { };
        public EventHandler<SourceCollectedEventArgs> SourceCollected = delegate { };

        private T[] _sources;
        private Func<T, String> _urlSelector;

        public FeedCollector(IEnumerable<T> sources, Func<T, string> urlSelector)
        {
            _sources = sources.ToArray();
            _urlSelector = urlSelector;
        }


        public void Collect()
        {
            Parallel.ForEach(_sources, CollectSingleSource);
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
            try
            {
                var syndicationFeed = GetFeed(_urlSelector.Invoke(source));
                if (syndicationFeed != null)
                {
                    Console.WriteLine(syndicationFeed.Title.Text);
                    lock (this)
                    {
                        foreach (var item in syndicationFeed.Items)
                        {
                            ItemCollected.Invoke(this, new SyndicationItemEventArgs(item, syndicationFeed, source));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("! " + ex.Message);
            }
            finally
            {
                lock(this) SourceCollected.Invoke(this, new SourceCollectedEventArgs(source));
            }
        }
    }
}
