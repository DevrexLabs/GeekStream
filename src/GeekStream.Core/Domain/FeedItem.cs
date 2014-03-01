using System;
using GeekStream.Core.Domain;

namespace GeekStream.Core
{
    [Serializable]
    public class FeedItem
    {
        internal int Id { get; set; }

        public Int64 LongId
        {
            get
            {
                //when in doubt, parenthesize
                return (((Int64)Feed.Id) << 32) | ((UInt32)Id);
            }
        }

        public string Title { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
        public DateTimeOffset Published { get; set; }
        internal Feed Feed{ get; set; }
    }
}
