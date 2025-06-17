using Mhyrenz_Interface.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database
{
    public class InterfaceDbContext: DbContext
    {

        DbSet<Session> Sessions { get; set; }
        DbSet<SalesRecord> SalesRecords { get; set; }
        public InterfaceDbContext(DbContextOptions options) : base(options) { }

    }
}