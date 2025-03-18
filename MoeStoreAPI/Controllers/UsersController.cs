using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoeStore.Entities.DB;
using MoeStore.Entities.Models.DTO;
namespace MoeStoreAPI.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext db;

        public UsersController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // allow admin to read available users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await db.Users
                .OrderByDescending(u => u.Id)
                .ToListAsync();

            List<UserProfileDto> userProfiles = new List<UserProfileDto>();

            foreach (var user in users)
            {
                var userProfileDto = new UserProfileDto()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };

                userProfiles.Add(userProfileDto);
            }

            return Ok(userProfiles);
        }
    }
}
