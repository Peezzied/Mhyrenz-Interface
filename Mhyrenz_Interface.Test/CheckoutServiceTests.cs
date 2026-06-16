using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Mhyrenz_Interface.Test
{
    [TestFixture]
    public class CheckoutServiceTests : DatabaseTest
    {
        private ICheckoutService _service;

        protected override void OnSetup()
        {
            _service = new CheckoutService(Factory);
        }
    }
}
