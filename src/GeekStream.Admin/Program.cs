using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GeekStream.Admin;
using GeekStream.Core.Views;
using LiveDomain.Core;
using GeekStream.Core.Domain;
using GeekStream.Core.Commands;
using GeekStream.Core;
using System.ServiceModel.Syndication;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Xml;

using GeekStream.Core.Queries;
using System.Reflection;
using System.Threading.Tasks;
using System.Configuration;

namespace GeekStream.Admin
{
	class Program
	{
		const int CollectorDefaultMaxDegreeOfParallelism = -1; // Unlimited
		private IEngine<GeekStreamModel> _geekStreamDb;
		private string[] _args;
		private Dictionary<int, DateTime> _collectedFeeds = new Dictionary<int, DateTime>();

		static void Main(string[] args)
		{
			new Program(args).Run();
			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}

		private void Run()
		{
			//Set up the db connection or embedded engine
			string connectionstring = ConfigurationManager.AppSettings["geekstream"];
			_geekStreamDb = ClientConfiguration.Create(connectionstring).GetClient<GeekStreamModel>();

			if (_args.Length >= 2 && _args[0] == "-a") AddUrls(_args.Skip(1));
			else if (_args.Length == 2 && _args[0] == "-o") AddFeedsFromOpml(_args[1]);
			else if (_args.Length >= 2 && _args[0] == "-c")
			{
				if (_args.Length == 4 && _args[2] == "-p")
					CollectContinously(_args[1], int.Parse(_args[3]));
				else CollectContinously(_args[1]);
			}
			else if (_args.Length == 2 && _args[0] == "-r") RemoveFeed(_args[1]);
			else if (_args.Length == 2 && _args[0] == "-f") Find(_args[1]);
			else if (_args.Length == 1 && _args[0] == "-s") ShowStatistics();
			else
			{
				Console.WriteLine("Invalid command line.");
				Console.WriteLine("usage: geekstream.admin <command> <args>");
				Console.WriteLine("add urls: -a <url> [...]");
				Console.WriteLine("add all urls from opml file: -o <url>");
				Console.WriteLine("collect from sources due: -c <pollinterval_in_minutes> [-p <MaxDegreeOfParallelism>]");
				Console.WriteLine("remove url: -r <url> or <feedid>");
				Console.WriteLine("find feed by name or description: -f  <regex>");
				Console.WriteLine("show statistics: -s");
			}

		}

		private void ShowStatistics()
		{
			var statistics = _geekStreamDb.Execute(new GetStatisticsQuery());
			Console.WriteLine("--------------------- Statistics -------------");
			Console.WriteLine("Feeds    : {0}", statistics.TotalFeeds);
			Console.WriteLine("Items    : {0}", statistics.TotalFeedItems);
			Console.WriteLine("Words    : {0}", statistics.TotalKeywords);
			Console.WriteLine("Unique   : {0}", statistics.UniqueKeywords);
			Console.WriteLine("Clicks   : {0}", statistics.TotalClicks);
		}

		private void Find(string regex)
		{
			var query = new GetFeedsByRegexQuery(regex);
			FeedView[] feeds = _geekStreamDb.Execute(query);
			Console.WriteLine("Showing {0} feeds", feeds.Length);
			foreach (var feedView in feeds)
			{
				Console.WriteLine("--------------------------------------------------------------");
				Console.WriteLine("Id:    {0}", feedView.LongId);
				Console.WriteLine("Title: {0}", feedView.Title);
				Console.WriteLine("Url:   {0}", feedView.Url);
				Console.WriteLine("--------------------------------------------------------------");
			}
		}

		private void RemoveFeed(string urlOrId)
		{
			Command<GeekStreamModel> command;
			int feedId;
			if (Int32.TryParse(urlOrId, out feedId))
			{
				command = new RemoveFeedByIdCommand(feedId);
			}
			else
			{
				command = new RemoveFeedByUrlCommand(urlOrId);
			}

			try
			{
				ExecuteCommand(command, true);
				Console.WriteLine("ok");
			}
			catch (CommandAbortedException)
			{
				Console.WriteLine("no such feed");
			}
		}

		private void AddFeedsFromOpml(string path)
		{
			List<string> urls = new List<string>();
			HtmlDocument doc = new HtmlDocument();
			doc.Load(path);
			foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//outline"))
			{
				string xmlUrl = node.GetAttributeValue("xmlUrl", "");
				if (xmlUrl != "")
				{
					urls.Add(xmlUrl);
				}
			}
			AddUrls(urls);
		}

		public Program(string[] args)
		{
			_args = args;
		}

		private void AddUrls(IEnumerable<string> urls)
		{
			foreach (var url in urls)
			{
				AddUrl(url);
			}
			//Parallel.ForEach(urls, AddUrl); //server or client is broken (not thread safe)
		}

		private void AddUrl(string url)
		{
			var query = new GetFeedByUrl(url);
			FeedView feed = _geekStreamDb.Execute(query);

			if (feed == null) AddFeed(url);
			else Console.WriteLine("[=] " + url);

		}

		private void AddFeed(string url)
		{
			try
			{
				SyndicationFeed syndicationFeed = FeedCollector<FeedView>.GetFeed(url);
				CreateFeed(syndicationFeed, url);
				Console.WriteLine("[+] " + url);
			}
			catch (Exception ex)
			{
				Console.WriteLine("[!] " + url);
				Console.WriteLine(ex.Message);
			}

		}

		/// <summary>
		/// Connect to geekstream db and get list of feeds to be collected
		/// </summary>
		private void CollectContinously(string minutes, int maxDegreeOfParallelism = CollectorDefaultMaxDegreeOfParallelism)
		{
			if (!Regex.IsMatch(minutes, @"^\d+$"))
			{
				Console.WriteLine("not a number: {0}", minutes);
				return;
			}

			var pollIntervall = TimeSpan.FromMinutes(int.Parse(minutes));

			while (true)
			{
				DateTime collectedBefore = DateTime.Now.Add(-pollIntervall);
				FeedView[] feedsToCollect = _geekStreamDb.Execute(new GetFeedsToCollectQuery(collectedBefore, 0, 20));
				if (feedsToCollect.Length == 0)
				{
					Console.Title = "No feeds to collect, waiting 1 minute";
					Thread.Sleep(TimeSpan.FromMinutes(1));
				}
				else
				{
					Console.Title = string.Format("Collecting {0} feeds", feedsToCollect.Length);

					var collector = new FeedCollector<FeedView>(feedsToCollect, feedView => feedView.Url, maxDegreeOfParallelism);
					collector.SourceCollected += SourceCollected;
					collector.Collect();
				}
			}
		}

		long _newItems;
		long _invalidItems;

		void SourceCollected(object sender, FeedCollector<FeedView>.SourceCollectedEventArgs sourceCollectedEventArgs)
		{
			//lock (_collectedFeeds)
			//    _collectedFeeds.Add(sourceCollectedEventArgs.Source.Id, DateTime.Now);

			var items = new List<AddFeedItemCommand>();
			foreach (var e in sourceCollectedEventArgs.Items)
			{
				try
				{
					FeedView feedView = e.Source;
					string[] indexKeys;
					FeedItem item = CreateFeedItem(e.SyndicationItem, out indexKeys);

					//Include feed title in search terms
					var set = new HashSet<string>(indexKeys, StringComparer.InvariantCultureIgnoreCase);
					set.UnionWith(Regex.Split(feedView.Title, @"\W+"));
					indexKeys = set.ToArray();

					if (!feedView.ItemUrls.Contains(item.Url))
					{
						items.Add(new AddFeedItemCommand(item, indexKeys, feedView.Id, DateTime.Now));
						Interlocked.Increment(ref _newItems);
					}
				}
				catch (Exception ex)
				{
					Interlocked.Increment(ref _invalidItems);
				}
			}
			var command = new AddFeedItemsCommand(sourceCollectedEventArgs.Source, items.ToArray());
			ExecuteCommand(command);

			var feed = sourceCollectedEventArgs.Source;

			var collectedCommand = new SetFeedLastCollectedCommand(sourceCollectedEventArgs.Source.Id, DateTime.Now);
			ExecuteCommand(collectedCommand, partitionId: feed.LongId & Int16.MaxValue);
			
			Console.Write("\r{0} new items and {1} invalid items           ", _newItems, _invalidItems);
		}
		
		void ExecuteCommand(Command<GeekStreamModel> command, bool retryOnTimeout = false, bool throwIfTimeout = false, int partitionId = -1)
		{
			int retryInterval = 100;
			double retryCount = 0;
			while (true)
			{
				try
				{
					if(partitionId > -1) _geekStreamDb.Execute(command);
					else _geekStreamDb.Execute(command);
					break;
				}
				catch (TimeoutException)
				{
					if (retryOnTimeout)
					{
						retryCount++;
						if (retryCount < 10)
						{
							Thread.Sleep(retryInterval * (int)Math.Pow(2, retryCount));
							continue;
						}
					}
					if (throwIfTimeout) throw;
					break;
				}
			}
		}

		private void CreateFeed(SyndicationFeed syndicationFeed, string url)
		{

			var feed = new Feed
						   {
							   PartitionId = 2,
							   Title = syndicationFeed.Title != null ? syndicationFeed.Title.Text : "Untitled feed",
							   Description = syndicationFeed.Description != null ? syndicationFeed.Description.Text : "No description",
							   ImageUrl = syndicationFeed.ImageUrl != null ? syndicationFeed.ImageUrl.ToString() : null,
							   LastIndexed = DateTime.MinValue,
							   Url = url,
							   Created = DateTime.Now
						   };
			var command = new AddFeedCommand(feed);
			_geekStreamDb.Execute(command);


		}

		private static FeedItem CreateFeedItem(SyndicationItem syndicationItem, out string[] searchWords)
		{
			FeedItem newFeedItem = new FeedItem();

			try
			{
				var link = syndicationItem.Links.FirstOrDefault(l => l.RelationshipType == "alternate")
						   ?? syndicationItem.Links.FirstOrDefault(l => l.RelationshipType == "self")
						   ?? syndicationItem.Links.First();
				newFeedItem.Url = link.Uri.ToString();
			}
			catch (Exception ex)
			{
				throw new Exception("Syndication item has no links", ex);
			}

			newFeedItem.Published = syndicationItem.PublishDate;

			var builder = new SyndicationItemParser(syndicationItem);
			newFeedItem.Summary = builder.Summary;
			searchWords = builder.Keywords;

			newFeedItem.Title = syndicationItem.Title.Text;
			return newFeedItem;

		}
	}
}
