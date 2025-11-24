using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;

namespace Mhyrenz_Interface.Database.Services
{
    public class SessionDataService : ISessionDataService
    {
        public string Name = typeof(Session).TableName();
        private readonly InventoryDbService _context;

        public SessionDataService(InventoryDbService context)
        {
            _context = context;
        }

        public async Task<Session> Update(Guid id, Session updatedEntity)
        {
            return await Task.Run(() =>
            {
                var col = _context.Instance.GetCollection<Session>(Name);

                updatedEntity.UniqueId = id;
                col.Update(updatedEntity);

                return updatedEntity;
            });
        }

        public async Task<Session> Create(Session entity)
        {
            return await Task.Run(() =>
            {
                var col = _context.Instance.GetCollection<Session>(Name);
                col.Insert(entity);
                return entity;
            });
        }

        public async Task<bool> Delete(Guid uid)
        {
            return await Task.Run(() =>
            {
                var col = _context.Instance.GetCollection<Session>(Name);
                return col.Delete(uid);
            });
        }

        public async Task<Session> Get(Guid uid)
        {
            return await Task.Run(() =>
            {
                var col = _context.Instance.GetCollection<Session>(Name);
                var session = col.FindById(uid);

                if (session != null)
                    LoadTransactions(session);

                return session;
            });
        }

        public async Task<IEnumerable<Session>> GetAll()
        {
            return await Task.Run(() =>
            {
                var col = _context.Instance.GetCollection<Session>(Name);
                var list = col.FindAll().ToList();

                foreach (var s in list)
                    LoadTransactions(s);

                return list;
            });
        }


        // -----------------------------
        // Manual relationship loading
        // -----------------------------
        private void LoadTransactions(Session session)
        {
            var trxCol = _context.Instance.GetCollection<Transaction>(typeof(Transaction).TableName());

            session.Transactions = trxCol.Query()
                .Where(t => t.SessionId == session.UniqueId)
                .ToList();
        }
    }
}
