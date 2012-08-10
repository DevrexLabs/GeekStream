using System;
using System.Collections.Generic;
using System.Linq;
using LiveDomain.Core;
using System.Text.RegularExpressions;
using GeekStream.Core.Views;

namespace GeekStream.Core.Domain
{
    [Serializable]
    public class GeekStreamModel : Model
    {
        const int NumPopularFeeds = 10;
        const int MaxSearchResults = 1000;
        const int NumberOfRecentEntries = 10;

        private HighscoreList<FeedItem> _mostRecentItems;


        // feed id is always = index + 1. never remove, just set feed slot to null.
        private List<Feed> _feeds;

        private Dictionary<string, HashSet<IndexEntry>> _searchIndex;
        private HighscoreList<Feed> _popularFeeds;
        private HighscoreList<FeedItem> _popularItems;

        public GeekStreamModel()
        {
            _feeds = new List<Feed>();
            _mostRecentItems = new HighscoreList<FeedItem>((a, b) => b.Published.CompareTo(a.Published),
                                                           NumberOfRecentEntries);
            _popularFeeds = new HighscoreList<Feed>((a, b) => Math.Sign(b.Clicks - a.Clicks), NumPopularFeeds);
            _searchIndex = new Dictionary<string, HashSet<IndexEntry>>(StringComparer.InvariantCultureIgnoreCase);
            _popularItems = new HighscoreList<FeedItem>((a, b) => Math.Sign(b.Clicks - a.Clicks));

        }

        public Statistics GetStatistics()
        {
            Statistics stats = new Statistics();

            long totalClicks = 0, totalItems = 0;
            foreach (var feed in GetFeeds())
            {
                totalClicks += feed.Clicks;
                totalItems += feed.Items.Count;
            }

            stats.TotalClicks = totalClicks;
            stats.TotalFeedItems = totalItems;
            stats.TotalFeeds = _feeds.Count;
            stats.TotalKeywords = _searchIndex.Sum(kvp => kvp.Value.Count);
            stats.UniqueKeywords = _searchIndex.Count;

            return stats;
        }


        public IEnumerable<Feed> PopularFeeds()
        {
            return _popularFeeds;
        }

        public IEnumerable<FeedItem> PopularItems()
        {
            return _popularItems;
        }

        public IEnumerable<FeedItem> GetMostRecentItems()
        {
            return _mostRecentItems;
        }

        public int AddFeed(Feed feed)
        {
            feed.Id = _feeds.Count + 1;
            _feeds.Add(feed);
            return feed.Id;
        }

        public void AddItem(FeedItem item, string[] searchTerms, int feedId, DateTime collected)
        {
            //Find the feed and add to its entries
            Feed feed = _feeds[feedId - 1];
            feed.LastIndexed = collected;
            feed.AddItem(item);

            //Most recent list
            _mostRecentItems.Add(item);

            //Update search index
            foreach (string keyword in searchTerms)
            {
                if (!_searchIndex.ContainsKey(keyword))
                {
                    _searchIndex[keyword] = new HashSet<IndexEntry>(new IndexEntryComparer());
                }
                _searchIndex[keyword].Add(new IndexEntry(keyword) { Item = item });
            }
        }

        public FeedItem GetItemByLongId(Int64 id)
        {
            var feedId = (int)(id >> 32);
            var itemId = (int)id;
            return _feeds[feedId - 1].GetItemById(itemId);
        }

        public IEnumerable<IndexEntry> Search(string query, out int totalResults, bool orderByClicks = false)
        {
            HashSet<IndexEntry> result = null;
            foreach (string searchTerm in ParseQuery(query))
            {
                HashSet<IndexEntry> set;
                if (!_searchIndex.TryGetValue(searchTerm, out set))
                {
                    totalResults = 0;
                    return Enumerable.Empty<IndexEntry>();
                }
                if (result == null)
                {
                    result = new HashSet<IndexEntry>(set, new IndexEntryComparer());
                }
                else
                    result.IntersectWith(set);
            }
           
            totalResults = result.Count;
            if (orderByClicks)
            {
                return result.Take(MaxSearchResults).OrderByDescending(ie => ie.Item.Clicks);
            }
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
            foreach (var feed in GetFeeds().Where(f => f.LastIndexed <= collectedBefore))
            {
                yield return feed;
            }
        }

        public void Click(Int64 feedItemId, string searchQuery)
        {
            var itemClicked = GetItemByLongId(feedItemId);
            itemClicked.Clicks++;
            itemClicked.Feed.Clicks++;
            _popularFeeds.Add(itemClicked.Feed);
            _popularItems.Add(itemClicked);

            //IEnumerable<string> searchTerms = ParseQuery(searchQuery);
            //foreach (var searchTerm in searchTerms)
            //{
            //    HashSet<IndexEntry> set;
            //    if (_searchIndex.TryGetValue(searchTerm, out set))
            //    {
            //        set.Single(ie => ie.Item.LongId == itemClicked.LongId).Clicks++;
            //    }
            //}
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
            if (id <= _feeds.Count)
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
                _feeds[id-1] = null;

                _popularFeeds.Remove(feedToRemove);
                _mostRecentItems.RemoveItems(feedToRemove.Items);
                _popularItems.RemoveItems(feedToRemove.Items);
                
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
                
                entrySet.RemoveWhere(entry => itemsToRemove.Contains(entry.Item));
                if (entrySet.Count == 0) searchTermsToRemove.Add(searchTerm);
            }

            foreach (string searchTerm in searchTermsToRemove) _searchIndex.Remove(searchTerm);
        }

        public IEnumerable<FeedView> GetFeedsByRegex(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return GetFeeds()
                .Where(f => regex.IsMatch(f.Title) || regex.IsMatch(f.Description))
                .Select(f => new FeedView(f));

        }

        public void SetFeedLastCollected(int feedId, DateTime when)
        {
            try
            {
                GetFeedById(feedId).LastIndexed = when;
            }
            catch (NullReferenceException)
            {
                throw new ArgumentException("no such feed, id: " + feedId);
            }
            
        }

        public bool RemoveFeedByUrl(string url)
        {
            Feed feed = GetFeedByUrl(url);
            if (feed != null) return RemoveFeedById(feed.Id);
            return false;
        }
    }
}
