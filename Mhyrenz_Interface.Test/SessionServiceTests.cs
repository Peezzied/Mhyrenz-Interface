using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using NUnit.Framework;

namespace Mhyrenz_Interface.Test
{
    public class SessionServiceTests : DatabaseTest
    {
        private ISessionService _service;

        protected override void OnSetup()
        {
            _service = new SessionService(Factory,
                new CheckoutService(Factory),
                new ProductService(Factory),
                new DatabaseSnapshotService(Factory, Path.Combine(AppContext.BaseDirectory, ".Mhyrenz Export")));
        }

        [Test]
        public async void RecordSession_Test()
        {
            await _service.RecordSession();
        }
    }
}
