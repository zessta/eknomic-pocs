using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        public UserManagementController(InventoryDbContext context)
        {
            _context = context;
        }

        //[HttpPost("create")]
        //public async Task CreateUser(User user)
        //{
        //    await _context.Users.AddAsync(new User { Name = user.Name });
        //    await _context.SaveChangesAsync();
        //}
    }
}
