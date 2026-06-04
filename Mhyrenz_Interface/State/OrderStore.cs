using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.State
{
    public class OrderStore : IOrderStore
    {
        public SourceCollection<int, OrderViewModel> Store { get; }
            = new SourceCollection<int, OrderViewModel>(x => x.Order.ProductId);
    }
}
