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

        private readonly HighscoreList<FeedItem> _mostRecentItems;
        private readonly HighscoreList<Feed> _mostRecentFeeds;

        private readonly SortedDictionary<int,Feed> _feeds;
        private int _nextFeedId = 1;

        private readonly SortedDictionary<string, SortedSet<FeedItem>> _searchIndex;

        private readonly Statistics _statistics;

        public GeekStreamModel()
        {
            _feeds = new SortedDictionary<int,Feed>();
            _mostRecentItems = new HighscoreList<FeedItem>((a, b) => b.Published.CompareTo(a.Published),
                                                           NumberOfRecentEntries);
            _mostRecentFeeds = new HighscoreList<Feed>((a, b) => (b.LastItemPublished ?? DateTime.MinValue).CompareTo(a.LastItemPublished));
            _searchIndex = new SortedDictionary<string, SortedSet<FeedItem>>(StringComparer.InvariantCultureIgnoreCase);
            _statistics = new Statistics();
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
            feed.Id = _nextFeedId++;
            _feeds.Add(feed.Id, feed);
            _statistics.TotalFeeds++;
            return feed.Id;
        }

        public void AddItem(FeedItem item, string[] searchTerms, int feedId, DateTime collected)
        {
            //Find the feed and add to its entries
            Feed feed = _feeds[feedId];
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
                    _searchIndex[keyword] = new SortedSet<FeedItem>();
                }
                _statistics.TotalKeywords++;
                _searchIndex[keyword].Add(item);
            }
        }

        public FeedItem GetItemByLongId(Int64 id)
        {
            var feedId = (int)(id >> 32);
            var itemId = (int)id;
            return _feeds[feedId].GetItemById(itemId);
        }

        public IEnumerable<FeedItem> Search(string query, out int totalResults)
        {
            HashSet<FeedItem> result = null;
            foreach (string searchTerm in ParseQuery(query).Distinct())
            {
                SortedSet<FeedItem> currentSearchTermHits;
                if (!_searchIndex.TryGetValue(searchTerm, out currentSearchTermHits))
                {
                    //the current term does not exist in the index.
                    //The total result will always be an empty set so we can exit immediately
                    totalResults = 0;
                    return Enumerable.Empty<FeedItem>();
                }
                if (result == null)
                {
                    result = new HashSet<FeedItem>(currentSearchTermHits);
                }
                else
                    result.IntersectWith(currentSearchTermHits);
            }
            if (result == null)
            {
                totalResults = 0;
                return Enumerable.Empty<FeedItem>();
            }
            totalResults = result.Count;

            return result.Take(MaxSearchResults);
        }

        public IEnumerable<FeedItem> GetEntriesByFeedId(int feedId)
        {
            var feed = GetFeedById(feedId);
            if (feed != null) return feed.Items;
            return Enumerable.Empty<FeedItem>();

        }

        public IEnumerable<Feed> GetFeeds()
        {
            return _feeds.Values;
        }

        public IEnumerable<Feed> GetFeedsCollectedBefore(DateTime collectedBefore)
        {
            return GetFeeds().Where(f => f.LastCollected <= collectedBefore);
        }

        private IEnumerable<string> ParseQuery(string searchQuery)
        {
            return from Match match in Regex.Matches(searchQuery, @"\w+") select match.Value;
        }

        public Feed GetFeedByUrl(string url)
        {
            return GetFeeds().SingleOrDefault(f => f.Url == url);
        }

        public Feed GetFeedById(int feedId)
        {
            Feed feed = null;
            _feeds.TryGetValue(feedId, out feed);
            return feed;
        }

        public bool TryGetFeedById(int id, out Feed feed)
        {
            feed = GetFeedById(id);
            return feed != null;
        }

        public bool RemoveFeedById(int id)
        {
            Feed feedToRemove;
            bool feedExists = TryGetFeedById(id, out feedToRemove);

            if (feedExists)
            {
                _statistics.TotalFeeds--;
                _statistics.TotalFeedItems -= feedToRemove.Items.Count;
                _feeds.Remove(id);
                if (feedToRemove.Items.Any())
                {
                    _mostRecentItems.RemoveItems(feedToRemove.Items);
                    RemoveFeedFromSearchIndex(feedToRemove);
                }
            }

            return feedExists;
        }

        private void RemoveFeedFromSearchIndex(Feed feed)
        {
            var searchTermsToRemove = new List<string>();
            var itemsToRemove = new HashSet<FeedItem>(feed.Items);

            foreach (KeyValuePair<string, SortedSet<FeedItem>> kvp in _searchIndex)
            {
                var searchTerm = kvp.Key;
                var entrySet = kvp.Value;

                _statistics.TotalKeywords -= entrySet.RemoveWhere(feedItem => itemsToRemove.Contains(feedItem));
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
