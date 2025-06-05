using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Transactions : DomainObject, IEnumerable<Transaction>
    {
        private readonly IEnumerable<Transaction> _source;

        public Transactions(IEnumerable<Transaction> source)
        {
            _source = source ?? Enumerable.Empty<Transaction>();
        }

        // Precompute unique Ids
        private HashSet<int> uniqueIds => _source
            .Where(tx => tx != null)
            .GroupBy(tx => tx.Id)
            .Where(g => g.Count() == 1)
            .Select(g => g.Key)
            .ToHashSet();

        // Precompute unique ProductIds
        private HashSet<int> uniqueProductIds => _source
            .Where(tx => tx != null)
            .GroupBy(tx => tx.ProductId)
            .Where(g => g.Count() == 1)
            .Select(g => g.Key)
            .ToHashSet();

        // Filter transactions by unique Id and unique ProductId
        public int Count => _source
            .Where(tx => tx != null)
            .Where(tx => uniqueIds.Contains(tx.Id))
            .Where(tx => uniqueProductIds.Contains(tx.ProductId))
            .Count();


        public IEnumerator<Transaction> GetEnumerator() => _source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
