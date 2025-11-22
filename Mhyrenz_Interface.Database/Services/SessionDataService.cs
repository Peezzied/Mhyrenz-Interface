using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Database.Services
{
    public class SessionDataService: ISessionDataService
    {
        public string Name = nameof(Session).TableName();
        private readonly InventoryDbContextFactory _contextFactory;

        public SessionDataService(InventoryDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Session> Update(Guid id, Session updatedEntity)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Session>(Name);

                    updatedEntity.UniqueId = id;
                    col.Update(updatedEntity);

                    return updatedEntity;
                }
            });
        }

        public async Task<Session> Create(Session entity)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Session>(Name);
                    col.Insert(entity);
                    return entity;
                }
            });
        }

        public async Task<bool> Delete(Guid uid)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Session>(Name);
                    return col.Delete(uid);
                }
            });
        }

        public async Task<Session> Get(Guid uid)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Session>(Name);
                    var session = col.FindById(uid);

                    if (session != null)
                        LoadTransactions(context, session);

                    return session;
                }
            });
        }

        public async Task<IEnumerable<Session>> GetAll()
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Session>(Name);
                    var list = col.FindAll().ToList();

                    foreach (var s in list)
                        LoadTransactions(context, s);

                    return list;
                }
            });
        }


        // -----------------------------
        // Manual relationship loading
        // -----------------------------
        private void LoadTransactions(ILiteDatabase context, Session session)
        {
            var trxCol = context.GetCollection<Transaction>(nameof(Transaction).TableName());

            session.Transactions = trxCol.Query()
                .Where(t => t.SessionId == session.UniqueId)
                .ToList();
        }
    }
}
