using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Models.Snapshots;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface IDatabaseSnapshotService
    {
        Task ExportSnapshot(Session session, bool isBackup = false);
        Task RestoreSnapshot(DatabaseSnapshot snapshot);
    }
}
