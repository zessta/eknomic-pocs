using accounting.DTOs;
using accounting.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace accounting.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController(IProductService productService) : ControllerBase
    {
        private readonly IProductService _productService = productService;

        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct(ProductDTO product,Guid siteId)
        {
            return Ok(await _productService.Addproduct(product, siteId));
        }
    }
}
