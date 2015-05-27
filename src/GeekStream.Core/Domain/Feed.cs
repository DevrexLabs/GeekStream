using System;
using System.Collections.Generic;
using System.Linq;

namespace GeekStream.Core.Domain
{
    [Serializable]
    public class Feed
    {

        private int _partitionId, _id;

        /// <summary>
        /// Unique id within the partition
        /// </summary>
        public int Id
        {
            get { return _id; }
            set
            {
                if (value < 0 || value > UInt16.MaxValue) throw new ArgumentException();
                _id = value;
            }
        }

        public int PartitionId
        {
            get { return _partitionId; }
            set
            {
                if (value < 0 || value > UInt16.MaxValue) throw new ArgumentException();
                _partitionId = value;
            }
        }

        public int LongId
        {
            get { return PartitionId << 16 | Id; }
        }

        /// <summary>
        /// Title as displayed on the blog site
        /// </summary>
        public string Title { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public string Url { get; set; }

        public DateTime Created { get; set; }
        
        /// <summary>
        /// Last time we checked for new items
        /// </summary>
        public DateTime LastCollected { get; set; }
        
        /// <summary>
        /// Last time an item was added 
        /// </summary>
        public DateTime? LastItemPublished
        {
            get
            {
                if (Items.Count == 0) return null;
                return Items.Last().Published.LocalDateTime;
            }
        }

        /// <summary>
        /// Chronological list of entries
        /// </summary>
        public List<FeedItem> Items { get; set; }

        public Feed()
        {
            Items = new List<FeedItem>();
        }

        internal void AddItem(FeedItem item)
        {
            item.Feed = this;
            item.Id = Items.Count + 1;
            Items.Add(item);
        }


        public FeedItem GetItemById(int id)
        {
            return Items[id - 1];
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode();
        }

    }
}
