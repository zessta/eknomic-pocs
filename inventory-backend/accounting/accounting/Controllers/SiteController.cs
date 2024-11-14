using accounting.DTOs;
using accounting.Enums;
using accounting.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace accounting.Controllers
{
    [Route("api/site")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        private readonly ISiteService _siteService;
        public SiteController(ISiteService siteService)
        {
            _siteService = siteService;
        }

        [HttpGet("types")]
        public IActionResult GetSiteTypes()
        {
            var siteTypes = Enum.GetValues(typeof(SiteType)).Cast<SiteType>().Select(x => new
            {
                Name = x.ToString(),
                Value = (int)x
            });

            return Ok(siteTypes);
        }

        [HttpPost("onboard")]
        public async Task OnBoardSite(SiteDTO site)
        {
            await _siteService.OnBoardSite(site);
        }
    }
}
