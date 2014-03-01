using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GeekStream.Core.Views;
using OrigoDB.Core;

namespace GeekStream.Core.Domain
{
    [Serializable]
    public class GeekStreamModel : Model
    {
        const int MaxSearchResults = 1000;
        const int NumberOfRecentEntries = 10;

        private HighscoreList<FeedItem> _mostRecentItems;
        private HighscoreList<Feed> _mostRecentFeeds; 

        // feed id is always = index + 1. never remove, just set feed slot to null.
        private List<Feed> _feeds;

        private Dictionary<string, HashSet<IndexEntry>> _searchIndex;

        private Statistics _statistics;

        public GeekStreamModel()
        {
            _feeds = new List<Feed>();
            _mostRecentItems = new HighscoreList<FeedItem>((a, b) => b.Published.CompareTo(a.Published),
                                                           NumberOfRecentEntries);
            _mostRecentFeeds = new HighscoreList<Feed>((a,b) => (b.LastItemPublished ?? DateTime.MinValue).CompareTo(a.LastItemPublished));
            _searchIndex = new Dictionary<string, HashSet<IndexEntry>>(StringComparer.InvariantCultureIgnoreCase);
            _statistics  = new Statistics();
        }

        public Statistics GetStatistics()
        {
            return _statistics;
        }


        public IEnumerable<Feed> MostRecentlyModifiedFeeds()
        {
            return _mostRecentFeeds;
        }

        public IEnumerable<FeedItem> GetMostRecentItems()
        {
            return _mostRecentItems;
        }

        public int AddFeed(Feed feed)
        {
            feed.Id = _feeds.Count + 1;
            _feeds.Add(feed);
            _statistics.TotalFeeds++;
            return feed.Id;
        }

        public void AddItem(FeedItem item, string[] searchTerms, int feedId, DateTime collected)
        {
            //Find the feed and add to its entries
            Feed feed = _feeds[feedId - 1];
            feed.AddItem(item);
            _statistics.TotalFeedItems++;

            //Most recent list
            _mostRecentItems.Add(item);

            //Update search index
            foreach (string keyword in searchTerms)
            {
                if (!_searchIndex.ContainsKey(keyword))
                {
                    _statistics.UniqueKeywords++;
                    _searchIndex[keyword] = new HashSet<IndexEntry>(new IndexEntryComparer());
                }
                _statistics.TotalKeywords++;
                _searchIndex[keyword].Add(new IndexEntry(keyword) { Item = item });
            }
        }

        public FeedItem GetItemByLongId(Int64 id)
        {
            var feedId = (int)(id >> 32);
            var itemId = (int)id;
            return _feeds[feedId - 1].GetItemById(itemId);
        }

        public IEnumerable<IndexEntry> Search(string query, out int totalResults)
        {
            HashSet<IndexEntry> result = null;
            foreach (string searchTerm in ParseQuery(query))
            {
                HashSet<IndexEntry> currentSearchTermHits;
                if (!_searchIndex.TryGetValue(searchTerm, out currentSearchTermHits))
                {
                    //the current term does not exist in the index.
                    //The total result will always be an empty set so we can exit immediately
                    totalResults = 0;
                    return Enumerable.Empty<IndexEntry>();
                }
                if (result == null)
                {
                    result = new HashSet<IndexEntry>(currentSearchTermHits, new IndexEntryComparer());
                }
                else
                    result.IntersectWith(currentSearchTermHits);
            }
            if (result == null)
            {
                totalResults = 0;
                return Enumerable.Empty<IndexEntry>();
            }
            totalResults = result.Count;

            return result.Take(MaxSearchResults);
        }

        public IEnumerable<FeedItem> GetEntriesByFeedId(int feedId)
        {
            return _feeds[feedId - 1].Items;
        }

        public IEnumerable<Feed> GetFeeds()
        {
            foreach (var feed in _feeds)
            {
                if(feed != null) yield return feed;
            }
        }

        public IEnumerable<Feed> GetFeedsCollectedBefore(DateTime collectedBefore)
        {
            foreach (var feed in GetFeeds().Where(f => f.LastCollected <= collectedBefore))
            {
                yield return feed;
            }
        }

        private IEnumerable<string> ParseQuery(string searchQuery)
        {
            foreach (Match match in Regex.Matches(searchQuery, @"\w+"))
            {
                yield return match.Value;
            }
        }

        public Feed GetFeedByUrl(string url)
        {
            return GetFeeds().SingleOrDefault(f => f.Url == url);
        }

        public Feed GetFeedById(int feedId)
        {
            return _feeds[feedId - 1];
        }

        public bool TryGetFeedById(int id, out Feed feed)
        {
            feed = null;
            bool result = false;
            if (id > 0 && id <= _feeds.Count)
            {
                feed = GetFeedById(id);
                result = feed != null;
            }
            return result;
        }

        public bool RemoveFeedById(int id)
        {
            Feed feedToRemove;
            bool feedExists = TryGetFeedById(id, out feedToRemove);

            if (feedExists)
            {
                _statistics.TotalFeeds--;
                _statistics.TotalFeedItems -= feedToRemove.Items.Count;
                _feeds[id-1] = null;

                _mostRecentItems.RemoveItems(feedToRemove.Items);
                RemoveFeedFromSearchIndex(feedToRemove);
            }

            return feedExists;
        }

        private void RemoveFeedFromSearchIndex(Feed feed)
        {
            var searchTermsToRemove = new List<string>();
            var itemsToRemove = new HashSet<FeedItem>(feed.Items);

            foreach (KeyValuePair<string, HashSet<IndexEntry>> kvp in _searchIndex)
            {
                var searchTerm = kvp.Key;
                var entrySet = kvp.Value;
                
                _statistics.TotalKeywords -= entrySet.RemoveWhere(entry => itemsToRemove.Contains(entry.Item));
                if (entrySet.Count == 0) searchTermsToRemove.Add(searchTerm);
            }
            _statistics.UniqueKeywords -= searchTermsToRemove.Count;

            foreach (string searchTerm in searchTermsToRemove) _searchIndex.Remove(searchTerm);
        }

        public IEnumerable<FeedView> GetFeedsByRegex(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return GetFeeds()
                .Where(f => regex.IsMatch(f.Title) || regex.IsMatch(f.Description))
                .Select(f => new FeedView(f));

        }

        public bool RemoveFeedByUrl(string url)
        {
            Feed feed = GetFeedByUrl(url);
            if (feed != null) return RemoveFeedById(feed.Id);
            return false;
        }

        internal void SetFeedsLastCollected(int[] feedIds, DateTime when)
        {
            foreach (var feedId in feedIds)
            {
                Feed feed;
                if (TryGetFeedById(feedId, out feed)) feed.LastCollected = when;
            }
        }
    }
}
