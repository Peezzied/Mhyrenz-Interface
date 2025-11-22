using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Session
    {
        [BsonId] public Guid UniqueId { get; set; }
        public DateTime Period { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
    }
}
