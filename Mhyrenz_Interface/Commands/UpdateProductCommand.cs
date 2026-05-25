using System.Threading.Tasks;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;

namespace Mhyrenz_Interface.Commands
{
    public class UpdateProductCommand : BaseAsyncCommand
    {
        private readonly IProductService _productService;

        public UpdateProductCommand(IProductService productService)
        {
            _productService = productService;
        }

        public class DTO
        {
            public int Id { get; set; }
            public Product UpdatedProduct { get; set; }
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var DTO = parameter as DTO;
            await _productService.Update(DTO.Id, DTO.UpdatedProduct);
        }
    }
}
