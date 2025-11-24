using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;

namespace Mhyrenz_Interface.Database.Services
{
    public class SalesRecordDataService : GenericDataService<SalesRecord>, ISalesRecordDataService
    {
        private readonly InventoryDbService _context;
        public SalesRecordDataService(InventoryDbService context) : base(context)
        {
            _context = context;
        }

        public override async Task<SalesRecord> Get(int id)
        {
            return await Task.Run(() =>
            {
                var record = GetTable().FindById(id);

                if (record != null)
                    LoadSession(record);

                return record;
            });
        }

        public override async Task<IEnumerable<SalesRecord>> GetAll()
        {
            return await Task.Run(() =>
            {
                var list = GetTable().FindAll().ToList();

                foreach (var record in list)
                    LoadSession(record);

                return list;
            });
        }

        private void LoadSession(SalesRecord record)
        {
            if (record == null) return;

            var context = _context.Instance;
            var sessionCol = context.GetCollection<Session>(typeof(Session).TableName());
            var trxCol = context.GetCollection<Transaction>(typeof(Transaction).TableName());

            // Load the related Session
            var session = sessionCol.FindById(record.SessionId);

            if (session != null)
            {
                // Load transactions for this session
                session.Transactions = trxCol.Query()
                    .Where(t => t.SessionId == session.UniqueId)
                    .ToList();

                record.Session = session;
            }

        }
    }
}
