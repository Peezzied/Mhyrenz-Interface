using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;

namespace Mhyrenz_Interface.Database.Services
{
    public class SessionDataService : GenericDataService<Session>, ISessionDataService
    {
        private readonly InventoryDbService _context;
        private readonly ITransactionsDataService _transactionsDataService;

        public SessionDataService(InventoryDbService context, ITransactionsDataService transactionsDataService) : base(context)
        {
            _context = context;
            _transactionsDataService = transactionsDataService;
        }

        public override Session Get(object id)
        {
            var col = _context.Instance.GetCollection<Session>(Name);
            var session = col.FindById((dynamic)id);

            if (session != null)
                LoadTransactions(session);

            return session;
        }

        public override IEnumerable<Session> GetAll()
        {
            var list = GetTable().FindAll().ToList();

            foreach (var s in list)
                LoadTransactions(s);

            return list;
        }


        // -----------------------------
        // Manual relationship loading
        // -----------------------------
        private void LoadTransactions(Session session)
        {
            var trxCol = _context.Instance.GetCollection<Transaction>(typeof(Transaction).TableName());

            session.Transactions = _transactionsDataService.GetAll()
                .Where(t => t.SessionId == session.Id)
                .ToList();
        }
    }
}
