using System.Collections.Generic;
using System.Linq;
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

        public override SalesRecord Get(object id)
        {
            var record = GetTable().FindById((dynamic)id);

            if (record != null)
                LoadSession(record);

            return record;
        }

        public override IEnumerable<SalesRecord> GetAll()
        {
            var list = GetTable().FindAll().ToList();

            foreach (var record in list)
                LoadSession(record);

            return list;
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
                    .Where(t => t.SessionId == session.Id)
                    .ToList();

                record.Session = session;
            }

        }
    }
}
