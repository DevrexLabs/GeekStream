using System;
using GeekStream.Core.Commands;
using GeekStream.Core.Domain;
using NUnit.Framework;

namespace GeekStream.Tests
{
    [TestFixture]
    public class ModelTests
    {
        [Test]
        public void GetFeedByIdTest()
        {
            var model = new GeekStreamModel();
            var feed = new Feed {Title = "Feed title", LastCollected = DateTime.MinValue};
            var id = model.AddFeed(feed);
            Console.WriteLine(feed.Id);
            Assert.AreEqual(id, feed.Id);
            var collected = DateTime.Now;
            var cmd = new SetFeedsLastCollectedCommand(new[] {id}, collected);
            cmd.Execute(model);
            Assert.AreEqual(feed.LastCollected, collected);
        }
    }
}
