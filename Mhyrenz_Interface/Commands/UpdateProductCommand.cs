using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Exceptions;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Commands
{
    public class UpdateProductCommandDTO
    {
        public int Id { get; set; }
        public string PropertyName { get; set; }
        public object Value { get; set; }
    }
    public class UpdateProductCommand : BaseAsyncCommand
    {
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventroyStore;

        public UpdateProductCommand(IProductService productService, IInventoryStore inventroyStore)
        {
            _productService = productService;
            _inventroyStore = inventroyStore;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var DTO = parameter as UpdateProductCommandDTO;

            try
            {
                var updated = await _productService.Edit(DTO.Id, DTO.PropertyName, DTO.Value);
                Debug.WriteLine($"Updated product {updated.Id} from command.");
            }
            catch (InvalidPriceException)
            {

            }
            catch (NegativeException)
            {

            }
            catch (DataException)
            {
                
            }
        }
    }
}
