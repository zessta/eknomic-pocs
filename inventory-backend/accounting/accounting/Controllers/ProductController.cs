using accounting.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace accounting.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        [HttpPost("create")]
        public async Task CreateProduct(ProductDTO product,Guid siteId)
        {

        }
    }
}
