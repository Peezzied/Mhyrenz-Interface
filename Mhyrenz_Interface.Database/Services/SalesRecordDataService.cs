using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database.Services
{
    public class SalesRecordDataService : GenericDataService<SalesRecord>, ISalesRecordDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;
        public SalesRecordDataService(InventoryDbContextFactory contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override async Task<SalesRecord> Get(int id)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var record = GetTable().FindById(id);

                    if (record != null)
                        LoadSession(context, record);

                    return record;
                }
            });
        }

        public override async Task<IEnumerable<SalesRecord>> GetAll()
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var list = GetTable().FindAll().ToList();

                    foreach (var record in list)
                        LoadSession(context, record);

                    return list;
                }
            });
        }

        private void LoadSession(ILiteDatabase context, SalesRecord record)
        {
            if (record == null) return;

            var sessionCol = context.GetCollection<Session>(nameof(Session).TableName());
            var trxCol = context.GetCollection<Transaction>(nameof(Transaction).TableName());

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
