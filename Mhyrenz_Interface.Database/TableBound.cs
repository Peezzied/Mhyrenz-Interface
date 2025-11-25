using LiteDB;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Database.Services
{
    public class TableBound<T> where T : class
    {
        private readonly InventoryDbService _context;
        public string Name = typeof(T).TableName();

        public TableBound(InventoryDbService context)
        {
            _context = context;
        }

        protected ILiteCollection<T> GetTable()
        {
            return _context.Instance.GetCollection<T>(Name);
        }
    }
}