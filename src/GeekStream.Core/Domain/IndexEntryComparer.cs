using System.Collections.Generic;

namespace GeekStream.Core.Domain
{
    public class IndexEntryComparer : IEqualityComparer<IndexEntry>
    {
        #region Implementation of IEqualityComparer<in IndexEntry>

        public bool Equals(IndexEntry x, IndexEntry y)
        {
            return x.Item.Equals(y.Item);
        }

        public int GetHashCode(IndexEntry obj)
        {
            return obj.Item.GetHashCode();
        }

        #endregion
    }
}