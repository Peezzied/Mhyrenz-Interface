using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Transactions : IEnumerable<Transaction>
    {
        private readonly IEnumerable<Transaction> _source;

        public Transactions(IEnumerable<Transaction> source)
        {
            _source = source ?? Enumerable.Empty<Transaction>();
        }

        public int Count => _source
            .Where(tx => tx != null)
            .Select(tx => tx.Id)
            .Distinct()
            .Count();

        public IEnumerator<Transaction> GetEnumerator() => _source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
